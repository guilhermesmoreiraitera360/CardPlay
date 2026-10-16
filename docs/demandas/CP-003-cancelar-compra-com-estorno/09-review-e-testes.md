# CP-003 — Revisão e testes

Revisão de leitura, corrigida em 2026-10-16. RN01–RN04 e CA01–CA05 foram aprovadas nesta conversa como texto oficial. O enunciado em `02-historia-rica.md` e `03-criterios-de-aceite.md` não foi reescrito.

Os seis achados da primeira leitura foram tratados. As provas novas estão em `revisao/`. A entrega foi concluída em 2026-10-16, em `10-entrega.md`.

## Aprovação

| Decisão | Registro |
| --- | --- |
| RN01–RN04 e CA01–CA05 | Aprovadas em 2026-10-16 |
| P2.1 | “Não há compra deste cartão para cancelar.” |
| P2.3 | “Esta compra já foi cancelada.” |
| P3.1 | Depois do sucesso o cliente permanece no Histórico |
| P3.2 | Fechado na conclusão: não há aviso novo no aplicativo. A limitação fica em `10-entrega.md` |

## Achados

### 1. A suíte não tinha execução gravada

| Campo | Conteúdo |
| --- | --- |
| Estado | Corrigido como evidência |
| Prova | `revisao/dotnet-test.txt` |
| Resultado | `dotnet test backend/CardPlay.sln` terminou com 50 aprovados, 0 falhas. O arquivo inclui `CancelarAsync_FalhaAoSalvar_RelendoSqlite_MantemSaldoSemMarcaESemEstorno` e `CancelarAsync_CompraDeOutroCartao_NaoGravaNenhumDosDois` |

### 2. A repetição e o outro cartão não fechavam o histórico no HTTP

| Campo | Conteúdo |
| --- | --- |
| Estado | Corrigido como evidência |
| Prova | `revisao/chamadas.txt` |
| Resultado | Cartão A `CP-PZ9LHC` comprou café de R$ 8,50 e ficou com saldo 41,5, compra ainda sem data de cancelamento. O cartão B `CP-EGF5RR` pediu essa compra: HTTP 404, “Não há compra deste cartão para cancelar.” A lista de A continuou sem estorno e sem data de cancelamento. Saldos 41,5 e 10. O cancelamento de A devolveu saldo 50 e um estorno. A repetição respondeu HTTP 409, “Esta compra já foi cancelada.” A lista relida depois do 409 continua com um estorno e a mesma data de cancelamento |

### 3. A falha ao gravar não tinha sido observada

| Campo | Conteúdo |
| --- | --- |
| Estado | Corrigido como evidência do teste. Não houve chamada HTTP 500, porque o produto não tem interruptor para falhar o salvar |
| Prova | `revisao/dotnet-test.txt`, método `CancelarAsync_FalhaAoSalvar_RelendoSqlite_MantemSaldoSemMarcaESemEstorno` |
| Resultado | O método passou. Ele afirma a mensagem “O cancelamento não foi concluído.”, saldo gravado 96, compra sem data de cancelamento e nenhum `CompraId` |

### 4. Backlog, README e arquitetura não descreviam o cancelamento

| Campo | Conteúdo |
| --- | --- |
| Estado | Corrigido |
| Onde | `docs/backlog.md`, `README.md`, `docs/architecture.md` |
| Resultado | Os três passam a descrever o cancelamento da compra concluída, o estorno do total debitado e a rota `POST /api/cartoes/{id}/compras/{compraId}/cancelamento`. **Cancelar** no aviso de **Usar cartão** continua só desistindo da compra |

### 5. O saldo do contexto esperava a lista

| Campo | Conteúdo |
| --- | --- |
| Estado | Corrigido no cancelamento |
| Onde | `CartaoContexto.tsx` |
| Resultado | O saldo e a lista local passam a refletir a resposta do cancelamento antes do `GET /movimentacoes`. Se a lista falha, o saldo novo, a marca e o estorno local permanecem, e a mensagem diz que o cancelamento foi registrado. A falha forçada da lista não foi provocada: não há interruptor no produto. O percurso de sucesso em `revisao/historico-hora-local.png` mostra saldo coerente com o estorno já gravado |

### 6. A hora do histórico repetia os dígitos UTC

| Campo | Conteúdo |
| --- | --- |
| Estado | Corrigido |
| Onde | `MapeadorCartao` marca as datas como UTC. `formatarDataHora` trata data sem fuso como UTC |
| Prova | `revisao/chamadas.txt` e `revisao/fuso.txt`, captura `revisao/historico-hora-local.png` |
| Resultado | A API devolveu `2026-10-16T17:27:54.123683Z`. No navegador em `America/Sao_Paulo`, a tela mostrou 16/10/2026, 14:27 |

`node node_modules/typescript/bin/tsc --noEmit`, a partir de `mobile`, terminou com código 0. A saída está em `revisao/tsc.txt`.

## Falsos positivos

| Suspeita | Por que não é achado |
| --- | --- |
| O estorno foi mostrado como compra, sem “+” | `revisao/historico-hora-local.png` mostra “Estorno” e “+ R$ 8,50” |
| A repetição creditou de novo | `revisao/chamadas.txt` mostra HTTP 409 e, na lista seguinte, um estorno e saldo 50 |
| O pedido do outro cartão marcou a compra | A lista depois do 404 continua com `dataCancelamento` nulo |
| “Cancelar” de “Usar cartão” passou a desfazer compra concluída | Os documentos mantêm esse aviso como desistência da compra |

## O que continua fora

- P3.2 ficou fechado na conclusão da entrega: sem aviso novo no aplicativo.
- Estorno parcial, chargeback, taxa, prazo, banco, autenticação e a CP-004.
- Dois cancelamentos ao mesmo tempo.
- HTTP 500 da falha de gravação e lista interrompida depois do POST 200. Os dois dependeriam de um interruptor que o produto não tem.

A decisão de aceitar a entrega continua humana.
