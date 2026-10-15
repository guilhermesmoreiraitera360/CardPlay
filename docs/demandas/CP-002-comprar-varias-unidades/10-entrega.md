# CP-002 — Entrega: Comprar várias unidades

## Estado deste documento

| Campo | Valor |
| --- | --- |
| Demanda | CP-002 — Comprar várias unidades de um produto |
| Artefato | Síntese para revisão humana |
| Fontes | `02-historia-rica.md`, `03-criterios-de-aceite.md`, `06-plan.md`, `07-decisoes.md`, `07-execucao.md`, `08-validacao.md`, `09-review-e-testes.md`, pasta `evidencias/` |
| Código comparado | Branch `docs/cp-002-comprar-varias-unidades`, a partir do commit `19a90b1`. A CP-002 está no diretório de trabalho e não tem commit |
| Data da leitura | 2026-10-15 |
| Situação | **Não concluída.** RN01–RN04 e CA01–CA05 estão aprovadas. Os pares de R$ 20,00 foram aceitos como exemplo. O caminho principal, o saldo zero na tela, o 409 na tela e a falha de gravação relida no SQLite têm artefato. Ficam abertos o alerta nativo, o HTTP de estouro e o log SQL |

`08-validacao.md` e `09-review-e-testes.md` registram o inventário anterior. Onde eles dizem que um item não foi observado, a pasta `evidencias/` prevalece para o que ela de fato contém. “Observado” aqui aponta para arquivo desta demanda.

---

## Problema

A compra com saldo já existia, sempre de uma unidade. A história pede comprar várias unidades de um único produto, ver produto, quantidade, preço unitário e total antes de confirmar, debitar o total e guardar um registro com quantidade, preço unitário e total.

As decisões de 2026-10-15, em `07-decisoes.md`, fecham o caminho: quantidade 1 não é defeito; `Valor` é o total; o preço unitário é campo próprio; a confirmação envia a quantidade e o preço visto; preço diferente recusa sem débito; a falha de gravação só existe no teste. RN01–RN04 e CA01–CA05 foram aprovadas nesse mesmo dia como texto oficial.

O par escrito R$ 100,00 / R$ 20,00 / quantidade 3 foi aceito como exemplo. O seed não tem produto a R$ 20,00. Café a R$ 8,50 e adesivo a R$ 4,00 observam a mesma regra.

---

## Solução

A compra continua no mesmo caminho da CP-001. `CardPlay.Services` não entra. Não há carrinho, estoque, teto comercial nem interruptor de falha.

1. `Cartao.Comprar` recebe a quantidade. Total é o preço vezes a quantidade. Quantidade inválida, estouro e saldo menor que o total lançam antes de alterar o cartão. No sucesso, debita o total e cria um registro com quantidade, `PrecoUnitario` e `Valor` igual ao total. `Comprar(produto)` continua comprando uma unidade.
2. `MovimentacoesCartao` ganha `PrecoUnitario`, anulável, sem chave estrangeira. A recarga deixa esse campo vazio. Migration `20261015133903_PrecoUnitarioNaMovimentacao`.
3. `CompraServico.ComprarAsync` compara o preço visto com o vigente. Preço diferente lança `PrecoAlteradoException` e não grava. Preço igual chama o domínio e um `SalvarAlteracoesAsync`. Falha nesse salvar vira `CompraNaoConcluidaException`, mensagem “A compra não foi concluída.”
4. `POST /api/cartoes/{id}/compras` recebe `produtoId`, `quantidade` e `precoUnitarioVisto`. O sucesso devolve saldo e registro, com `precoUnitario` e `valor` como total. Saldo insuficiente e quantidade inválida continuam HTTP 400. Preço diferente é HTTP 409, título “Conflito”. A falha de gravação não ganhou status novo: exceção não mapeada segue HTTP 500, e o `detail` é a mensagem acima.
5. No catálogo, a quantidade é escolhida no card. O aviso mostra produto, quantidade, preço unitário e total. Cancelar não chama a API. Confirmar envia a quantidade e o preço mostrado. O cliente permanece no catálogo. Sem cartão, “Usar cartão” permanece visível e desabilitado.
6. O histórico da compra mostra nome, total sem “+”, quantidade e preço unitário. A recarga continua com a descrição e com “+”.

