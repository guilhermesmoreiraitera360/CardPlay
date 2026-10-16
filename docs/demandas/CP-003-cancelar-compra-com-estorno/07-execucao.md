# CP-003 — Acompanhamento de execução

Escrito em 2026-10-16, depois das sete etapas. Não substitui `06-plan.md`. Cada etapa foi aceita na conversa antes da seguinte. Na data desta escrita, as RN e os CA ainda estavam como proposta. A aprovação e a conclusão estão em `09-review-e-testes.md` e `10-entrega.md`.

A saída de `dotnet test` de cada etapa foi vista no terminal e não foi copiada para um arquivo na hora. Os percursos de HTTP e de tela estão em arquivo. Uma execução da suíte com o código já das sete etapas, neste dia, informou: **50 aprovados, 0 falhas**.

| Data | Etapa | O que ficou |
| --- | --- | --- |
| 2026-10-16 | 1 — crédito, marca e estorno no domínio | Aceita. Filtro `CartaoTestes` informou 25 aprovados. Sem migration |
| 2026-10-16 | 2 — colunas no SQLite | Aceita. Solution compilou com 0 avisos. Filtro de domínio mais o teste SQLite da compra informou 26 aprovados |
| 2026-10-16 | 3 — orquestração | Aceita. Filtro `CartaoTestes\|CompraServicoTestes` informou 45 aprovados |
| 2026-10-16 | 4 — HTTP | Aceita. Chamadas em `percurso-etapa4/chamadas.txt` |
| 2026-10-16 | 5 — confirmação no Histórico | Aceita. Percurso em `percurso-etapa5/` |
| 2026-10-16 | 6 — documentos | Aceita. `docs/product.md`, `docs/ui.md` e `AGENTS.md` |
| 2026-10-16 | 7 — falha de gravação no teste | Aceita. Filtro `FalhaGravacaoSqliteTestes\|CompraServicoTestes` informou 22 aprovados |

## Etapa 1 — Domínio

`Cartao.CancelarCompra` localiza a movimentação nesse cartão. Id ausente ou movimentação sem produto lança `CompraNaoCancelavelException` antes de alterar saldo e lista. Data de cancelamento já preenchida lança `CompraJaCanceladaException` antes de alterar. No sucesso, grava `DataCancelamento` em UTC, soma o `Valor` ao saldo e cria o estorno com descrição “Estorno” e `CompraId`. O estorno não preenche `NomeProduto`.

Arquivos: `Cartao.cs`, `MovimentacaoCartao.cs`, `CompraNaoCancelavelException.cs`, `CompraJaCanceladaException.cs`, `CartaoTestes.cs`.

Os testes de domínio cobrem total 20 com saldo 30 terminando em 50, quantidade maior que 1 creditando o total uma vez, repetição sem outro crédito, id ausente ou recarga sem alteração, e crédito que ignora o preço de outro produto.

Entre esta etapa e a migration, `FalhaGravacaoSqliteTestes` falhou com `PendingModelChangesWarning`. A etapa 2 removeu essa pendência. Não houve supressão do aviso.

## Etapa 2 — SQLite

`DataCancelamento` e `CompraId` entram em `MovimentacoesCartao`, anuláveis, `TEXT`. Migration `20261016125128_CancelamentoNaMovimentacao`. Sem chave estrangeira nova.

`cardplay.db` não foi usado como prova nesta etapa. As colunas foram aplicadas quando a API subiu na etapa 4.

## Etapa 3 — Orquestração

`CompraServico.CancelarAsync` carrega o cartão, chama o domínio, acrescenta o estorno e faz um `SalvarAlteracoesAsync`. Cartão ausente continua `CartaoNaoEncontradoException`. Compra de outro cartão usa a mesma recusa de “não há compra deste cartão”, sem busca global. Ainda não envolvia a falha de gravação.

Arquivos: `ICompraServico.cs`, `CompraServico.cs`, `CompraServicoTestes.cs`.

## Etapa 4 — HTTP

`POST /api/cartoes/{id}/compras/{compraId}/cancelamento`, sem corpo. O controlador só encaminha. A resposta da movimentação passa a incluir `dataCancelamento` e `compraId`.

