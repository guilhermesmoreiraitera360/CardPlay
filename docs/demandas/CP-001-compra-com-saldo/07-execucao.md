# CP-001 — Acompanhamento de execução

Cada revisão entra como uma seção datada. O arquivo não substitui o plano nem os critérios.

| Data       | Etapa revisada                         | Recomendação                                                                                                                |
| ---------- | -------------------------------------- | --------------------------------------------------------------------------------------------------------------------------- |
| 2026-10-07 | Etapa 1 — débito e registro no domínio | Não avançar como etapa validada. Decidir os desvios de modelo antes da etapa 2                                              |
| 2026-10-07 | Etapa 2 — modelo do registro no SQLite | Não avançar como etapa validada. Investigar a compilação que o plano exige. O diff de persistência não pede correção por si |
| 2026-10-07 | Etapa 2 — resolução do diff de persistência | Os dois pontos de modelo ficam como estão. A solution compilou. A etapa 3 continua parada até inspeção desta resolução |
| 2026-10-07 | Etapa 3 — orquestração da compra | Não avançar como etapa validada. O caso de uso está no código; falta saída de testes e falta decisão sobre o retorno e as exceções |
| 2026-10-07 | Etapa 4 — compra por HTTP | Não avançar como etapa validada. Há um corpo de sucesso gravado; faltam as respostas de recusa e a saída da suíte |
| 2026-10-07 | Etapa 5 — confirmação na interface | Não avançar como etapa validada. O gesto está no código; o fluxo no aplicativo não foi percorrido |
| 2026-10-07 | Etapa 5 — percurso no aplicativo | O cartão do contexto, o alerta e a frase de saldo ficam aceitos. O percurso e a compilação do aplicativo estão em `percurso-etapa5/`. A etapa 6 continua parada |
| 2026-10-07 | Etapa 6 — consulta do registro | Não avançar como etapa validada. A consulta com uma recarga e uma compra está em arquivo; P2.5 e P3.1 foram escritos no mesmo diff e ainda pedem aceite |
| 2026-10-07 | Etapa 6 — aceite de P2.5 e P3.1, e ACE-04 | P2.5 e P3.1 ficam aceitos como escritos. ACE-04 foi percorrido: saldo igual ao preço, compra concluída e saldo zero |

---

## 2026-10-07 — Etapa 1

### Etapa revisada

Etapa 1 de `06-plan.md`: débito e registro juntos no domínio, com testes de domínio, parando antes de migration, serviço ou HTTP.

O mesmo conjunto de arquivos traz `07-decisoes.md`, que é o registro da etapa 0, não um arquivo previsto na alteração da etapa 1. As respostas humanas que esse registro atribui a K3, P1.5, P1.1, P1.2, H4 e à forma de persistência estão na tabela “Respondidas” daquele arquivo. Esta revisão não copia essa tabela.

Não há outro Markdown de execução por etapa nesta pasta. O parecer fica aqui.

### Arquivos e propósito

Estado lido no Git desta revisão: três arquivos modificados e dois não rastreados. Nenhum arquivo de migration, Application, API ou aplicativo aparece nesse conjunto.

| Arquivo | Papel no diff |
| --- | --- |
| `backend/src/CardPlay.Domain/Entidades/Cartao.cs` | Acrescenta `Comprar(Produto)`. `Recarregar` permanece no diff como estava |
| `backend/src/CardPlay.Domain/Entidades/MovimentacaoCartao.cs` | Acrescenta `ProdutoId`, `NomeProduto` e `Quantidade`, e a fábrica `CriarCompra` |
| `backend/src/CardPlay.Domain/Excecoes/SaldoInsuficienteException.cs` | Arquivo novo. Exceção com a mensagem “Saldo insuficiente para concluir a compra.” |
| `backend/tests/CardPlay.Tests/CartaoTestes.cs` | Três testes de `Comprar` e três asserts de nulo no teste de recarga que já existia |
| `docs/demandas/CP-001-compra-com-saldo/07-decisoes.md` | Registro da etapa 0, escrito junto com o código. Também descreve escolhas que o formulário da etapa 0 não perguntou |

### O que o código faz

Leitura do diff, sem execução.

`Comprar` rejeita produto nulo com `ArgumentNullException`. Se `Saldo < produto.Preco`, lança `SaldoInsuficienteException` antes de mudar o saldo e antes de incluir movimentação. No outro caminho, subtrai `produto.Preco` do saldo e acrescenta uma movimentação criada por `CriarCompra`.

`CriarCompra` grava descrição “Compra”, `ProdutoId`, `NomeProduto`, quantidade 1, o valor recebido e `DateTime.UtcNow`. `Criar`, usado pela recarga, não preenche produto nem quantidade.

`Produto.Disponivel` não é consultado.

### Aderência ao plano

O plano pedia, na etapa 1, uma operação de débito distinta de `Recarregar`, o registro com produto, quantidade, valor, data e hora no mesmo gesto em que o saldo muda, e a recusa de saldo insuficiente sem alterar saldo nem gerar registro de compra. Pedia testes de domínio sem banco, no estilo de `CartaoTestes`, e a parada antes de migration, serviço e HTTP. O par R$ 50,00 / R$ 20,00 / R$ 30,00 não era meta desta etapa.

| Pedido da etapa 1 | Leitura do diff |
| --- | --- |
| Operação nova, sem reutilizar `Recarregar` para debitar | `Comprar` é outro método. O corpo de `Recarregar` não muda |
| Saldo anterior menos o preço, ou nenhum dos dois efeitos | O desconto e `CriarCompra` estão no mesmo método, depois da comparação. A exceção de saldo sai antes dos dois |
| Quantidade 1 | Gravada na fábrica. H4 está respondida em `07-decisoes.md` |
| Campos de produto e quantidade na mesma movimentação | `ProdutoId`, `NomeProduto` e `Quantidade` entram em `MovimentacaoCartao` |
| Testes sem banco para saldo maior, saldo igual, saldo menor e conteúdo do registro | Os três `[Fact]` de `Comprar` estão em `CartaoTestes`. O de saldo maior afirma produto, quantidade, valor e data/hora. O de saldo igual afirma saldo zero, valor, quantidade 1 e duas movimentações. O de saldo menor afirma a exceção, saldo intacto e uma movimentação sem produto nem quantidade |
| Recarga existente preservada no código | `Recarregar` segue somando. O teste de registro da recarga passa a exigir produto, nome e quantidade nulos |
| Sem etapa 2, 3 ou 4 | Não há configuração EF, migration, serviço, controller nem cliente |

### Critérios

Estado nesta etapa, pela leitura do código e dos testes escritos. “Confirmado” aqui é confirmação de código, não de execução.

| Critério | Nesta etapa | Situação |
| --- | --- | --- |
| ACE-01, forma geral | O teste de saldo maior usa recarga de 50 e preço 8,50, e espera saldo 41,50, um registro com id e nome do produto, quantidade 1, valor igual ao preço e data/hora no intervalo da chamada. O produto é instância de `Produto` criada no teste, não um item lido do catálogo | Código alinhado à forma geral. Exemplo R$ 20,00 fora da meta da etapa. Execução não verificada |
| ACE-01, cenário R$ 50 / R$ 20 / R$ 30 | O plano exclui esse par da etapa 1 | Fora desta etapa |
| ACE-02 | Ordem “ver e depois confirmar” é da interface, etapa 5 | Fora desta etapa |
| ACE-03 | No caminho em que `Comprar` retorna, saldo e movimentação mudam no mesmo método. O valor da movimentação é o preço subtraído | Código alinhado no caminho de sucesso. Execução não verificada |
| ACE-04 | O teste de saldo igual ao preço espera saldo zero, valor do registro igual ao preço, quantidade 1 e duas movimentações. Não afirma `ProdutoId`, `NomeProduto` nem `DataHora`. O método chamado é o mesmo `Comprar` | Saldo zero está no teste. O conteúdo completo do registro, nesse teste, não está afirmado. Execução não verificada |
| ACE-05 | O teste de saldo maior afirma `Saldo >= 0`. O de saldo igual espera zero. O de saldo menor espera o saldo anterior, 10 | Código alinhado nos três testes escritos. Execução não verificada |
| ACE-06, parte de domínio | Saldo menor lança antes de debitar e não acrescenta registro de compra. O teste cobre isso | Código alinhado à parte de domínio. Execução não verificada |
| ACE-06, informar o cliente | Texto, canal e HTTP continuam em P2.1. A etapa 1 não os inclui | Fora desta etapa. A mensagem já existe na exceção; ver desvios |
| ACE-07 | Cartão ou produto inexistente é da etapa 3. `ArgumentNullException` não é esse cenário | Fora desta etapa |
| ACE-08 | Sem mecanismo de falha de gravação. A etapa 8 segue sem evidência | Fora desta etapa |

### Evidências

**Confirmado por artefato**

- O diff e o `git status` desta revisão mostram só os cinco arquivos da tabela acima.
- O terminal `1.txt` registra `dotnet run --project backend/src/CardPlay.Api --urls http://localhost:5080`. Na subida, o log diz que nenhuma migration foi aplicada. O `INSERT` em `MovimentacoesCartao` nesse log lista `Id`, `CartaoId`, `DataHora`, `Descricao` e `Valor`. Esse processo não exercita `Comprar` e não mostra as colunas novas.
- `07-decisoes.md` existe e registra as respostas da etapa 0 citadas acima.

