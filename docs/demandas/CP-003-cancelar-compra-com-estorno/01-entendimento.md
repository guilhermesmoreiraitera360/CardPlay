# CP-003 — Entendimento: Cancelar uma compra com estorno integral

## Metadados

| Campo | Valor |
| --- | --- |
| Demanda | CP-003 — Cancelar uma compra com estorno integral |
| Tipo | História para implementação no laboratório |
| Fonte principal | Texto da história colado na conversa de 2026-10-16. Não há PDF desta demanda na pasta |
| Fontes de contraste | Produto depois da CP-002: `docs/product.md`, `docs/ui.md`, `AGENTS.md`, `MovimentacaoCartao.cs`, `CartoesController.cs` |
| Status do entendimento | Dúvidas P1 fechadas em 2026-10-16, nesta conversa. RN e CA continuam o texto da história, ainda não aprovados |
| Data | 2026-10-16 |
| Documento recebido | Preservado nesta conversa; não há arquivo-fonte separado para alterar |
| Próxima etapa sugerida | `02-historia-rica.md`, depois que as dúvidas P1 forem lidas. Sem código |

---

## Problema e resultado desejado

| Elemento | Conteúdo | Classificação | Fonte |
| --- | --- | --- | --- |
| Usuário | Cliente do CardPlay | **Fato** (história) | Texto CP-003 |
| Valor | Corrigir uma compra sem perder a rastreabilidade | **Fato** (história) | Texto CP-003 |
| Problema | Desfazer integralmente uma compra concluída, devolver exatamente o valor debitado ao mesmo cartão e preservar o histórico | **Fato** (história) | Texto CP-003 |
| Resultado desejado | Cancelar uma compra e receber o valor de volta no cartão, com a compra original conservada e o estorno rastreável | **Fato** (história) | Texto CP-003 |

**História:**  
Como cliente do CardPlay, quero cancelar uma compra e receber seu valor de volta no cartão, para corrigir uma compra sem perder a rastreabilidade.

**Descrição:** desfazer integralmente uma compra concluída no ambiente didático, preservando o histórico e devolvendo exatamente o valor debitado ao mesmo cartão.

**Prioridade didática escrita na história:** 2. Explora transação inversa, rastreabilidade e repetição segura. Exige decisões de negócio antes da implementação.

---

## Fatos conhecidos (com fonte)

### Da história CP-003

| ID | Fato | Fonte |
| --- | --- | --- |
| F-H1 | O cancelamento é integral e só vale para compra concluída que ainda não foi cancelada. A compra é identificada a partir do cartão usado. Estorno parcial não é permitido | RN01 |
| F-H2 | Neste recorte didático não há prazo de expiração nem taxa | RN01 |
| F-H3 | Antes de confirmar, o cliente vê a compra e o valor do estorno. Desistir dessa confirmação não altera dados | RN02 |
| F-H4 | O valor devolvido é o valor original pago, não o preço atual do catálogo | RN02 |
| F-H5 | O crédito no mesmo cartão, a marcação da compra como cancelada e o registro de estorno vinculado à compra ocorrem juntos. O registro original da compra é conservado | RN03 |
| F-H6 | Repetir o pedido não concede outro crédito | RN03 |
| F-H7 | Compra inexistente, compra de outro cartão, compra já cancelada ou falha na persistência não geram crédito novo | RN04 |
| F-H8 | Não há autenticação real. A operação usa dados de teste e a limitação de autorização deve ser reconhecida. Segurança de produção não deve ser declarada | RN04 |
| F-H9 | CA01: depois de uma compra de R$ 20 que deixou saldo R$ 30, cancelar resulta em saldo R$ 50, a compra marcada cancelada e um estorno de R$ 20 vinculado | CA01 |
| F-H10 | CA02: repetir o cancelamento da mesma compra mantém saldo R$ 50 e um único estorno | CA02 |
| F-H11 | CA03: compra do cartão A pedida a partir do cartão B, ou compra inexistente, é recusada, sem alterar saldos e sem criar estorno | CA03 |
| F-H12 | CA04: desistir antes de confirmar preserva saldo e histórico. Uma compra de R$ 20 cujo preço atual no catálogo é R$ 25 devolve R$ 20 ao confirmar | CA04 |
| F-H13 | CA05: falha ao gravar qualquer componente da operação preserva saldo e estado anteriores, sem estorno parcial persistido | CA05 |
| F-H14 | Fora do escopo: estorno parcial, chargeback bancário, taxas, prazo comercial e autorização de produção | Texto CP-003 |
| F-H15 | Dependência: a compra precisa ser identificável e o valor original pago precisa ser verificável | Texto CP-003 |
| F-H16 | Se a CP-004 for escolhida, as unidades voltam ao estoque no mesmo cancelamento, na quantidade original, uma única vez e na mesma transação lógica | Texto CP-003, integração |

