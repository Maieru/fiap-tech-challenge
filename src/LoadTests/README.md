# Testes de carga k6

Execute os comandos nesta pasta, com a API disponível no BASE_URL informado.

| Script | Duração das etapas | Usuários simultâneos |
| --- | --- | --- |
| jornada-usuario.js | 16 minutos | início com 10, pico de 100 |
| jornada-usuario-longa.js | 60 minutos | início com 2, pico de 10 |

A versão longa reaproveita a jornada completa e os thresholds da versão original: cadastro, login, catálogo, cliente, veículo, criação da ordem, diagnóstico, aprovação, execução, finalização e entrega. Cada usuário repete a jornada durante o teste, com pausa de 1 segundo entre iterações.

```powershell
k6 run -e BASE_URL=http://localhost:8080 jornada-usuario-longa.js
```

O perfil longo sobe de 2 para 10 usuários em 5 minutos, mantém 10 por 50 minutos e reduz a zero em 5 minutos. O encerramento pode incluir o período de tolerância de 30 segundos para iterações em andamento.

Para ajustar o perfil, use START_VUS, MAX_VUS, STAGE_DURATION (subida), HOLD_DURATION e RAMP_DOWN_DURATION. SLEEP_SECONDS controla a pausa entre jornadas:

```powershell
k6 run -e BASE_URL=http://localhost:8080 -e START_VUS=1 -e MAX_VUS=5 -e HOLD_DURATION=80m -e SLEEP_SECONDS=3 jornada-usuario-longa.js
```

Esse exemplo dura 90 minutos, com pico de 5 usuários. O teto de usuários não é um limite de requisições por segundo: a vazão depende da latência da API e da pausa entre jornadas.

O teste cria registros a cada iteração. CLEANUP=false é o padrão; use `-e CLEANUP=true` para executar a limpeza prevista no final das jornadas concluídas. Jornadas interrompidas ou com falha podem deixar registros. A versão longa mantém a mesma lógica de dados e limpeza da original.

Para conferir a configuração sem enviar requisições à API:

```powershell
k6 inspect jornada-usuario-longa.js
```