---

## Alterações

Tudo abaixo está sem commit, na branch `docs/cp-002-comprar-varias-unidades`.

| Fluxo | Arquivos | O que mudou |
| --- | --- | --- |
| Domínio | `Cartao.cs`, `MovimentacaoCartao.cs`, `QuantidadeInvalidaException.cs`, `EstouroDeTotalException.cs`, `PrecoAlteradoException.cs`, `CompraNaoConcluidaException.cs` | Quantidade, total, preço unitário e as recusas antes de alterar o cartão |
| SQLite | `MovimentacaoCartaoConfiguracao.cs`, migration `20261015133903_PrecoUnitarioNaMovimentacao`, snapshot | Coluna anulável `PrecoUnitario` |
| Orquestração | `ICompraServico.cs`, `CompraServico.cs` | Quantidade, preço visto, um `SaveChanges` no sucesso e a falha de gravação |
| HTTP | `CompraRequisicao.cs`, `MovimentacaoResposta.cs`, `MapeadorCartao.cs`, `CartoesController.cs`, `TratadorExcecoes.cs` | Corpo novo, `precoUnitario` na resposta, 400 e 409 |
| Aplicativo | `cartaoApi.ts`, `CartaoContexto.tsx`, `CardProduto.tsx`, `TelaCatalogo.tsx`, `TelaMovimentacoes.tsx`, `tipos/index.ts` | Quantidade no card, aviso com os quatro dados e histórico com unitário e total |
| Testes | `CartaoTestes.cs`, `CompraServicoTestes.cs`, `FalhaGravacaoSqliteTestes.cs`, `CardPlay.Tests.csproj` | Os casos anteriores e um teste que relê o SQLite depois da falha ao salvar. O projeto de teste referencia `Microsoft.EntityFrameworkCore.Sqlite` 10.0.12. Saída em `evidencias/dotnet-test.txt`: 37 passaram |
| Documentos | `docs/product.md`, `docs/ui.md`, `docs/architecture.md`, `README.md`, `AGENTS.md` | Deixam de dizer que toda compra é uma unidade |

---

## Validação

`evidencias/dotnet-test.txt` registra a suíte de 2026-10-15: 37 testes passaram, saída 0. Entre eles, `Comprar_TotalEstouraDecimal_NaoAlteraSaldoNemRegistraCompra`, `ComprarAsync_FalhaAoSalvar_InformaQueNaoConcluiuENaoGrava` e `ComprarAsync_FalhaAoSalvar_RelendoSqlite_MantemSaldoENaoGravaCompra`. `evidencias/tsc.txt` registra `tsc --noEmit` com saída 0 e sem mensagens.