As frases e os status estavam **propostos** no plano. A inspeção desta etapa seguiu com eles: compra inexistente, de outro cartão ou recarga responde 404, título “Recurso não encontrado”, detalhe “Não há compra deste cartão para cancelar.” Compra já cancelada responde 409, título “Conflito”, detalhe “Esta compra já foi cancelada.”

Evidência em `percurso-etapa4/chamadas.txt`, contra `http://localhost:5080`. Cartão A `CP-T6P8MK`, id `96f8f1fc-9d8d-4f89-be7a-028e43fa6cda`:

- Recarga 50 e café de 8,50. Cancelamento: HTTP 200, saldo 50, estorno de 8,5 com `compraId` da compra. A compra ficou com data de cancelamento.
- Repetição: HTTP 409. O saldo permaneceu 50.
- Cartão B `CP-8GKTTY`, saldo 10, pedindo a compra de A: HTTP 404. Saldos permaneceram 50 e 10.
- Id da recarga e id inexistente: o mesmo 404. Saldo final de A: 50.

## Etapa 5 — Histórico

“Cancelar compra” aparece só na compra sem data de cancelamento. O aviso mostra produto, quantidade, preço unitário, total pago e estorno. Desistir não chama a API. Confirmar chama a API e a tela permanece no Histórico. A compra cancelada fica sem “+” e ganha “Cancelada”. O estorno mostra “+” e o nome da compra ligada. Na web, o aviso é na própria tela. O alerta nativo não foi capturado.

`node node_modules/typescript/bin/tsc --noEmit`, a partir de `mobile`, terminou com código 0.

Percurso em `percurso-etapa5/percurso.txt`, `02-confirmacao-web.png`, `03-historico-apos-estorno.png` e `04-saldo-cartao.png`. Cartão `CP-2QZ4PQ`, id `d0034633-e512-431d-976c-c1bf86eb8351`, titular Etapa Cinco. Fuso da tela: `America/Sao_Paulo`.

- Recarga de R$ 20,00 e compra de 3 adesivos a R$ 4,00, total R$ 12,00. Saldo R$ 8,00.
- O preço do adesivo na API foi para 5,0. O aviso continuou com unitário R$ 4,00 e estorno R$ 12,00. Desistir manteve saldo 8 e duas movimentações.
- Confirmar devolveu o saldo a R$ 20,00. O histórico mostrou o estorno com “+”, a compra com “Cancelada” e a recarga. O botão sumiu, então a repetição não foi clicada nesta tela. O 409 está na etapa 4.
- O preço do adesivo foi restaurado para 4,0. `GET /api/produtos` devolveu 4,0.

## Etapa 6 — Documentos

`docs/product.md`, `docs/ui.md` e `AGENTS.md` passam a descrever o cancelamento da compra concluída. **Cancelar** no aviso de **Usar cartão** continua só desistindo da compra. Nenhuma regra nova.

`README.md` e `docs/architecture.md` não estavam na lista desta etapa e continuam sem descrever o cancelamento.

## Etapa 7 — Falha ao gravar

`CancelarAsync` envolve a falha de salvar em `CancelamentoNaoConcluidoException`, mensagem “O cancelamento não foi concluído.” `OperationCanceledException` continua subindo sem esse embrulho. O tratador HTTP não ganhou status novo: a exceção segue o caminho já existente e a resposta é 500, com essa frase no detalhe.

`CancelarAsync_FalhaAoSalvar_RelendoSqlite_MantemSaldoSemMarcaESemEstorno` usa um banco temporário. A compra do adesivo deixa o saldo em 96. O salvar lança `IOException` antes de gravar. Outro contexto lê saldo 96, compra sem data de cancelamento e nenhuma movimentação com `CompraId`. Não há interruptor no produto.

## O que este arquivo não fechava

Na data da escrita, estes pontos ainda estavam em aberto. A conclusão em `10-entrega.md` os encerrou ou os aceitou sem artefato novo.

- P3.2 ficou sem aviso novo no aplicativo.
- As RN e os CA foram aprovados depois, como texto da história.
- `README.md` e `docs/architecture.md` passaram a descrever o cancelamento.
- O alerta nativo continua sem captura e foi aceito assim.
- Dois cancelamentos ao mesmo tempo continuam fora da entrega.