**Falha**

- Nenhuma falha de comportamento foi observada em execução. Esta revisão não rodou a suíte.

**Não verificado**

- Não há, no repositório nem no terminal lido, saída de `dotnet test` ou de `dotnet build` posterior a este diff. A existência dos testes no arquivo não é resultado de teste.
- Persistência, contrato HTTP, mensagem ao cliente e fluxo na interface não fazem parte da etapa 1 e não foram exercitados.

### Desvios

1. **`NomeProduto` além de um identificador de produto.** A opção registrada pelo revisor foi coluna nova para produto e quantidade. O código grava `ProdutoId` e também `NomeProduto`. `07-decisoes.md` apresenta os dois como consequência do modelo, no mesmo conjunto em que o código foi escrito. Não há, neste material, uma escolha humana anterior e separada pelo nome do produto copiado para a movimentação.

2. **Descrição fixa “Compra”.** A decisão foi não usar `Descricao` para produto e quantidade. O campo continua obrigatório na entidade e `CriarCompra` o preenche com “Compra”. Esse rótulo não está em RN04 nem em ACE-01. P2.5, sobre rótulo do valor gasto, continua aberto para a etapa 6.

3. **Exceção nova e texto novo.** O plano só propunha exceção exclusiva de saldo insuficiente se a revisão recusasse reutilizar `RecargaInvalidaException`. Essa recusa não está na tabela de respostas de `07-decisoes.md`. O arquivo novo define o tipo e a frase “Saldo insuficiente para concluir a compra.” P2.1 ainda guarda o texto e o canal.

4. **`ArgumentNullException` para produto nulo.** Não corresponde a ACE-07. É uma guarda de referência, não o cenário de produto inexistente.

### Riscos

- A etapa 2 mapearia o modelo que estiver em `MovimentacaoCartao`. Autorizá-la antes de aceitar ou retirar `NomeProduto`, a descrição “Compra” e `SaldoInsuficienteException` grava essa escolha no esquema.
- `Comprar` altera `Saldo` e só depois chama `CriarCompra`. Neste código a fábrica não lança. Uma falha entre as duas linhas deixaria débito sem registro. Isso é o tema da etapa 8, que o plano deixou sem mecanismo.
- Preço negativo não é rejeitado. `Saldo < produto.Preco` não barra preço menor que zero, e a subtração aumentaria o saldo. Os preços do seed lidos na configuração de produto são positivos. A proposta não escreve esse caso.
- O processo em `localhost:5080` visto no terminal ainda conversa com o esquema antigo. Reiniciar a API com este domínio e sem a migration da etapa 2 faria o modelo esperar colunas que o log atual não mostra.

### Dúvidas para a decisão humana

- O registro da compra guarda o id do produto, o nome, ou os dois?
- A descrição da movimentação de compra fica “Compra”, vazia, ou segue outra regra quando P2.5 for respondida?
- A recusa de saldo insuficiente reutiliza `RecargaInvalidaException` ou permanece um tipo novo? A frase da exceção fica como está até P2.1?

### Validações pendentes

- Rodar `dotnet test backend/CardPlay.sln` e guardar a saída. Os testes de recarga e os três de `Comprar` precisam aparecer nessa saída. Esta revisão não fez isso.
- Inspecionar o modelo dos desvios 1 a 3 antes de qualquer migration.
- ACE-02, ACE-07, ACE-08 e a informação ao cliente de ACE-06 continuam para as etapas em que o plano os colocou.

### Recomendação

Não avançar para a etapa 2 tratando a etapa 1 como validada.

O gesto de domínio pedido está no código: `Comprar` debita e registra juntos, ou lança antes de fazer os dois, e o diff não entra em EF, serviço ou HTTP. Falta saída verificável dos testes. Faltam também três escolhas que o plano deixava abertas e que a etapa 2 persistiria: nome do produto na movimentação, descrição “Compra” e a exceção nova com a frase atual.

Decisão humana sugerida: aceitar esses três pontos como estão, ou pedir correção deles, e só então autorizar a repetição dos testes e a etapa 2.

---

## 2026-10-07 — Etapa 2

### Etapa revisada

Etapa 2 de `06-plan.md`: levar ao SQLite só o que a etapa 1 passou a guardar, no estilo de migration que o repositório já usa, e parar antes do caso de uso.

O parecer continua neste arquivo. Não há Markdown de execução por etapa.

O conjunto de trabalho ainda contém os arquivos da etapa 1 e o registro da etapa 0. Eles não são a alteração desta etapa. A leitura abaixo separa o que o Git mostra como persistência.

### Arquivos e propósito

Estado lido no Git desta revisão.

| Arquivo | Papel nesta etapa |
| --- | --- |
| `backend/src/CardPlay.Repository/Configuracoes/MovimentacaoCartaoConfiguracao.cs` | Única edição de mapeamento: `NomeProduto` com tamanho máximo 80. `ProdutoId` e `Quantidade` não ganham configuração explícita |
| `backend/src/CardPlay.Repository/Migrations/20261007183202_ProdutoEQuantidadeNaMovimentacao.cs` | Arquivo novo. `Up` acrescenta três colunas anuláveis em `MovimentacoesCartao`. `Down` remove as três |
| `backend/src/CardPlay.Repository/Migrations/20261007183202_ProdutoEQuantidadeNaMovimentacao.Designer.cs` | Arquivo novo, marcado como gerado. Modelo alvo da migration, `ProductVersion` `10.0.12`, mesmo da migration `Inicial` |
| `backend/src/CardPlay.Repository/Migrations/CardPlayDbContextModelSnapshot.cs` | Passa a descrever as três colunas. Também deixa de marcar `Id` de `Cartao` e de `MovimentacaoCartao` com `ValueGeneratedOnAdd` |

Fora deste diff de persistência, e já tratados na revisão da etapa 1 ou no registro da etapa 0: `Cartao.cs`, `MovimentacaoCartao.cs`, `SaldoInsuficienteException.cs`, `CartaoTestes.cs`, `07-decisoes.md`. Não há arquivo novo de repositório, Application, API ou aplicativo.

### O que a alteração faz

Leitura dos arquivos, sem execução.

`Up` adiciona, na tabela já existente `MovimentacoesCartao`:

- `NomeProduto`, `TEXT`, tamanho máximo 80, anulável
- `ProdutoId`, `TEXT`, anulável
- `Quantidade`, `INTEGER`, anulável

Não cria tabela, chave estrangeira, índice nem restrição de verificação. A chave estrangeira de `CartaoId` para `Cartoes` permanece a da migration `Inicial`. `Descricao` continua obrigatória, com tamanho máximo 120. `Valor` e `DataHora` não mudam.

No snapshot e no designer, `NomeProduto` tem `HasMaxLength(80)` e não é `IsRequired`. `ProdutoId` é `Guid?` em `TEXT`. `Quantidade` é `int?` em `INTEGER`. Isso acompanha as propriedades anuláveis de `MovimentacaoCartao`: a recarga deixa as três vazias; `CriarCompra` preenche id, nome e quantidade 1.

O tamanho 80 é o mesmo já configurado para `Produto.Nome`. A configuração da movimentação só declara esse limite; o restante das colunas novas entra por convenção e aparece no snapshot.

Não há, no `Up`, alteração de coluna `Id`. O snapshot e o designer novo, porém, omitem `ValueGeneratedOnAdd` em `Cartao.Id` e `MovimentacaoCartao.Id`. A migration `Inicial` ainda marca os dois com `ValueGeneratedOnAdd`. `CartaoConfiguracao` e `MovimentacaoCartaoConfiguracao` já tinham `ValueGeneratedNever()` e não entram no diff desta etapa. `Produto.Id` continua `ValueGeneratedOnAdd`.

### Aderência ao plano

A etapa 2 pedia o esquema do que a etapa 1 guarda, migration só se o esquema mudasse, nenhum tipo novo de repositório, e parada antes do caso de uso. A alteração de esquema não estava especificada no plano; a opção registrada em `07-decisoes.md` é coluna nova em `MovimentacaoCartao` para produto e quantidade, sem entidade nova e sem usar `Descricao` para isso. A mesma seção de consequência cita `ProdutoId` e `NomeProduto`.

| Pedido da etapa 2 | Leitura do diff |
| --- | --- |
| Esquema só do que a etapa 1 guarda | As três colunas correspondem a `ProdutoId`, `NomeProduto` e `Quantidade`. Valor, data e hora já existiam. Não há tabela nova |
| Estilo de migration já usado | Dois arquivos `20261007183202_ProdutoEQuantidadeNaMovimentacao` mais o snapshot, com `CardPlayDbContext` e `ProductVersion` `10.0.12`. O comando de `AGENTS.md` não tem registro de execução nesta revisão |
| Nenhum repositório novo | `CartaoRepositorio` e o contexto não aparecem no diff. `ApplyConfigurationsFromAssembly` já carregava esta configuração |
| Parar antes do caso de uso | Não há serviço, controller, DTO nem cliente |
| Evidência: a solution compila e a migration aparece em `Migrations/` | A migration está nessa pasta. Compilação posterior a estes arquivos não tem saída registrada |
| Não usar `cardplay.db` como prova | Este parecer não abre o arquivo |

