import {appendFileSync} from 'node:fs';
import {approvedRun,sha} from '../delivery/delivery.mjs';
if(process.env.GITHUB_REF!=='refs/heads/main') throw new Error('Plataforma somente em main.');
if(process.env.PLATFORM_ENABLED!=='true') throw new Error('Conclua a transicao/bootstrap e configure PLATFORM_ENABLED=true.');
const operation=process.env.OPERATION;
if(!['apply','destroy'].includes(operation)) throw new Error('Operacao invalida.');
for(const name of ['INFRA_ACTION_ROLE','DATABASE_ACTION_ROLE',...(operation==='apply'?['AWS_OS_PUBLISH_ROLE_ARN','AWS_OS_DEPLOY_ROLE_ARN','AWS_EXECUTION_PUBLISH_ROLE_ARN','AWS_EXECUTION_DEPLOY_ROLE_ARN','AWS_BILLING_PUBLISH_ROLE_ARN','AWS_BILLING_DEPLOY_ROLE_ARN','JWT_ISSUER','JWT_AUDIENCE']:[])]){
 if(!process.env[name]?.trim())throw new Error('Configure a variable '+name+' no repositorio principal.');
}
if(operation==='apply'&&process.env.HAS_NEW_RELIC_LICENSE!=='true')throw new Error('Configure o secret NEW_RELIC_LICENSE_KEY.');
const names={os:'fiap-tech-challenge',execution:'fiap-tech-challenge-execution',billing:'fiap-tech-challenge-billing',infra:'fiap-tech-challenge-infra',db:'fiap-tech-challenge-db'};
const token=process.env.GH_TOKEN;
async function api(path){
 const response=await fetch('https://api.github.com/repos/Maieru/'+path,{headers:{Authorization:'Bearer '+token,Accept:'application/vnd.github+json','X-GitHub-Api-Version':'2022-11-28'},signal:AbortSignal.timeout(30000)});
 if(!response.ok) throw new Error('GitHub HTTP '+response.status+' ao resolver '+path.split('/')[0]);
 return response.json();
}
const revisions={};
for(const [key,name] of Object.entries(names)){
 const revision=sha(key==='os'?process.env.GITHUB_SHA:(await api(name+'/git/ref/heads/main')).object.sha);
 if(operation==='apply'&&['os','execution','billing'].includes(key)){
  const workflow=key==='os'?'monorepo-ci.yml':'ci.yml';
  const runs=await api(name+'/actions/workflows/'+workflow+'/runs?head_sha='+revision+'&per_page=100');
  if(!approvedRun(runs.workflow_runs,revision)) throw new Error('CI de main precisa estar aprovado: '+name+'@'+revision);
 }
 revisions[key]=revision;
 appendFileSync(process.env.GITHUB_OUTPUT,key+'_sha='+revision+'\n');
}
appendFileSync(process.env.GITHUB_STEP_SUMMARY,'Operação: **'+operation+'**. Commits selecionados:\n'+Object.entries(revisions).map(([key,value])=>'- '+names[key]+': '+value).join('\n')+'\n');
