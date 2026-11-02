# language: pt
Funcionalidade: Gerir cadastros e preservar ordens
  Cenário: Editar cadastros sem trocar suas identidades
    Dado um cliente e um veículo cadastrados no OS Service
    Quando atualizo o nome do cliente e a placa do veículo
    Então os novos dados devem ser consultáveis com os mesmos identificadores

  Cenário: Consultar uma ordem após excluir logicamente seus cadastros
    Dado uma ordem aberta para um cliente e um veículo ativos
    Quando excluo logicamente o cliente e o veículo
    Então os cadastros ficam ocultos e a ordem mantém seu histórico