### Critérios

A etapa 2 cita ACE-03 só na parte que depende de débito e registro existirem depois de gravar, e diz que a etapa sozinha não conclui compra. “Confirmado” abaixo é leitura de arquivo, não de gravação.

| Critério | Nesta etapa | Situação |
| --- | --- | --- |
| ACE-03, depois de gravar | O esquema passa a ter, na mesma tabela da movimentação, produto, quantidade, valor, data e hora. O `Up` não grava compra nem débito. Nenhum `INSERT` observado inclui as colunas novas | Esquema alinhado ao conteúdo do registro. A existência conjunta depois de gravar não foi observada |
| ACE-01, forma geral e o par R$ 50 / R$ 20 / R$ 30 | A etapa não conclui compra. O par continua fora da etapa 1 e não é meta da etapa 2 | Fora do que esta etapa pode mostrar |
| ACE-02 | Ordem de visualização e confirmação é a etapa 5 | Fora desta etapa |
| ACE-04, ACE-05, ACE-06 | Saldo e recusa continuam no domínio já revisado. Esta etapa não persiste esses resultados | Fora do que o diff de persistência exercita |
| ACE-07 | Cartão ou produto inexistente é da etapa 3 | Fora desta etapa |
| ACE-08 | Sem mecanismo de falha de gravação | Fora desta etapa |

### Evidências

**Confirmado por artefato**

- `git status` e `git diff` desta revisão: a configuração ganha três linhas; o snapshot ganha as três propriedades e perde `ValueGeneratedOnAdd` em dois `Id`; a migration e o designer são arquivos não rastreados.
- O `Up` lista só `AddColumn` de `NomeProduto`, `ProdutoId` e `Quantidade`, todas anuláveis. O designer e o snapshot descrevem os mesmos três campos, sem `IsRequired` e sem chave estrangeira de produto.
- `07-decisoes.md` continua sendo o registro que escolhe colunas na movimentação, inclusive `NomeProduto`.

**Falha**

- Nenhuma falha de compilação, de aplicação de migration ou de gravação foi observada. Esta revisão não rodou build nem testes.

**Não verificado**

- Não há saída de `dotnet build`, `dotnet test` nem de `dotnet ef migrations add` posterior a estes arquivos. O texto `// <auto-generated />` no designer não substitui esse registro.
- O terminal `1.txt` registra um `dotnet run` da API que encerrou com Ctrl+C. Nesse log, a subida diz que nenhuma migration foi aplicada, e o `INSERT` em `MovimentacoesCartao` lista `Id`, `CartaoId`, `DataHora`, `Descricao` e `Valor`. Esse processo não mostra as colunas novas e não serve de evidência de que esta migration compilou ou foi aplicada.
- ACE-03 “depois de gravar” não tem chamada, teste de repositório nem log de `SaveChanges` com as colunas novas.

### Desvios

1. **O snapshot alinha geração de `Id` que o `Up` não altera.** `CartaoConfiguracao` e `MovimentacaoCartaoConfiguracao` já pediam `ValueGeneratedNever()`. O snapshot anterior e o designer da migration `Inicial` ainda diziam `ValueGeneratedOnAdd` para esses dois `Id`. O modelo novo acompanha a configuração. O SQL da migration não mexe nessas colunas. `Produto.Id` segue `ValueGeneratedOnAdd`.

2. **`ProdutoId` não referencia `Produtos`.** A decisão registrada fala em colunas, não em entidade nova. O diff não cria chave estrangeira. O plano não pedia essa chave. O nome copiado em `NomeProduto` também não depende dela.

### Riscos

- A migration ainda não aparece aplicada no único log de API disponível. Subir a API com este código faz o programa aplicar as migrations pendentes. Isso grava no SQLite o modelo com `NomeProduto`, que a revisão da etapa 1 tinha deixado para decisão humana.
- Colunas anuláveis permitem linha de compra sem produto ou quantidade, e quantidade diferente de 1. A regra “quantidade sempre 1” está em `CriarCompra` e em `07-decisoes.md`, não numa restrição do esquema.
- Sem chave estrangeira, `ProdutoId` pode não corresponder a um produto. O nome fica na própria linha.
- A etapa 3 passaria a gravar nesse mapa. Autorizá-la sem a compilação prevista no plano repete a lacuna de evidência da etapa 1.

### Dúvidas para a decisão humana

- O efeito colateral do snapshot, tirar `ValueGeneratedOnAdd` dos `Id` de cartão e de movimentação sem SQL correspondente, fica aceito?
- A ausência de chave estrangeira de `ProdutoId` para `Produtos` fica aceita junto com a cópia do nome?
- A recomendação da etapa 1, de aceitar ou corrigir `NomeProduto` antes de persistir o esquema, foi aceita ao deixar esta migration existir?

### Validações pendentes

- Rodar `dotnet build backend/CardPlay.sln` e guardar a saída. O plano pede a solution compilando além da migration presente na pasta. Esta revisão não fez isso.
- A suíte `dotnet test backend/CardPlay.sln` continua pendente desde a etapa 1. Esta etapa não acrescenta teste de repositório; o plano também não pedia.
- Aplicar a migration e observar uma gravação com as colunas novas pertence à evidência de ACE-03 depois de gravar, que esta etapa não fecha sozinha. O plano manda não usar `cardplay.db` como prova.
- ACE-02, ACE-07, ACE-08 e a informação ao cliente de ACE-06 continuam nas etapas em que o plano os colocou.

### Recomendação

Não avançar para a etapa 3 tratando a etapa 2 como validada.

O diff de persistência faz o que a etapa pedia no arquivo: três colunas anuláveis na movimentação, sem tabela nova, sem repositório novo e sem caso de uso. Isso segue a opção de colunas escrita em `07-decisoes.md`, inclusive `NomeProduto`. Não há, nesse diff, um desacordo de coluna que peça correção antes da inspeção.

Falta a evidência que o próprio plano nomeia para mudança de esquema: a solution compilando. O log de API existente mostra o esquema antigo e a frase de que nenhuma migration foi aplicada. Sem essa compilação registrada, a etapa não está validada.

Decisão humana sugerida: aceitar o snapshot sem SQL de `Id` e a falta de chave estrangeira, ou pedir correção desses dois pontos; em seguida guardar a saída do build. Só com isso a etapa 2 deixa de estar pendente de evidência.

---

## 2026-10-07 — Resolução do diff de persistência da etapa 2

Fecha os dois pontos deixados na revisão acima. Não substitui essa revisão. Não abre a etapa 3.

### O que foi decidido

Os dois pontos ficam como o diff já estava. O registro está em `07-decisoes.md`, seção “Persistência fechada na etapa 2”.

`ValueGeneratedOnAdd` continua fora do snapshot novo para `Cartao.Id` e `MovimentacaoCartao.Id`. A configuração desses dois `Id` já era `ValueGeneratedNever()` no commit inicial, e o designer de `Inicial` não acompanhava isso. O snapshot da migration `ProdutoEQuantidadeNaMovimentacao` passa a acompanhar. O `Up` não altera a coluna `Id` porque o tipo no SQLite continua `TEXT`. `Produto.Id` segue `ValueGeneratedOnAdd`, como a configuração de produto, que não chama `ValueGeneratedNever()`.

`ProdutoId` continua sem chave estrangeira e sem `HasOne` para `Produto`. A linha guarda o id e o nome. `MovimentacaoCartaoConfiguracao` agora declara `ProdutoId`, `NomeProduto` e `Quantidade` como opcionais, com `NomeProduto` limitado a 80. Isso não muda o modelo em relação à migration já gerada.

### Evidência de modelo e de compilação

Comando:

```text
dotnet ef migrations has-pending-model-changes --project backend/src/CardPlay.Repository --startup-project backend/src/CardPlay.Api
```

Saída desta execução: build da comparação concluiu; em seguida, “No changes have been made to the model since the last migration.” Código de saída 0.

Comando:

```text
dotnet build backend/CardPlay.sln
```

Saída desta execução: `Build succeeded.` 0 avisos, 0 erros. Projetos compilados: Domain, Application, Services, Repository, Api, Tests. Tempo 00:00:01.94.

Comando, sobre esse build:

```text
dotnet test backend/CardPlay.sln --no-build
```

Saída desta execução: `Passed! - Failed: 0, Passed: 16, Skipped: 0, Total: 16`. A suíte não abre SQLite e não grava as colunas novas.

O log antigo da API em `localhost:5080` não foi reutilizado. `cardplay.db` não foi aberto.

### O que continua pendente

ACE-03 depois de gravar segue sem `SaveChanges` observado com as colunas novas. A etapa 3 continua fora deste diff.

---

## 2026-10-07 — Etapa 3

### Etapa revisada

Etapa 3 de `06-plan.md`: buscar cartão e produto, aplicar a regra da etapa 1 e persistir saldo e registro no mesmo `SaveChanges`, ou não persistir nenhum dos dois. Parar antes do controller.

O parecer continua neste arquivo. Não há Markdown de execução por etapa.