| Critério | Resultado sustentado | O que permanece |
| --- | --- | --- |
| ACE-01 | Café a R$ 8,50, quantidade 3, saldo 100 para 74,5, um registro com unitário 8,5 e valor 25,5. API em `percurso-etapa4/chamadas.txt`. Tela do CP-NRBG9K em `percurso-etapa5/log.txt`. O teste de domínio correspondente está na suíte gravada | O par escrito R$ 20,00 × 3 foi aceito como exemplo |
| ACE-02 | Na web, o aviso do café mostra produto, quantidade 3, unitário R$ 8,50 e total R$ 25,50. Cancelar não chama `POST /compras`. `percurso-etapa5/01-alerta-cafe.png` e `percurso-etapa5/log.txt`. O adesivo em quantidade 3 repetiu os quatro dados em `evidencias/01-alerta-adesivo-total-12.png` | O alerta nativo do telefone segue sem captura. Não há emulador nem `adb` neste ambiente |
| ACE-03 | Os corpos 200 trazem saldo e registro juntos. Depois do café e depois do adesivo que zerou o saldo, o catálogo continuou aberto com o saldo novo. O código grava uma vez | Não há log SQL com `BEGIN` na pasta |
| ACE-04 | API: adesivo R$ 4,00, quantidade 3, saldo 12 para 0, em `chamadas.txt`. Tela: cartão CP-4YC229, recarga R$ 12,00, adesivo quantidade 3, saldo R$ 0,00, em `evidencias/percurso-tela.txt` e `02-saldo-zero-catalogo.png` | O par escrito R$ 60,00 / R$ 20,00 foi aceito como exemplo |
| ACE-05 | Os percursos gravados terminam com saldo 74,5, 0, 10, 20, 16, R$ 74,50, R$ 70,50, R$ 0,00 ou R$ 20,00. A falha ao salvar, relida em outro contexto SQLite, mantém saldo 100 e não grava compra. Teste nomeado em `dotnet-test.txt` | O teste usa um banco temporário. Não aponta para `cardplay.db` |
| ACE-06 | API: saldo 10 contra total 12, HTTP 400, a frase já usada, saldo permanece 10. Tela: fone, total R$ 237,00, a mesma frase, saldo R$ 74,50, sem compra do fone | O par escrito R$ 50,00 / R$ 60,00 foi aceito como exemplo |
| ACE-07 | Quantidades 0, −1 e 1,5 na API: HTTP 400, “Informe uma quantidade inteira maior que zero.”, saldo permanece 20. O estouro em memória passou na suíte. A conta está em `evidencias/estouro.txt` | Não há corpo HTTP de estouro. O maior preço da precisão 18,2 vezes a maior quantidade inteira ainda cabe em `decimal` |
| ACE-08 | Tela do CP-NRBG9K: adesivo, quantidade 1, unitário e total R$ 4,00, saldo de R$ 74,50 para R$ 70,50. O histórico mostra essa compra sem “+”, ao lado da recarga com “+”. O histórico do CP-4YC229 repete nome, total sem “+”, quantidade 3 e preço unitário, com a recarga de R$ 12,00 com “+”. `evidencias/03-historico-saldo-zero.png` | O relógio da tela repete os dígitos do JSON (`17:47`), com o navegador em `America/Sao_Paulo` e o arquivo da captura gravado às 14:47 no relógio local. Registrado em `percurso-tela.txt`. Não houve correção de código |
| ACE-09 | API: preço visto 3 contra 4, HTTP 409, saldo permanece 20; a confirmação seguinte com 4 conclui. Tela do CP-4YC229: o card ainda mostrava R$ 4,00, a API já lia 5,0, a confirmação mostrou “O preço deste produto mudou. Confirme novamente o total atualizado.” e o saldo ficou R$ 20,00, sem compra nova. `evidencias/04-alerta-preco-visto-4.png`, `05-conflito-preco-catalogo.png` e `percurso-tela.txt` | Na tela não houve a confirmação seguinte com o preço novo. O adesivo foi devolvido a 4,0 e `GET /api/produtos` voltou a devolver 4.0 |
| ACE-10 | Mensagem “A compra não foi concluída.” O teste em memória e o teste que relê o SQLite passaram na suíte gravada. Não há interruptor no produto | A resposta HTTP 500 dessa falha continua só no mapeamento do `TratadorExcecoes`, sem chamada capturada |

---

## Ainda aberto

O alerta nativo do telefone, o HTTP de estouro e o log SQL com `BEGIN` continuam sem artefato. A confirmação seguinte na tela, depois do 409, também. A resposta HTTP 500 da falha de gravação continua só no mapeamento do `TratadorExcecoes`.

`docs/backlog.md` agora descreve a quantidade escolhida. O título do arquivo continua a regra das demandas futuras: não implementar até serem pedidas com critérios claros.

O navegador desta sessão ficou no cartão CP-4YC229. O CP-NRBG9K permanece no banco.

## Fora desta entrega

Carrinho, estoque, desconto, frete, parcelamento, banco, autenticação e teto comercial não entram. Cartão inexistente e produto inexistente continuam o 404 da CP-001 e não foram recapturados.