### Do produto depois da CP-002 (contraste; não é regra da CP-003)

| ID | Fato | Fonte |
| --- | --- | --- |
| F-P1 | Já existe compra com saldo: a quantidade é escolhida, a confirmação mostra produto, quantidade, preço unitário e total, e o saldo diminui o total | `docs/product.md` |
| F-P2 | O registro da compra guarda produto, quantidade, preço unitário e total. `Valor` é o total debitado. A descrição gravada é “Compra” | `AGENTS.md`; `MovimentacaoCartao.CriarCompra` |
| F-P3 | Recarga e compra ficam na mesma lista do cartão. A tela mostra a compra sem “+” e a recarga com “+” | `docs/ui.md`; `AGENTS.md` |
| F-P4 | A compra nasce em `POST /api/cartoes/{id}/compras`. A listagem é `GET /api/cartoes/{id}/movimentacoes`. Não há rota de cancelamento nem de estorno | `CartoesController.cs` |
| F-P5 | `MovimentacaoCartao` não tem marca de cancelada nem vínculo com outra movimentação | `MovimentacaoCartao.cs` |
| F-P6 | No produto de hoje, “cancelar” na compra é desistir da confirmação de “Usar cartão”. Isso não desfaz uma compra concluída | `docs/ui.md` |
| F-P7 | A CP-001 deixou cancelamento e estorno fora daquela entrega | `docs/demandas/CP-001-compra-com-saldo/01-entendimento.md`, F-H8 |
| F-P8 | Sem autenticação, estoque, carrinho, checkout ou banco | `docs/product.md`; `AGENTS.md` |
| F-P9 | Dinheiro no servidor é `decimal`. Datas em UTC | `AGENTS.md` |
| F-P10 | A falha de gravação da compra, na CP-002, é provocada no teste, sem interruptor no produto | `docs/demandas/CP-002-comprar-varias-unidades/07-decisoes.md`, P1.4 |
| F-P11 | O cartão usado na compra do aplicativo é o que está no contexto | `docs/ui.md`; `AGENTS.md` |

### Contraste explícito

| O que a CP-003 diz | O que o produto faz hoje |
| --- | --- |
| Cancelar uma compra concluída e devolver o valor | “Cancelar” só fecha a confirmação de uma compra que ainda não ocorreu |
| Conservar a compra e marcá-la cancelada, com um estorno vinculado | O registro não tem marca nem vínculo |
| Repetir o cancelamento não gera outro crédito | Não há cancelamento para repetir |
| Devolver o valor original pago, não o preço atual | O total pago está em `Valor`. O catálogo continua com o preço vigente, separado |
| Compra de outro cartão é recusada | A compra já pertence a um `CartaoId`. Não há operação que cruze cartões |
| Falha de gravação não deixa estorno parcial | A compra já grava saldo e registro juntos. O estorno ainda não existe |
| Estoque volta só se a CP-004 estiver escolhida | Não há estoque nesta versão |

---

## Escopo (in / out) — só o escrito na história

### In

- Cancelamento integral de uma compra concluída que ainda não foi cancelada.
- Identificação da compra a partir do cartão usado.
- Confirmação que mostra a compra e o valor do estorno. Desistir não altera saldo nem histórico.
- Crédito exatamente do valor original pago, no mesmo cartão, mesmo se o preço do catálogo mudou.
- No mesmo passo: crédito, marca de cancelada e um registro de estorno vinculado. A compra original permanece.
- Repetição do mesmo cancelamento sem segundo crédito e com um único estorno.
- Recusa, sem crédito e sem estorno, quando a compra não existe, é de outro cartão, já está cancelada ou a gravação falha.
- Ambiente didático, com dados de teste, sem declarar segurança de produção.

### Out

- Estorno parcial
- Chargeback bancário
- Taxa
- Prazo comercial ou prazo de expiração
- Autorização de produção
- Estoque, enquanto a CP-004 não tiver sido escolhida. Esta conversa ainda não a escolhe

> A história diz que, se a CP-004 for escolhida, a quantidade original volta ao estoque uma única vez, no mesmo cancelamento. Não diz que essa escolha já foi feita aqui.

---

## Regras e critérios escritos na história

Classificação: **fato do texto recebido**. Não é aceite aprovado para implementação.

### Regras