### Arquivos e propósito

Estado lido no Git desta revisão: quatro arquivos modificados e quatro não rastreados. Nenhum controller, DTO de entrada, cliente ou tela aparece nesse conjunto.

| Arquivo | Papel no diff |
| --- | --- |
| `backend/src/CardPlay.Application/Contratos/Servicos/ICompraServico.cs` | Arquivo novo. `ComprarAsync(cartaoId, produtoId)` devolve `CartaoResposta` |
| `backend/src/CardPlay.Application/Servicos/CompraServico.cs` | Arquivo novo. Lê cartão e produto, chama `Cartao.Comprar`, adiciona a movimentação e chama `SalvarAlteracoesAsync` uma vez |
| `backend/src/CardPlay.Application/Contratos/Repositorios/IProdutoRepositorio.cs` | Declara `ObterPorIdAsync` |
| `backend/src/CardPlay.Repository/Repositorios/ProdutoRepositorio.cs` | Implementa a leitura por id, com `AsNoTracking`, sem filtrar `Disponivel` |
| `backend/src/CardPlay.Domain/Excecoes/ProdutoNaoEncontradoException.cs` | Arquivo novo. Mensagem “Produto {id} não foi encontrado.” |
| `backend/src/CardPlay.Api/Program.cs` | Uma linha: registra `ICompraServico` em `CompraServico`. Não há action HTTP |
| `backend/tests/CardPlay.Tests/CompraServicoTestes.cs` | Arquivo novo. Cinco testes com repositórios em memória |
| `docs/demandas/CP-001-compra-com-saldo/07-decisoes.md` | A linha que bloqueava o lugar do caso de uso passa a dizer que ele fica em `CompraServico`. O contrato HTTP continua na etapa 4 |

### O que o código faz

Leitura dos arquivos, sem execução.

`ComprarAsync` busca o cartão. Se não existe, lança `CartaoNaoEncontradoException` antes de ler o produto e antes de gravar. Se o cartão existe e o produto não, lança `ProdutoNaoEncontradoException` antes de `Comprar` e antes de gravar. No outro caminho, `cartao.Comprar(produto)` debita pelo preço carregado do repositório; em seguida o serviço chama `AdicionarMovimentacao` e um único `SalvarAlteracoesAsync`. O retorno é `MapeadorCartao.ParaResposta`, que copia id, titular, código, saldo e data de criação do cartão.

`Comprar` pode lançar `SaldoInsuficienteException` antes de alterar o cartão. Nesse caminho o serviço não chega em `AdicionarMovimentacao` nem em `SalvarAlteracoesAsync`.

`Produto.Disponivel` não é consultado. Não há gravação de tentativa quando a compra é recusada.

O duplo de cartão em `CompraServicoTestes` incrementa `VezesSalvo` e deixa `AdicionarMovimentacao` vazio. O duplo de produto guarda a instância recebida. O teste de saldo maior espera saldo 41,50 a partir de recarga 50 e preço 8,50, uma gravação, e uma movimentação com id do produto, nome, quantidade 1 e valor igual ao preço. O de saldo igual espera saldo zero, uma gravação e uma movimentação com o id do produto. O de saldo menor espera a exceção de saldo, saldo 10 e zero gravações. Cartão ausente e produto ausente esperam a exceção correspondente e zero gravações; no produto ausente, o saldo permanece 50.

### Aderência ao plano

O plano pedia orquestração que receba cartão e produto, uma leitura de produto por id, um `SaveChanges` só no caminho de conclusão, testes de serviço em memória no estilo de `CartaoServicoTestes`, e parada antes do controller. O lugar do tipo estava em aberto. O nome não estava escolhido.

| Pedido da etapa 3 | Leitura do diff |
| --- | --- |
| Buscar cartão e produto e aplicar a regra da etapa 1 | `CompraServico` chama `ObterPorIdAsync` nos dois repositórios e depois `cartao.Comprar` |
| Um `SaveChanges`, ou nenhum | Uma chamada a `SalvarAlteracoesAsync` depois de `Comprar`. As três recusas lançam antes dela |
| Leitura de produto por identificador | `IProdutoRepositorio.ObterPorIdAsync` e a implementação no repositório concreto |
| Testes: compra concluída, saldo igual, saldo menor, cartão ausente, produto ausente | Os cinco `[Fact]` estão em `CompraServicoTestes` |
| Suíte de recarga preservada no código | `CartaoServico` e `CartaoServicoTestes` não entram neste diff |
| Parar antes do controller | `Program.cs` só registra o serviço. Não há action nova |
| Sem rastro de tentativa | Nenhum `SaveChanges` nos caminhos de exceção |
| `Produto.Disponivel` fora da regra | A leitura não filtra esse campo e o serviço não o lê |

### Critérios

“Confirmado” abaixo é leitura de código e de teste escrito, não de execução.

| Critério | Nesta etapa | Situação |
| --- | --- | --- |
| ACE-01, forma geral | O teste de saldo maior usa recarga de 50 e um `Produto` de 8,50 criado no teste, e espera saldo 41,50 e um registro com produto, quantidade 1 e valor 8,50. Não afirma data e hora. O produto não é lido do seed | Código e teste alinhados à forma geral do saldo e do registro, sem a data. Execução não verificada |
| ACE-01, par R$ 50 / R$ 20 / R$ 30 | Fora da meta já na etapa 1. Esta etapa também não usa esse par | Fora desta etapa |
| ACE-02 | Ordem “ver e depois confirmar” é a etapa 5 | Fora desta etapa |
| ACE-03 | No caminho que retorna, débito e `AdicionarMovimentacao` acontecem antes do único `SalvarAlteracoesAsync`. O valor da movimentação no teste de saldo maior é o preço. O teste de saldo igual não afirma nome, quantidade, valor nem data | Código alinhado no caminho de sucesso. O teste de saldo igual não observa o conteúdo completo do registro. Execução não verificada. Gravação em SQLite não está neste teste |
| ACE-04 | O teste espera saldo zero, uma gravação e uma movimentação com o id do produto | Saldo zero está no teste. O restante do registro, nesse teste, não está afirmado. Execução não verificada |
| ACE-05 | O teste de saldo maior afirma `Saldo >= 0`. O de saldo igual espera zero. O de saldo menor espera 10 | Código alinhado nos três testes escritos. Execução não verificada |
| ACE-06, sem a redação | Saldo menor lança `SaldoInsuficienteException`, não grava e mantém o saldo. O teste não afirma o texto | Parte de domínio e de “não gravar” alinhada ao teste escrito. Execução não verificada |
| ACE-07, cenários A e B | Cartão ausente lança `CartaoNaoEncontradoException` e não grava. Produto ausente lança `ProdutoNaoEncontradoException`, mantém saldo 50 e não grava | Os dois cenários estão em testes separados, com exceções e frases diferentes. ACE-07 pede uma informação de dado inválido para os dois. Execução não verificada |
| ACE-08 | Sem falha provocada de `SaveChanges`. O plano deixa esta evidência de fora | Fora desta etapa |

### Evidências

**Confirmado por artefato**

- `git status` e `git diff` desta revisão mostram os oito arquivos da tabela. Não há controller novo.
- `CompraServico` chama `SalvarAlteracoesAsync` uma vez, depois de `Comprar` e de `AdicionarMovimentacao`.
- `TratadorExcecoes` não cita `SaldoInsuficienteException` nem `ProdutoNaoEncontradoException`. O ramo genérico continua HTTP 500. Nenhum controller chama `ICompraServico`.
- `MovimentacaoResposta` continua com id, valor, descrição e data/hora. Não tem produto nem quantidade.
- A seção anterior deste arquivo registra `dotnet test` com 16 testes, anterior a `CompraServicoTestes`.

**Falha**

- Nenhuma falha de execução foi observada. Esta revisão não rodou build nem testes.

**Não verificado**

- Não há, no repositório nem nos terminais lidos, saída de `dotnet test` ou de `dotnet build` posterior a estes arquivos. A existência dos cinco testes não é resultado de teste.
- O contador `VezesSalvo` é o sinal de gravação do duplo. `AdicionarMovimentacao` nesse duplo está vazio, então o teste não observa se o repositório recebeu a movimentação. O plano já registra que o duplo em memória não grava de verdade.
- ACE-03 depois de gravar no SQLite continua sem chamada. O plano não pede teste de banco nesta etapa.

### Desvios

1. **O tipo e o nome foram escolhidos neste diff.** O plano deixava em aberto método num serviço existente ou tipo novo, e dizia que o nome não estava escolhido. O código cria `CompraServico`. `07-decisoes.md` passa a afirmar esse lugar no mesmo conjunto de arquivos, não numa decisão anterior à implementação.

2. **ACE-07 fica com duas exceções e duas frases.** Cartão ausente reutiliza “Cartão {id} não foi encontrado.” Produto ausente ganha “Produto {id} não foi encontrado.” O critério pede a mesma informação de dado inválido nos dois cenários. P2.1, ainda listado como aberto em `07-decisoes.md`, guarda texto e canal.

3. **O caso de uso devolve só o cartão.** `CartaoResposta` não carrega produto, quantidade, valor da compra nem data da compra. O registro é afirmado no teste pela entidade em memória, não pelo retorno. O plano diz que a etapa 4 encaminha o que este caso de uso devolver, e que a resposta de sucesso inclui saldo e registro.

