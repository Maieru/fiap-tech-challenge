# Agent Instructions

## NUnit Assertions
- Sempre que um teste tiver múltiplos asserts independentes (`Assert.That`), agrupar dentro de `Assert.Multiple(() => { ... })`.
- Evitar asserts independentes sequenciais fora de `Assert.Multiple` nesses cenários.
