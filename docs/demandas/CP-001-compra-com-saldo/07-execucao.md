# CP-001 — Acompanhamento de execução

Cada revisão entra como uma seção datada. O arquivo não substitui o plano nem os critérios.

| Data       | Etapa revisada                         | Recomendação                                                                                                                |
| ---------- | -------------------------------------- | --------------------------------------------------------------------------------------------------------------------------- |
| 2026-10-07 | Etapa 1 — débito e registro no domínio | Não avançar como etapa validada. Decidir os desvios de modelo antes da etapa 2                                              |
| 2026-10-07 | Etapa 2 — modelo do registro no SQLite | Não avançar como etapa validada. Investigar a compilação que o plano exige. O diff de persistência não pede correção por si |

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