### Riscos

- Encaminhar `ComprarAsync` para HTTP sem mudar o retorno entrega o saldo novo e não entrega o registro da compra.
- `SaldoInsuficienteException` e `ProdutoNaoEncontradoException` caem no ramo 500 de `TratadorExcecoes` se um controller as deixar subir. P2.1 ainda não escolhe 400, 404 ou outro status.
- O teste de compra concluída continua passando se `AdicionarMovimentacao` deixar de ser chamado, porque o duplo ignora o argumento e a movimentação já está na lista do cartão desde `Comprar`.
- `ObterPorIdAsync` não filtra `Disponivel`. Isso segue o plano. Um produto com `Disponivel` falso seria comprado se a etapa 4 o encaminhasse.

### Dúvidas para a decisão humana

- `CompraServico` como tipo novo fica aceito, ou a orquestração deve ir para um método de `CartaoServico`?
- O retorno da etapa 3 permanece `CartaoResposta`, ou passa a incluir o registro antes do HTTP?
- As duas frases de cartão ausente e produto ausente permanecem distintas até P2.1?

### Validações pendentes

- Rodar `dotnet test backend/CardPlay.sln` e guardar a saída, com os cinco testes de `CompraServicoTestes` e a suíte de recarga. Esta revisão não fez isso.
- ACE-02, o contrato HTTP, o texto ao cliente e ACE-08 continuam nas etapas em que o plano os colocou.

### Recomendação

Não avançar para a etapa 4 tratando a etapa 3 como validada.

O fluxo pedido está no código: uma leitura dos dois ids, a regra da etapa 1, uma gravação no sucesso e nenhuma gravação nas três recusas, sem controller. Falta saída verificável dos testes. Ficam também três escolhas que a etapa 4 herdaria: o tipo `CompraServico`, o retorno só com o cartão e as duas mensagens de ausência.

Decisão humana sugerida: aceitar esses três pontos ou pedir correção antes do HTTP, e guardar a saída da suíte. Sem essa saída, a etapa não está validada.

---

## 2026-10-07 — Etapa 4

### Etapa revisada

Etapa 4 de `06-plan.md`: receber a confirmação, chamar a orquestração e devolver saldo e registro, ou a informação de recusa, sem regra de saldo no controller. Parar antes do aplicativo.

O parecer continua neste arquivo. Não há Markdown de execução por etapa.

### Arquivos e propósito

Estado lido no Git desta revisão: oito arquivos modificados e dois não rastreados. `mobile/` não aparece nesse conjunto.

| Arquivo | Papel no diff |
| --- | --- |
| `backend/src/CardPlay.Application/Dtos/CompraRequisicao.cs` | Arquivo novo. Corpo com `ProdutoId` |
| `backend/src/CardPlay.Application/Dtos/CompraResposta.cs` | Arquivo novo. `CartaoId`, `Saldo` e `Registro` |
| `backend/src/CardPlay.Application/Dtos/MovimentacaoResposta.cs` | Acrescenta `ProdutoId`, `NomeProduto` e `Quantidade`, anuláveis |
| `backend/src/CardPlay.Application/Mapeamentos/MapeadorCartao.cs` | Copia esses três campos da movimentação para o DTO |
| `backend/src/CardPlay.Application/Contratos/Servicos/ICompraServico.cs` | `ComprarAsync` passa a devolver `CompraResposta` |
| `backend/src/CardPlay.Application/Servicos/CompraServico.cs` | O retorno leva o saldo do cartão e a movimentação recém-criada |
| `backend/src/CardPlay.Api/Controllers/CartoesController.cs` | `POST {id}/compras` chama `ComprarAsync` e devolve `Ok` |
| `backend/src/CardPlay.Api/Tratamento/TratadorExcecoes.cs` | Saldo insuficiente entra no 400 já usado pela recarga inválida. Produto ausente entra no 404 já usado pelo cartão ausente |
| `backend/tests/CardPlay.Tests/CompraServicoTestes.cs` | O teste de saldo maior passa a afirmar `CartaoId` e os campos do `Registro` |
| `docs/demandas/CP-001-compra-com-saldo/07-decisoes.md` | A seção “HTTP fechado na etapa 4” registra rota, corpo, 400 e 404 no mesmo conjunto de arquivos |

### O que o código faz

Leitura dos arquivos, sem execução nesta revisão.

`Comprar` no controller recebe o id da rota e `ProdutoId` do corpo, chama o serviço e devolve o resultado. Não compara saldo nem preço.

`CompraResposta` carrega o id do cartão, o saldo depois da operação e o `Registro`. O registro é `MovimentacaoResposta`, que agora também tem produto, nome e quantidade. `ListarMovimentacoes` devolve esse mesmo DTO, então a listagem passa a incluir os três campos. Recarga não os preenche no domínio; no DTO ficam nulos.

`TratadorExcecoes` traduz `SaldoInsuficienteException` para HTTP 400, título “Requisição inválida”. Traduz `ProdutoNaoEncontradoException` para HTTP 404, título “Recurso não encontrado”, o mesmo título de `CartaoNaoEncontradoException`. O `detail` continua a mensagem de cada exceção: “Saldo insuficiente para concluir a compra.”, “Cartão {id} não foi encontrado.” e “Produto {id} não foi encontrado.”

### Aderência ao plano

O plano pedia a confirmação na API, saldo e registro na resposta, ou a informação de recusa, sem regra de saldo no controller. Pedia também o corpo observado numa chamada, não só o DTO em C#. Rota, verbo e status não estavam escolhidos. Estender `MovimentacaoResposta` ou criar outra leitura dependia de P1.1 e P1.2, já respondidos em `07-decisoes.md` como a mesma lista e campos próprios. O cliente em `mobile/src/api/` não devia mudar nesta etapa.

| Pedido da etapa 4 | Leitura do diff |
| --- | --- |
| Receber a confirmação e chamar a orquestração | `POST /api/cartoes/{id}/compras` chama `ComprarAsync` |
| Devolver saldo e registro | `CompraResposta` tem `Saldo` e `Registro` |
| Sem regra de saldo no controller | A action não lê preço nem saldo |
| Traduzir recusa | 400 para saldo insuficiente; 404 para cartão ou produto ausente |
| Não alterar o cliente | Nenhum arquivo de `mobile/` neste diff |
| Campos do registro na mesma lista | `MovimentacaoResposta` ganha produto, nome e quantidade, e a listagem usa esse DTO |
| Corpo observado, não só o DTO | Um arquivo de corpo de sucesso existe. As outras chamadas pedidas pelo plano não estão nesse arquivo |

### Critérios

“Confirmado” abaixo separa código, o corpo gravado e o que não tem registro.

| Critério | Nesta etapa | Situação |
| --- | --- | --- |
| ACE-01, forma geral | O arquivo `/tmp/compra-cafe.json` contém `saldo` 41.5, `registro.valor` 8.5, `quantidade` 1, `nomeProduto` “Café especial” e `produtoId` `6f1c2a7e-0c4a-4b1d-9e2f-1a2b3c4d5e6f`. Esse id, em `ProdutoConfiguracao`, é o café do seed a 8,50. O JSON não traz o saldo anterior | Corpo de sucesso alinhado ao preço do seed e a um saldo final de 41,5. O saldo anterior dessa chamada não está no arquivo. Execução da suíte não verificada |
| ACE-01, par R$ 50 / R$ 20 / R$ 30 | Continua fora da meta | Fora desta etapa |
| ACE-02 | A ordem “ver e depois confirmar” é a etapa 5 | Fora desta etapa |
| ACE-03 | O serviço grava e devolve a mesma movimentação no `Registro`. O corpo gravado traz valor, data/hora, produto e quantidade juntos com o saldo | Código e esse corpo alinhados. Não há, no log lido, o par débito e registro dentro de um único `SaveChanges` com os valores dos parâmetros: o log marca os parâmetros com `?` |
| ACE-04 | Nenhum corpo gravado mostra saldo zero | Não verificado na API |
| ACE-05 | O corpo de sucesso tem saldo 41.5. Os outros finais não estão em arquivo de resposta | Só o sucesso está no arquivo. O restante não verificado |
| ACE-06 | O código traduz a exceção para 400 e o `detail` é a frase de saldo insuficiente. Não há arquivo com essa resposta HTTP | Código alinhado. Resposta HTTP não verificada nesta revisão |
| ACE-07 | Os dois casos caem em 404 com o mesmo título e `detail` diferente. Não há arquivo com essas respostas | Status comum está no código. A informação não é a mesma frase. Resposta HTTP não verificada |
| ACE-08 | Sem mecanismo de falha de gravação | Fora desta etapa |

### Evidências

**Confirmado por artefato**

