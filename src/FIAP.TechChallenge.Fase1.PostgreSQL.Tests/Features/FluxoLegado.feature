# language: pt
Funcionalidade: Preservar o fluxo operacional existente
  Cenário: Concluir e entregar uma ordem de serviço
    Dado que a aplicação legada está autenticada com PostgreSQL
    Quando uma ordem percorre diagnóstico, aprovação e execução dos reparos
    Então a ordem deve ser finalizada e entregue com seus valores preservados
