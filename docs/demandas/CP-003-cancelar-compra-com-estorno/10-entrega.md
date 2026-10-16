# CP-003 — Entrega: Cancelar uma compra com estorno integral

## Estado deste documento

| Campo | Valor |
| --- | --- |
| Demanda | CP-003 — Cancelar uma compra com estorno integral |
| Artefato | Síntese para revisão humana |
| Fontes | `02-historia-rica.md`, `03-criterios-de-aceite.md`, `06-plan.md`, `07-execucao.md`, `08-validacao.md`, `09-review-e-testes.md`, pastas `percurso-etapa4/`, `percurso-etapa5/` e `revisao/` |
| Código comparado | Branch `docs/cp-003-cancelar-compra-com-estorno-integral`, a partir do commit `cb0ee53`. A CP-003 está no diretório de trabalho e não tem commit |
| Data da leitura | 2026-10-16 |
| Situação | **Concluída** em 2026-10-16. RN01–RN04 e CA01–CA05 estão aprovadas. O alerta nativo, o log SQL, o HTTP 500 da falha de gravação, a lista interrompida depois do POST 200 e o aviso novo de autorização de produção ficam como limites aceitos, sem artefato novo |

`08-validacao.md` registra o inventário anterior. Onde ele diz que um item não foi observado, `revisao/` prevalece para o que ela de fato contém. “Observado” aqui aponta para arquivo desta demanda.

---

## Problema

A compra concluída não podia ser desfeita. O “Cancelar” existente só fechava a confirmação de “Usar cartão”. A história pede cancelar integralmente uma compra ainda não cancelada, devolver o total debitado ao mesmo cartão e conservar o registro original ligado a um estorno.

As decisões de 2026-10-16 fecham o caminho: a CP-004 não entra; a compra é o id da movimentação no Histórico; a marca é a data de cancelamento; o crédito é o `Valor` gravado, uma vez; a falha de gravação só existe no teste. RN01–RN04 e CA01–CA05 foram aprovadas nesse mesmo dia como texto oficial. As frases aprovadas são “Não há compra deste cartão para cancelar.” e “Esta compra já foi cancelada.” Depois do sucesso o cliente permanece no Histórico.

O par escrito R$ 20,00 / saldo R$ 30,00 / saldo R$ 50,00 é o exemplo da história. O seed não tem produto a R$ 20,00. Café a R$ 8,50 e adesivo a R$ 4,00 observam a mesma regra. O teste de domínio do par escrito está na suíte gravada.

---

## Solução

O cancelamento continua no caminho da compra. `CardPlay.Services` não entra. Não há estoque, estorno parcial, taxa, prazo, chargeback nem interruptor de falha.

1. `Cartao.CancelarCompra` localiza a movimentação nesse cartão. Id ausente, recarga ou compra de outro cartão lança `CompraNaoCancelavelException` antes de alterar saldo e lista. Compra já cancelada lança `CompraJaCanceladaException` antes de alterar. No sucesso, grava `DataCancelamento` em UTC, soma o `Valor` ao saldo e cria um estorno com descrição “Estorno” e `CompraId`. O estorno não preenche `NomeProduto`.
2. `MovimentacoesCartao` ganha `DataCancelamento` e `CompraId`, anuláveis, sem chave estrangeira nova. Migration `20261016125128_CancelamentoNaMovimentacao`.
3. `CompraServico.CancelarAsync` chama o domínio e um `SalvarAlteracoesAsync`. Falha nesse salvar vira `CancelamentoNaoConcluidoException`, mensagem “O cancelamento não foi concluído.”
4. `POST /api/cartoes/{id}/compras/{compraId}/cancelamento` não tem corpo. O sucesso devolve saldo e o estorno. Compra inexistente, de outro cartão ou recarga é HTTP 404, título “Recurso não encontrado”. Compra já cancelada é HTTP 409, título “Conflito”. A falha de gravação não ganhou status novo: exceção não mapeada segue HTTP 500, e o `detail` é a mensagem acima. As datas da resposta saem em UTC.
5. No Histórico, “Cancelar compra” aparece só na compra sem data de cancelamento. O aviso mostra produto, quantidade, preço unitário, total pago e estorno. Cancelar não chama a API. Confirmar chama a API e a tela permanece no Histórico. A compra cancelada continua sem “+” e ganha “Cancelada”. O estorno mostra “+” e o nome da compra ligada. **Cancelar** no aviso de **Usar cartão** continua só desistindo da compra.
6. O saldo do contexto passa a refletir a resposta do cancelamento antes da nova lista. Se a lista falha, o crédito e a marca local permanecem.

---

## Alterações

Tudo abaixo está sem commit, na branch `docs/cp-003-cancelar-compra-com-estorno-integral`.