- `git status` e `git diff` desta revisão mostram os dez arquivos da tabela. Não há alteração em `mobile/`.
- `/tmp/compra-cafe.json` contém o JSON citado em ACE-01, com nomes em camelCase: `cartaoId`, `saldo`, `registro`, `produtoId`, `nomeProduto`, `quantidade`, `dataHora`.
- O log do processo em `127.0.0.1:5099`, com `Data Source=/tmp/cardplay-etapa4.db`, registra a aplicação da migration `20261007183202_ProdutoEQuantidadeNaMovimentacao`, inclusive `ADD "NomeProduto"`, `ADD "ProdutoId"` e `ADD "Quantidade"`. Os `INSERT` e `SELECT` de `MovimentacoesCartao` nesse log incluem essas colunas. O processo encerrou com código 137.
- O log de `localhost:5080` em `1.txt` continua com “No migrations were applied” e não lista as colunas novas. Não é evidência desta rota.

**Falha**

- Nenhuma falha de compilação ou de resposta HTTP foi lida nesta revisão. Esta revisão não rodou build nem testes.

**Não verificado**

- Não há saída de `dotnet test` nem de `dotnet build` posterior a estes arquivos. O registro anterior de 16 testes é anterior a `CompraServicoTestes`.
- Não há arquivo de resposta para saldo igual ao preço, saldo menor, cartão ausente ou produto ausente. O log SQL não mostra status HTTP nem o valor dos parâmetros.
- ACE-08 e a ordem de confirmação na interface continuam fora desta etapa.

### Desvios

1. **Rota e status foram escolhidos neste diff.** O plano dizia que verbo, caminho e código HTTP não estavam escolhidos. O código usa `POST /api/cartoes/{id}/compras`, 400 e 404. `07-decisoes.md` descreve isso na seção “HTTP fechado na etapa 4”, escrita junto com o código.

2. **ACE-07 permanece com duas frases.** Os dois casos usam o título “Recurso não encontrado” e HTTP 404. O `detail` continua “Cartão {id} não foi encontrado.” ou “Produto {id} não foi encontrado.” O critério pede uma informação de dado inválido para os dois.

3. **A listagem muda de corpo junto com a compra.** `MovimentacaoResposta` ganha três campos. `GET /api/cartoes/{id}/movimentacoes` passa a enviá-los. P1.1 e P1.2 já pediam a mesma lista com campos próprios. O plano também avisava que estender esse DTO muda o corpo que `listarMovimentacoes` já consome. O aplicativo não foi atualizado nesta etapa, como o plano pedia.

### Riscos

- A tela de histórico ainda pode mostrar o valor da compra com o prefixo de crédito. Isso é a etapa 6. O corpo da listagem já traz a compra e a recarga no mesmo JSON.
- O processo de `localhost:5080` visto em `1.txt` não aplicou a migration nova. Subir essa API antiga contra o código atual, ou o contrário, mistura esquema e rota.
- O corpo em `/tmp/compra-cafe.json` veio de `/tmp/cardplay-etapa4.db`, não do `cardplay.db` do projeto. O arquivo não guarda o saldo anterior nem o status HTTP da resposta.
- `8.5` e `41.5` são o JSON de 8,50 e 41,50. O texto não conserva o zero dos centavos.

### Dúvidas para a decisão humana

- `POST /api/cartoes/{id}/compras`, 400 e 404 ficam aceitos como o contrato?
- As duas frases de cartão ausente e produto ausente ficam distintas, com o mesmo título 404?
- Estender `MovimentacaoResposta` na listagem, antes da etapa 6, fica aceito?

### Validações pendentes

- Guardar as respostas HTTP de saldo igual ao preço, saldo menor, cartão ausente e produto ausente. O plano pede essas chamadas com o corpo observado. Esta revisão não as encontrou em arquivo.
- Rodar `dotnet test backend/CardPlay.sln` e guardar a saída. Esta revisão não fez isso.
- ACE-02, o gesto na interface e ACE-08 continuam nas etapas em que o plano os colocou.

### Recomendação

Não avançar para a etapa 5 tratando a etapa 4 como validada.

O controller encaminha a confirmação e o retorno de sucesso tem saldo e registro. Um corpo gravado mostra a compra do café do seed a 8,50 com saldo 41,5, quantidade 1 e o nome do produto, em camelCase. Faltam, em arquivo, as respostas de saldo insuficiente, de dado ausente e de saldo igual ao preço, e falta a saída da suíte. Rota, status e as duas frases de ausência foram registrados no mesmo diff que o código.

Decisão humana sugerida: aceitar esse contrato ou pedir correção antes da interface, e guardar as chamadas que ainda não estão em arquivo. Sem isso, a etapa não está validada.

---

## 2026-10-07 — Etapa 5

### Etapa revisada

Etapa 5 de `06-plan.md`: o cliente identifica o cartão, seleciona o produto, vê o produto e o valor, e só então confirma, usando a API da etapa 4. Sem tela nova e sem mudar a leitura do histórico.

O parecer continua neste arquivo. Não há Markdown de execução por etapa.

O conjunto de trabalho ainda contém os arquivos da etapa 4. Eles não são a alteração desta etapa. A leitura abaixo usa o diff de `mobile/` e a seção “Interface fechada na etapa 5” de `07-decisoes.md`.

### Arquivos e propósito

| Arquivo | Papel no diff |
| --- | --- |
| `mobile/src/api/cartaoApi.ts` | `comprar` envia `POST /api/cartoes/{id}/compras` com `produtoId` |
| `mobile/src/tipos/index.ts` | `Compra` com `cartaoId`, `saldo` e `registro`. `Movimentacao` ganha `produtoId`, `nomeProduto` e `quantidade` opcionais |
| `mobile/src/estado/CartaoContexto.tsx` | `comprar` chama a API, grava o saldo devolvido e busca de novo as movimentações |
| `mobile/src/componentes/CardProduto.tsx` | “Usar cartão” deixa de ficar sempre desabilitado. Com cartão, o toque abre a confirmação com nome e preço |
| `mobile/src/telas/TelaCatalogo.tsx` | Liga o botão ao cartão do contexto, mostra o saldo e a mensagem de erro ou de compra registrada |
| `docs/demandas/CP-001-compra-com-saldo/07-decisoes.md` | A seção “Interface fechada na etapa 5” registra o gesto, o botão, o cartão do contexto, o texto da API e a permanência no catálogo |

Não há diff em `TelaMovimentacoes.tsx`, `mobile/src/tema/cores.ts` nem na barra de abas.

### O que o código faz

Leitura dos arquivos, sem execução nesta revisão.

O card continua mostrando nome, descrição e `formatarMoeda(produto.preco)` antes do botão. O preço vem da lista de `produtoApi.listar`. Sem cartão no contexto, o botão fica desabilitado e o texto secundário é “Solicite um cartão”. Com cartão, o botão usa `cores.laranja` e o toque abre `Alert.alert` com o nome e o mesmo preço. Cancelar não chama `onConfirmar`. Confirmar chama `comprar(produto.id)`.

`comprar` usa `cartao.id` do `CartaoProvider`. Se a API responde, o contexto substitui o saldo pelo `compra.saldo` e substitui `movimentacoes` pelo retorno de `listarMovimentacoes`. Se a API falha, `setCartao` não roda. O catálogo mostra `falha.message` em coral. `clienteHttp` monta essa mensagem com `detail` ou `title`.

Depois do sucesso, a tela permanece no catálogo e escreve “Compra registrada. Saldo:” com o saldo devolvido. A linha “Saldo atual” lê `cartao.saldo` do contexto.

### Aderência ao plano

O plano pedia a confirmação depois de o produto e o valor estarem visíveis, o cliente HTTP em `mobile/src/api/`, a atualização do `CartaoProvider` como a recarga já faz, e a parada antes da leitura do histórico. Não pedia tela nova, mudança de `cores.ts` nem da barra de abas. P2.2 e P3.2 estavam em aberto. O plano dizia que o id em `@cardplay/cartaoId` não seria a identificação de RN02 até a etapa 0 dizer isso.

| Pedido da etapa 5 | Leitura do diff |
| --- | --- |
| Ver produto e preço antes de confirmar | Nome e preço estão acima do botão. A API só é chamada no “Confirmar” do alerta |
| Valor visto igual ao preço do catálogo | O alerta usa `produto.preco` do mesmo objeto exibido no card |
| Cliente HTTP da operação da etapa 4 | `cartaoApi.comprar` aponta para `POST /api/cartoes/${id}/compras` |
| Atualizar o contexto depois do sucesso | `setCartao` com o saldo devolvido e nova leitura das movimentações |
| Sem tela nova | Não há rota nem tela nova. O passo extra é `Alert.alert` |
| Sem mudança de histórico | `TelaMovimentacoes.tsx` não entra no diff |
| Sem `cores.ts` e sem barra de abas | Esses arquivos não entram no diff |
| Evidência no fluxo do aplicativo | Não há sessão, captura nem chamada HTTP disparada por essa tela |

### Critérios

“Confirmado” abaixo é leitura de código. O plano pede o fluxo no aplicativo. Essa execução não está registrada.