| ID | Regra |
| --- | --- |
| RN01 | Cancelamento integral de compra concluída ainda não cancelada, identificada a partir do cartão usado. Sem estorno parcial, sem prazo e sem taxa neste recorte |
| RN02 | Mostrar a compra e o valor do estorno e pedir confirmação. Desistir não altera dados. O valor é o original pago, não o preço atual do catálogo |
| RN03 | Crédito no mesmo cartão, marcação da compra cancelada e registro de estorno vinculado nascem juntos. O registro original fica. A repetição não gera outro crédito |
| RN04 | Compra inexistente, de outro cartão, já cancelada ou falha de persistência não geram crédito novo. Sem autenticação real, só dados de teste, sem declarar segurança de produção |

### Critérios

| ID | Critério |
| --- | --- |
| CA01 | Compra de R$ 20 com saldo resultante R$ 30. Depois do cancelamento, saldo R$ 50, compra marcada cancelada e um estorno de R$ 20 vinculado |
| CA02 | Repetir o cancelamento mantém saldo R$ 50 e um único estorno |
| CA03 | Compra do cartão A pedida pelo cartão B, ou compra inexistente, é recusada. Saldos intactos. Sem estorno |
| CA04 | Desistir antes de confirmar preserva saldo e histórico. Compra de R$ 20 com preço atual R$ 25 devolve R$ 20 |
| CA05 | Falha ao gravar qualquer componente preserva saldo e estado anteriores. Sem estorno parcial persistido |

---

## Hipóteses (explícitas, não aprovadas)

| ID | Hipótese | Por que é hipótese |
| --- | --- | --- |
| H1 | O estorno entra na mesma lista de histórico das recargas e das compras | A história manda preservar o histórico e não descreve uma segunda lista |
| H2 | A compra é identificada pelo id da movimentação daquela compra | A história diz “identificada a partir do cartão usado” e não nomeia o identificador. O registro já tem `Id` e `CartaoId` |
| H3 | O valor creditado é o `Valor` daquela compra, o total debitado, também quando a quantidade é maior que 1 | RN02 e a descrição mandam devolver o valor debitado. Os exemplos são uma compra de R$ 20, sem quantidade |
| H4 | A falha de gravação é provocada no teste, como na CP-002, sem interruptor no produto | CA05 exige a falha. O texto não descreve o meio. A CP-002 decidiu isso só para a compra |
| H5 | O cliente começa o cancelamento no histórico da compra | A história não escolhe a tela. O histórico é onde a compra concluída aparece hoje |

Nenhuma hipótese acima era decisão quando foi escrita. Em 2026-10-16, H1 a H5 foram fechadas na seção seguinte.

---

## Dúvidas abertas priorizadas

### P1 — Bloqueantes

| ID | Pergunta | Por que importa | Quem decide |
| --- | --- | --- | --- |
| P1.1 | Esta demanda inclui a CP-004, ou o estoque fica fora até uma escolha explícita? | A integração manda devolver a quantidade original na mesma operação. Sem estoque, esse passo não existe | Responsável pelo negócio, não nomeado no texto |
| P1.2 | Qual identificador da compra o cliente e a API usam, e em qual tela a confirmação aparece? | RN01 pede identificar a compra a partir do cartão. O produto não tem ação de cancelar uma compra concluída | Responsável pelo negócio, não nomeado no texto |
| P1.3 | Como a compra fica “marcada cancelada” sem deixar de existir, e o que o estorno vinculado mostra? | RN03 pede marca, registro original conservado e estorno vinculado. O registro atual não tem esses dados | Responsável pelo negócio, não nomeado no texto |
| P1.4 | O crédito é sempre o total debitado daquela compra, inclusive se a quantidade for maior que 1? | Os exemplos usam R$ 20. O produto debita preço vezes quantidade | Responsável pelo negócio, não nomeado no texto |
| P1.5 | Como a falha de persistência da CA05 é provocada? | A história exige que nenhum componente fique gravado sozinho. Não descreve o meio | Responsável pelo negócio, não nomeado no texto |

### P2 — Importantes

| ID | Pergunta | Por que importa | Quem decide |
| --- | --- | --- | --- |
| P2.1 | Quais frases a API e a tela usam para desistência, compra inexistente, compra de outro cartão, compra já cancelada e falha de gravação? | RN04 e CA02–CA05 exigem recusa ou informação. Não fixam o texto | Responsável pelo negócio, não nomeado no texto |
| P2.2 | O estorno no histórico leva “+”, o nome do produto e o vínculo visível com a compra? | RN03 pede o vínculo. A tela hoje só separa recarga com “+” e compra sem “+” | Responsável pelo negócio, não nomeado no texto |
| P2.3 | A repetição informa que a compra já foi cancelada, ou só devolve o estado já gravado? | CA02 fixa saldo e um único estorno. Não fixa a mensagem | Responsável pelo negócio, não nomeado no texto |

