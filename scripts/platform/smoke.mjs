import {randomUUID,randomInt,randomBytes} from 'node:crypto';
import {appendFileSync} from 'node:fs';
const endpoint=process.env.API_ENDPOINT;
if(!/^https:\/\/[a-z0-9]+\.execute-api\.us-east-1\.amazonaws\.com\/?$/.test(endpoint||''))throw new Error('Endpoint AWS invalido.');
const base=endpoint.replace(/\/$/,'');
const correlation='smoke-'+randomUUID();
let token;
async function request(method,path,body,expected=200,authenticated=true){
 const response=await fetch(base+path,{method,headers:{'content-type':'application/json','X-Correlation-Id':correlation,...(authenticated&&token?{Authorization:'Bearer '+token}:{})},...(body===undefined?{}:{body:JSON.stringify(body)}),signal:AbortSignal.timeout(30000)});
 if(response.status!==expected)throw new Error(method+' '+path+' retornou HTTP '+response.status+'; esperado '+expected);
 if(expected===204)return;
 const content=await response.text();
 return content?JSON.parse(content):undefined;
}
const pause=ms=>new Promise(resolve=>setTimeout(resolve,ms));
async function wait(path,accept){
 for(let attempt=0;attempt<90;attempt++){
  const response=await fetch(base+path,{headers:{Authorization:'Bearer '+token,'X-Correlation-Id':correlation},signal:AbortSignal.timeout(15000)});
  if(response.ok){const data=await response.json();if(accept(data))return data;}
  else if(![404,502,503,504].includes(response.status))throw new Error('Falha HTTP '+response.status+' aguardando '+path);
  await pause(2000);
 }
 throw new Error('Prazo excedido aguardando '+path);
}
for(const service of ['os','execution','billing']){
 const path='/api/'+service+'/v1/health/ready';
 let ready=false;
 for(let attempt=0;attempt<60;attempt++){
  const response=await fetch(base+path,{signal:AbortSignal.timeout(15000)});
  if(response.ok){ready=true;break;}await pause(3000);
 }
 if(!ready)throw new Error('Readiness falhou: '+service);
}
const frontend=await fetch(base+'/',{signal:AbortSignal.timeout(15000)});
if(!frontend.ok||!(frontend.headers.get('content-type')||'').includes('text/html'))throw new Error('Frontend indisponivel.');
for(const path of ['/api/os/v1/clientes','/api/execution/v1/servicos','/api/billing/v1/cobrancas'])await request('GET',path,undefined,401,false);
const credentials={usuario:'smoke-'+randomUUID().slice(0,18),senha:randomBytes(24).toString('base64url')};
const user=await request('POST','/api/os/v1/usuarios',credentials,201,false);
try{
 const login=await request('POST','/api/os/v1/usuarios/login',credentials,200,false);
 token=login.token;if(!token)throw new Error('OS nao emitiu JWT.');
 await request('GET','/api/execution/v1/servicos');await request('GET','/api/billing/v1/cobrancas');
 const digits=String(randomInt(100000000,999999999)).split('').map(Number);
 for(let length=9;length<=10;length++){const remainder=digits.reduce((sum,digit,index)=>sum+digit*(length+1-index),0)*10%11;digits.push(remainder===10?0:remainder);}
 const customer=await request('POST','/api/os/v1/clientes',{nome:'Cliente demonstracao AWS',cpf:digits.join(''),telefone:'11987654321'},201);
 const vehicle=await request('POST','/api/os/v1/veiculos',{clienteId:customer.id,placa:'SM'+String.fromCharCode(65+randomInt(26))+String(randomInt(10000)).padStart(4,'0'),marca:'Toyota',modelo:'Corolla',ano:2024},201);
 const order=await request('POST','/api/os/v1/ordens',{clienteId:customer.id,veiculoId:vehicle.id,descricaoProblema:'Validacao automatizada AWS'},201);
 const os='/api/os/v1/ordens/'+order.id;const execution='/api/execution/v1/ordens/'+order.id;
 await wait(execution,data=>data.execucao.status===1);
 const service=await request('POST','/api/execution/v1/servicos',{descricao:'Revisao smoke AWS',valorUnitario:100.25},201);
 const part=await request('POST','/api/execution/v1/pecas-insumos',{nome:'Filtro smoke AWS',codigo:'SM-'+randomUUID(),precoUnitario:20.15,quantidadeEstoque:5},201);
 await request('PUT',execution+'/iniciar-diagnostico',{});
 await request('POST',execution+'/addservico',{servicoId:service.id,quantidade:1},201);
 await request('POST',execution+'/addpecainsumo',{pecaInsumoId:part.id,quantidade:2},201);
 await request('PUT',execution+'/solicitar-aprovacao',{});
 await wait(os,data=>data.status===3);
 await request('PUT',os+'/aprovar-execucao',{},202);
 const authorized=await wait(execution,data=>data.execucao.status===4);
 await request('PUT',execution+'/servicos/'+authorized.execucao.servicos[0].id+'/concluir',{tempoGastoMinutos:45});
 await request('PUT',execution+'/finalizar',{});
 const projected=await wait(os,data=>data.status===5);
 const finished=await wait(execution,data=>data.execucao.status===5);
 const charge=await request('GET','/api/billing/v1/ordens/'+order.id+'/cobranca');
 const stock=await request('GET','/api/execution/v1/pecas-insumos/'+part.id);
 if(charge.situacao!==1||charge.valor!==140.55||finished.valorTotal!==140.55||projected.pagamento.cobrancaId!==charge.id||stock.quantidadeEstoque!==3)throw new Error('Pagamento/estoque divergentes entre os servicos.');
 const message='Smoke AWS aprovado: frontend, JWT e fluxo OS → Billing → Execution. Ordem '+order.id+'. Correlacao '+correlation+'.';
 console.log(message);
 if(process.env.GITHUB_STEP_SUMMARY)appendFileSync(process.env.GITHUB_STEP_SUMMARY,message+'\nEndpoint: '+base+'\n');
}finally{
 if(token&&user?.id)await request('DELETE','/api/os/v1/usuarios/'+user.id,undefined,204);
}