| Critério | Nesta etapa | Situação |
| --- | --- | --- |
| ACE-02 | O código só confirma depois de o card já ter desenhado nome e preço, e o alerta repete os dois. O preço do alerta é o do produto listado | Ordem alinhada no código. Fluxo não verificado |
| ACE-01, trecho em que o gatilho é a confirmação | Confirmar chama `comprar`. O sucesso grava `compra.saldo` no contexto. Não há observação de saldo anterior menos o preço do seed | Código do gatilho alinhado. Resultado no aplicativo não verificado |
| ACE-04 | O mesmo `comprar` serve para saldo igual ao preço. Não há caminho separado nem observação de saldo zero | Não verificado |
| ACE-06, gatilho do cliente | A falha da API não altera o cartão no contexto e o catálogo mostra a mensagem da exceção. O texto esperado da API é o `detail` de saldo insuficiente | Código alinhado. Fluxo com saldo menor não verificado |
| ACE-07 | P2.4 ficou de fora. Sem cartão, o botão não chama a API. Isso não informa “dado inválido” | Fora do que a etapa 0 equiparou a dado inválido |

### Evidências

**Confirmado por artefato**

- `git diff` de `mobile/src/api/cartaoApi.ts`, `CardProduto.tsx`, `CartaoContexto.tsx`, `TelaCatalogo.tsx` e `tipos/index.ts`, mais a seção de interface em `07-decisoes.md`.
- `TelaMovimentacoes.tsx`, `cores.ts` e a barra de abas não aparecem nesse diff.

**Falha**

- O único comando de TypeScript registrado nesta pasta de terminais é `npx tsc --noEmit` em `mobile/`, código de saída 1. A saída é `FETCH_ERROR` ao buscar `tsc` no registro npm. Não chega a compilar o aplicativo. Esta revisão não tratou isso como falha do código da etapa.

**Não verificado**

- Não há registro de `tsc` concluído sobre estes arquivos.
- Não há percurso no aplicativo: cartão com saldo maior que um preço do seed, confirmação só depois do preço visível, saldo do contexto depois do débito, e repetição com saldo menor sem compra concluída.
- Não há chamada HTTP atribuída a esta tela.
- A etapa 6, a apresentação do registro, continua fora deste diff.

### Desvios

1. **O cartão da compra é o do contexto.** O plano dizia que o id em `@cardplay/cartaoId` não seria RN02 até a etapa 0. `comprar` usa `cartao.id`, e esse cartão é o carregado por `obterCartaoIdSalvo`. `07-decisoes.md` registra isso na mesma alteração, não numa resposta anterior de P2.4.

2. **P2.2, P3.2, P2.1 e P3.3 foram escritos junto com o código.** O alerta, o botão existente, o `detail` da API e a permanência no catálogo estão na seção “Interface fechada na etapa 5”. O plano deixava esses pontos em aberto.

3. **Há uma frase de sucesso no catálogo.** “Compra registrada. Saldo:” aparece depois da resposta. Não é tela nova. P1.3 continua na lista do que não pede comprovante.

### Riscos

- `comprar` recolhe as movimentações. A aba Histórico lê essa lista e ainda não distingue a compra da recarga. O arquivo da aba não mudou. O sinal do valor gasto continua para a etapa 6.
- Sem cartão, o botão não compra. Um id salvo que a API não reconheça continua no erro de carga do contexto, não neste botão.
- `AGENTS.md` e `docs/ui.md` ainda descrevem o botão como desabilitado. A etapa 7 é que alinha esses textos.

### Dúvidas para a decisão humana

- Usar o cartão já carregado no aplicativo fica aceito como a identificação da compra?
- O alerta com nome e preço, em vez de comprar no primeiro toque, fica aceito?
- A frase “Compra registrada. Saldo:” no catálogo fica aceita?

### Validações pendentes

- Percorrer no aplicativo um cartão com saldo maior que um preço do seed, confirmar só depois de o produto e o preço estarem visíveis, e ver o saldo do contexto depois do débito.
- Repetir com saldo menor e ver a mensagem de saldo insuficiente, o saldo intacto e a ausência de compra concluída.
- `npx tsc --noEmit` registrado não compilou o projeto. Uma compilação concluída ainda não está neste acompanhamento.
- A etapa 6 continua parada.

### Recomendação

Não avançar para a etapa 6 tratando a etapa 5 como validada.

O código coloca a confirmação depois do nome e do preço, chama `POST /api/cartoes/{id}/compras` só no confirmar, e atualiza o saldo do contexto com a resposta. Cancelar não compra. Não há tela nova nem edição da aba Histórico. Não há percurso registrado nesse fluxo, nem compilação concluída do aplicativo.

Decisão humana sugerida: aceitar o cartão do contexto, o alerta e a frase de saldo no catálogo, ou pedir correção, e guardar o percurso no aplicativo. Sem esse percurso, a etapa não está validada.

---

## 2026-10-07 — Percurso no aplicativo da etapa 5

Fecha o percurso e a compilação deixados na revisão acima. Não substitui essa revisão. Não abre a etapa 6.

### O que foi aceito

O cartão da compra é o já carregado no contexto. A confirmação mostra o nome e o preço antes de chamar a API. A frase “Compra registrada. Saldo:” permanece no catálogo, junto com “Saldo atual”.

### Correção para o alerta existir na web

`Alert.alert` de `react-native-web` não abre diálogo. Em `mobile/src/componentes/CardProduto.tsx`, na web o toque em “Usar cartão” abre um aviso na própria tela, com o título “Usar cartão”, a frase `Confirmar a compra de {nome} por {preço}?`, e os botões Cancelar e Confirmar. No celular o código continua chamando `Alert.alert` com o mesmo texto. Não há rota nova. `cores.ts` e a barra de abas não mudaram. As cores do aviso são as que já existiam.

### Percurso

Aplicativo web em `http://127.0.0.1:8081`, API em `http://127.0.0.1:5080`. Capturas, log e corpos HTTP em `docs/demandas/CP-001-compra-com-saldo/percurso-etapa5/`.

Cartão criado na primeira aba, titular “Percurso Etapa 5”, número **CP-QBDG8R**, id `76066d9e-9442-462b-87da-eceb478d3be7`. Recarga de R$ 10,00.

| Passo | O que a tela mostrou | HTTP |
| --- | --- | --- |
| Catálogo antes de confirmar | “Saldo atual: R$ 10,00”. Café especial com preço R$ 8,50 acima de “Usar cartão” (`04-catalogo-saldo-10.png`) | `GET /api/produtos` 200 |
| Toque em “Usar cartão” do café | Aviso “Confirmar a compra de Café especial por R$ 8,50?” (`05-alerta-cafe.png`) | Nenhuma chamada a `/compras` |
| Cancelar | O aviso fecha. O saldo continua R$ 10,00 (`06-cancelou.png`) | Nenhuma chamada a `/compras` |
| Confirmar o café | “Compra registrada. Saldo: R$ 1,50” e “Saldo atual: R$ 1,50” (`07-compra-cafe.png`). A aba Cartão mostra “Saldo disponível R$ 1,50” (`08-cartao-apos-compra.png`) | `POST /api/cartoes/76066d9e-9442-462b-87da-eceb478d3be7/compras` 200. Corpo em `compras.json`: saldo 1,5, produto “Café especial”, valor 8,5, quantidade 1 |
| Adesivo de R$ 4,00 com saldo R$ 1,50 | Aviso “Confirmar a compra de Adesivo CardPlay por R$ 4,00?” (`09-alerta-adesivo.png`). Depois do confirmar, “Saldo insuficiente para concluir a compra.” e “Saldo atual: R$ 1,50” (`10-saldo-insuficiente.png`) | `POST` no mesmo cartão, 400, `detail` “Saldo insuficiente para concluir a compra.” |
| Histórico | Uma “Compra” de R$ 8,50 e uma “Recarga” de R$ 10,00. Não há segunda compra (`11-historico.png`) | A recusa não acrescentou registro |

R$ 10,00 menos R$ 8,50 é R$ 1,50, o saldo que o catálogo e a aba Cartão mostraram depois do confirmar.

### Compilação

`node node_modules/typescript/bin/tsc --noEmit` em `mobile/`, código de saída 0. A saída está em `percurso-etapa5/tsc.txt`. O `npx tsc --noEmit` da revisão anterior continua sendo a tentativa que falhou com `FETCH_ERROR` antes de compilar.

### O que este percurso não fecha

- ACE-04, saldo exatamente igual ao preço, não foi percorrido.
- A aba Histórico mostra a compra com o sinal de recarga (“+ R$ 8,50”). O arquivo da aba não mudou. Isso continua na etapa 6.
- `AGENTS.md` e `docs/ui.md` ainda descrevem o botão como desabilitado. Isso continua na etapa 7.

---

## 2026-10-07 — Etapa 6

### Etapa revisada

Etapa 6 de `06-plan.md`: na consulta decidida na etapa 0, mostrar o registro da compra com produto, quantidade, valor, data e hora, e manter as recargas com o sinal e o texto que já tinham. Parar antes da documentação de produto.

O parecer continua neste arquivo. Não há Markdown de execução por etapa.

O diff desta revisão é `mobile/src/telas/TelaMovimentacoes.tsx`, a seção “Histórico fechado na etapa 6” de `07-decisoes.md` e a pasta `docs/demandas/CP-001-compra-com-saldo/percurso-etapa6/`. `formatacao.ts`, `tipos/index.ts` e `MovimentacaoResposta` não entram nesse diff.

### Arquivos e propósito