| Fluxo | Arquivos | O que mudou |
| --- | --- | --- |
| Domínio | `Cartao.cs`, `MovimentacaoCartao.cs`, `CompraNaoCancelavelException.cs`, `CompraJaCanceladaException.cs`, `CancelamentoNaoConcluidoException.cs` | Crédito do total, marca, estorno e as recusas antes de alterar o cartão |
| SQLite | `MovimentacaoCartaoConfiguracao.cs`, migration `20261016125128_CancelamentoNaMovimentacao`, snapshot | Colunas anuláveis `DataCancelamento` e `CompraId` |
| Orquestração | `ICompraServico.cs`, `CompraServico.cs` | Um `SaveChanges` no sucesso e a falha de gravação |
| HTTP | `MovimentacaoResposta.cs`, `MapeadorCartao.cs`, `CartoesController.cs`, `TratadorExcecoes.cs` | Rota sem corpo, datas em UTC, 404 e 409 |
| Aplicativo | `cartaoApi.ts`, `clienteHttp.ts`, `CartaoContexto.tsx`, `TelaMovimentacoes.tsx`, `tipos/index.ts`, `formatacao.ts` | Aviso no Histórico, estorno com “+” e hora local |
| Testes | `CartaoTestes.cs`, `CompraServicoTestes.cs`, `FalhaGravacaoSqliteTestes.cs` | O par escrito, a repetição, o outro cartão e a falha que relê o SQLite. Saída em `revisao/dotnet-test.txt`: 50 passaram |
| Documentos | `docs/product.md`, `docs/ui.md`, `docs/architecture.md`, `docs/backlog.md`, `README.md`, `AGENTS.md` | Passam a descrever o cancelamento da compra concluída |

---

## Validação

`revisao/dotnet-test.txt` registra a suíte de 2026-10-16: 50 testes passaram, saída 0. Entre eles, `CancelarCompra_Saldo30AposCompraDe20_Credita20MarcaECriaUmEstorno`, `CancelarAsync_CompraDeOutroCartao_NaoGravaNenhumDosDois` e `CancelarAsync_FalhaAoSalvar_RelendoSqlite_MantemSaldoSemMarcaESemEstorno`. `revisao/tsc.txt` registra `tsc --noEmit` com saída 0.

| Critério | Resultado sustentado | O que permanece |
| --- | --- | --- |
| ACE-01 | Café de R$ 8,50, saldo 41,5 para 50, compra conservada com data de cancelamento e um estorno de 8,5. `percurso-etapa4/chamadas.txt` e `revisao/chamadas.txt`. O par escrito 20 / 30 / 50 passou na suíte gravada | O par escrito não foi percorrido na API. O seed não tem produto de R$ 20,00 |
| ACE-02 | Na web, o aviso do adesivo mostra produto, quantidade 3, unitário R$ 4,00, total pago R$ 12,00 e estorno R$ 12,00. Cancelar manteve saldo 8 e duas movimentações. `percurso-etapa5/02-confirmacao-web.png` e `percurso-etapa5/percurso.txt` | O alerta nativo do telefone segue sem captura |
| ACE-03 | O corpo 200 traz saldo e estorno juntos. A compra conserva nome, quantidade, unitário e total, e ganha data de cancelamento. Na tela a compra fica sem “+” e com “Cancelada”; o estorno fica com “+”. `percurso-etapa5/03-historico-apos-estorno.png` | Não há log SQL com `BEGIN` na pasta |
| ACE-04 | Repetição HTTP 409, “Esta compra já foi cancelada.” A lista relida continua com um estorno e saldo 50. `revisao/chamadas.txt` | O par escrito de estorno de R$ 20,00 não foi percorrido na API |
| ACE-05 | Três adesivos, total e estorno R$ 12,00, um estorno. `percurso-etapa5/03-historico-apos-estorno.png`. O teste de quantidade maior que 1 está na suíte gravada | Um produto do seed |
| ACE-06 | Desistir manteve saldo 8, sem data de cancelamento e sem estorno. Com o adesivo a 5,0 na API, o crédito foi o total gravado de R$ 12,00. `percurso-etapa5/percurso.txt` | O par escrito R$ 20,00 contra catálogo R$ 25,00 não foi percorrido. O preço do adesivo foi restaurado para 4,0 |
| ACE-07 | Com a compra ainda sem data de cancelamento, o cartão B recebeu HTTP 404. A lista de A continuou sem marca e sem estorno. Saldos 41,5 e 10. `revisao/chamadas.txt` | A mesma frase cobre id inexistente e recarga, já vistos em `percurso-etapa4/chamadas.txt` |
| ACE-08 | O mesmo 409 do ACE-04. A lista depois da repetição conserva a compra original e um estorno | Nenhum segundo crédito nessa lista |
| ACE-09 | Mensagem “O cancelamento não foi concluído.” O teste que relê outro contexto SQLite passou na suíte gravada. Não há interruptor no produto | A resposta HTTP 500 dessa falha continua só no mapeamento do `TratadorExcecoes`, sem chamada capturada |

A hora do histórico, no navegador em `America/Sao_Paulo`, mostrou 16/10/2026, 14:27 para o instante `2026-10-16T17:27:54.123683Z`. `revisao/fuso.txt` e `revisao/historico-hora-local.png`.

---

## Aceito na conclusão

Em 2026-10-16 a entrega foi dada como concluída. Estes pontos permanecem sem artefato novo e foram aceitos assim:

- O alerta nativo do telefone. A web tem o modal.
- O log SQL com `BEGIN`.
- A chamada HTTP 500 de “O cancelamento não foi concluído.” O teste que relê o SQLite cobre a falha de gravação.
- A lista interrompida depois do POST 200. O código aplica o saldo e a marca antes dessa lista.
- P3.2: não há aviso novo no aplicativo. A limitação de autorização fica neste documento. Isto não é autorização de produção.

O navegador desta sessão ficou no cartão CP-PZ9LHC. O CP-2QZ4PQ e o CP-T6P8MK permanecem no banco.

## Fora desta entrega

Estorno parcial, chargeback, taxa, prazo, banco, autenticação, dois cancelamentos ao mesmo tempo e a CP-004 não entram. Estoque não volta.