### P3 — Refinamentos

| ID | Pergunta | Por que importa | Quem decide |
| --- | --- | --- | --- |
| P3.1 | Depois do cancelamento, o cliente permanece no histórico? | A história não muda o destino. Não bloqueia crédito nem registro | Responsável pelo negócio, não nomeado no texto |
| P3.2 | A limitação de autorização fica só registrada na entrega, ou também visível no aplicativo? | RN04 manda reconhecer o limite e não declarar segurança de produção. Não escolhe o lugar do aviso | Responsável pelo negócio, não nomeado no texto |

---

## Decisões tomadas em 2026-10-16

Autorizadas nesta conversa para desbloquear a história rica. Não alteram o texto da história. Não são código.

| ID | Decisão |
| --- | --- |
| P1.1 | A CP-004 não entra neste cancelamento. Estoque não volta. A integração escrita na história fica de fora até uma escolha explícita |
| P1.2 | A compra é o id da movimentação de compra. A ação fica no Histórico, no item ainda não cancelado, com o botão “Cancelar compra”. O aviso mostra o nome do produto, a quantidade, o preço unitário, o total pago e o valor do estorno. Desistir não chama a API. Confirmar envia o id do cartão que está no contexto e o id dessa compra |
| P1.3 | A compra original permanece, com produto, quantidade, preço unitário, total e data. A marca é a data de cancelamento, em UTC, no próprio registro; vazia significa não cancelada. O estorno é outra movimentação do mesmo cartão, com valor igual ao total da compra, descrição “Estorno” e o id da compra. Não é uma compra. No histórico, a compra cancelada continua sem “+” e ganha a linha “Cancelada”. O estorno entra na mesma lista, com “+”, o nome do produto lido pelo vínculo e o valor devolvido |
| P1.4 | O crédito é o total debitado daquela compra, o `Valor`, também quando a quantidade é maior que 1. Há um estorno da compra inteira, não um estorno por unidade |
| P1.5 | A falha de gravação é provocada no teste, no momento de salvar, sem interruptor no produto. Não grava crédito, nem marca, nem estorno. Não fica rastro de tentativa. O cliente é informado de que o cancelamento não foi concluído |

## Decisões necessárias

| Decisão | Por que | Quem decide | Status |
| --- | --- | --- | --- |
| Aprovar RN01–RN04 e CA01–CA05 como aceite, ou listar o que muda | O texto ainda é história de laboratório, não aceite fechado neste arquivo | Responsável pelo negócio, não nomeado no texto | **Fechada** em 2026-10-16. Aprovadas como texto oficial, registradas em `09-review-e-testes.md` |
| Dizer se a CP-004 entra neste cancelamento | Desbloqueia ou tira o estoque da mesma operação | Esta conversa | **Fechada** em P1.1 |
| Definir identificação da compra e o lugar da confirmação | Desbloqueia RN01 e RN02 | Esta conversa | **Fechada** em P1.2 |
| Definir a marca de cancelada e o registro de estorno | Desbloqueia RN03, CA01 e CA02 | Esta conversa | **Fechada** em P1.3 |
| Confirmar que o crédito é o total debitado | Desbloqueia CA01 quando a quantidade não é 1 | Esta conversa | **Fechada** em P1.4 |
| Definir como simular a falha de gravação | Desbloqueia CA05 | Esta conversa | **Fechada** em P1.5 |

---

## O que não foi decidido / fora desta etapa

- **Não decidido neste arquivo antes de 2026-10-16:** aprovação da história; inclusão da CP-004; identificador da compra; tela da confirmação; forma da marca e do estorno; crédito quando a quantidade é maior que 1; meio da falha de gravação. Esses pontos P1 foram fechados na seção “Decisões tomadas em 2026-10-16”. P2.2 foi fechado junto com P1.3. Na revisão do mesmo dia, RN01–RN04 e CA01–CA05 foram aprovadas. As frases de P2.1 e P2.3 são as que a API já devolvia: “Não há compra deste cartão para cancelar.” e “Esta compra já foi cancelada.” P3.1 fecha com a permanência no Histórico. P3.2 fecha na conclusão da entrega: sem aviso novo no aplicativo. A entrega está concluída em `10-entrega.md`.
- **Fora desta etapa:** código, API, migration, tela, teste e alteração dos documentos de produto.
- **Não inventado aqui:** estorno parcial, taxa, prazo, chargeback, autenticação, autorização de produção, estoque sem a CP-004.
- **Perguntas não são decisões.** Permanecem abertas até registro explícito.