| Arquivo | Papel no diff |
| --- | --- |
| `mobile/src/telas/TelaMovimentacoes.tsx` | A mesma lista passa a desenhar a compra pelo nome do produto, quantidade e valor sem “+”. A recarga continua com a descrição e com “+ ” |
| `docs/demandas/CP-001-compra-com-saldo/07-decisoes.md` | P2.5 e P3.1 saem de “Ainda abertas” e ganham a seção “Histórico fechado na etapa 6” |
| `percurso-etapa6/historico.png` | Captura da aba Histórico |
| `percurso-etapa6/texto.txt` | Texto do documento no momento da captura |
| `percurso-etapa6/tsc.txt` | Saída de `node node_modules/typescript/bin/tsc --noEmit` |

### O que o código faz

Leitura do diff, sem execução nesta revisão.

`ehCompra` é verdadeiro quando `nomeProduto` não é nulo e tem tamanho maior que zero. Nesse caso o título da linha é o nome do produto, surge a linha “Quantidade” com `quantidade`, e o valor é `formatarMoeda` sem prefixo. Caso contrário a linha continua com `descricao` e `+ ` antes do valor. A data, nos dois casos, continua `formatarDataHora`. A cor do valor continua `cores.laranja`. Não há sinal de menos.

Os três textos que falavam só de recarga mudam: sem cartão, “o histórico aparecerá aqui”; com cartão, “Histórico deste cartão.”; lista vazia, “Nenhuma movimentação ainda.”

### Aderência ao plano

P1.1 já era a mesma lista. O plano pedia essa consulta com produto, quantidade, valor, data e hora, e pedia que a recarga anterior conservasse o sinal e o texto. Não pedia segunda lista, mudança de `formatarDataHora` nem documentação de produto. Dizia que P2.5 e P3.1 não tinham critério de exibição até serem respondidos, e que mostrar a compra com “+ ” não estava aprovado.

| Pedido da etapa 6 | Leitura do diff e dos arquivos |
| --- | --- |
| Mesma lista, sem componente novo | Só `TelaMovimentacoes.tsx` muda no aplicativo |
| Compra com produto, quantidade, valor, data e hora | A linha usa `nomeProduto`, `quantidade`, `valor` e `formatarDataHora` |
| Recarga com o sinal e o texto anteriores | Sem `nomeProduto`, a descrição e o prefixo “+ ” permanecem |
| Valor gasto sem aparecer como crédito | A compra não recebe “+ ” |
| Evidência com uma recarga e uma compra | `historico.png` mostra as duas linhas. O fluxo que criou a compra está em `percurso-etapa5/` |
| P2.5 e P3.1 respondidos antes do critério de exibição | As respostas estão no mesmo diff da tela, na seção nova de `07-decisoes.md` |

### Critérios

| Critério | Nesta etapa | Situação |
| --- | --- | --- |
| ACE-01, um único registro com produto, quantidade, valor, data e hora | `historico.png` tem uma linha “Café especial”, “Quantidade 1”, “R$ 8,50” e “07/10/2026, 19:45”. `percurso-etapa5/compras.json` tem um corpo 200 com esse produto, quantidade 1 e valor 8,5, em `2026-10-07T19:45:39.7661731Z`. A recusa do adesivo, no log da etapa 5, não deveria gerar segunda compra | A captura mostra um registro de compra e uma recarga. O id da movimentação não aparece na tela |
| ACE-03, o registro correspondente existe na consulta | A mesma captura não é um histórico só de recargas | Confirmado na captura. O débito em si não é desta etapa |
| ACE-04 | Saldo exatamente igual ao preço | Continua não percorrido |

### Evidências

**Confirmado por artefato**

- `git diff` de `TelaMovimentacoes.tsx`: a compra deixa de usar a descrição e o prefixo “+ ”; a recarga conserva os dois; a quantidade só entra na compra.
- `historico.png`: aba Histórico, subtítulo “Histórico deste cartão.”, primeira linha “Café especial” / “R$ 8,50” / “Quantidade 1” / “07/10/2026, 19:45”, segunda linha “Recarga” / “+ R$ 10,00” / “07/10/2026, 19:45”.
- `texto.txt`, na mesma pasta, inclui “CP-QBDG8R” e as mesmas duas linhas. O número do cartão também está em `percurso-etapa5/log.txt`, no percurso que gravou o café.
- `percurso-etapa6/tsc.txt`: `node node_modules/typescript/bin/tsc --noEmit` em `mobile/`, código de saída 0. Isso registra compilação do aplicativo. Não é a consulta.

**Falha**

- Nenhuma falha de compilação ou de tela está nesses três arquivos. Esta revisão não rodou build nem testes.

**Não verificado**

- `texto.txt` também contém o texto da aba Cartão. A captura visível é `historico.png`, não esse texto inteiro.
- Os textos de lista vazia e de cartão ausente mudam no diff e não aparecem na captura.
- A hora “19:45” coincide com os dígitos do instante em `compras.json`. Não há fuso da captura. Esta revisão não conclui se a hora exibida é a local do cliente.
- A etapa 7, os documentos de produto, continua fora deste diff.

### Desvios

1. **P2.5 e P3.1 foram escritos junto com a tela.** O plano dizia que não havia critério de exibição até essas respostas. `07-decisoes.md` as registrava em “Ainda abertas” e dizia que a etapa seguia sem diff até o registro. O diff tira as duas da lista e define, na mesma alteração, nome do produto, “Quantidade” com o número gravado, valor sem “+”, recarga com “+ ”, cor `laranja` e o `formatarDataHora` já usado na recarga.

2. **A compra deixa de mostrar a descrição “Compra”.** O título passa a ser `nomeProduto`. A descrição gravada continua “Compra”; a tela da compra não a escreve.

### Riscos

- Uma movimentação com `nomeProduto` vazio continua no ramo da recarga e recebe “+ ”. O domínio da compra preenche o nome; a tela não tem outro critério.
- A hora curta dos dois registros cai no mesmo minuto. A captura não separa segundos. Os instantes do log da etapa 5 também caem nesse minuto.
- `AGENTS.md` e `docs/ui.md` ainda descrevem o histórico como recarga e o botão como desabilitado. A etapa 7 é que alinha esses textos.

### Dúvidas para a decisão humana

- P2.5, como está na seção “Histórico fechado na etapa 6”, fica aceito?
- P3.1, reutilizar `formatarDataHora` da recarga, fica aceito?
- Omitir a descrição “Compra” e usar o nome do produto como título fica aceito?

### Validações pendentes

- Aceite humano de P2.5 e P3.1. Sem esse aceite, o critério de exibição que o plano deixou em aberto continua sendo o texto escrito no mesmo diff da tela.
- ACE-04 continua não percorrido.
- A etapa 7 continua parada.

### Recomendação

Não avançar para a etapa 7 tratando a etapa 6 como validada.

A consulta pedida está em arquivo: no mesmo cartão do percurso da etapa 5, a aba Histórico mostra uma compra com produto, quantidade 1, R$ 8,50 e data e hora, e a recarga anterior com “+ R$ 10,00”. A compilação registrada em `percurso-etapa6/tsc.txt` terminou com código 0. P2.5 e P3.1 não vieram de uma resposta anterior; foram definidos nesta alteração.

Decisão humana sugerida: aceitar esse desenho do histórico, ou pedir correção do sinal, do rótulo, da descrição omitida ou do formato da data. Sem isso, a etapa não está validada.

---

## 2026-10-07 — Aceite de P2.5 e P3.1, e ACE-04

Fecha as duas pendências da revisão da etapa 6. Não substitui essa revisão. Não abre a etapa 7.

### Aceite

P2.5 e P3.1 ficam aceitos como estão na seção “Histórico fechado na etapa 6” de `07-decisoes.md`. A compra mostra o nome do produto, a quantidade gravada e o valor sem “+”. A recarga conserva a descrição e o “+ ”. A data e a hora da compra usam o mesmo `formatarDataHora` da recarga. O título da compra é o nome do produto; a descrição “Compra” não é repetida na tela.

### ACE-04

Cartão **CP-NYL59G**, id `518814c5-f699-4db0-af64-a52965500247`, titular “ACE-04”. Recarga de R$ 4,00, igual ao preço do adesivo do seed. Capturas e corpo HTTP em `docs/demandas/CP-001-compra-com-saldo/percurso-ace04/`.

| Passo | O que a tela mostrou |
| --- | --- |
| Catálogo antes de confirmar | “Saldo atual: R$ 4,00” e Adesivo CardPlay a R$ 4,00 (`03-catalogo-saldo-igual.png`) |
| Confirmação | “Confirmar a compra de Adesivo CardPlay por R$ 4,00?” (`04-alerta-adesivo.png`) |
| Depois de confirmar | “Compra registrada. Saldo: R$ 0,00” e “Saldo atual: R$ 0,00” (`05-saldo-zero.png`). A aba Cartão mostra “Saldo disponível R$ 0,00” (`06-cartao-zero.png`) |
| Histórico | “Adesivo CardPlay”, “R$ 4,00”, “Quantidade 1” e a recarga “+ R$ 4,00” (`07-historico.png`) |

`compras.json` guarda o `POST /compras` 200: saldo 0,0, produto “Adesivo CardPlay”, valor 4,0, quantidade 1. Não houve segunda compra.
