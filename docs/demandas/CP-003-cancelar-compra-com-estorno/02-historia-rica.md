# CP-003 — História rica: Cancelar uma compra com estorno integral

## Estado deste documento

| Campo | Valor |
| --- | --- |
| Demanda | CP-003 — Cancelar uma compra com estorno integral |
| Artefato | História rica para revisão |
| Fontes | `01-entendimento.md`; texto da história recebido em 2026-10-16; produto depois da CP-002 |
| Data | 2026-10-16 |
| Decisões | As da seção “Decisões tomadas em 2026-10-16” de `01-entendimento.md`, autorizadas nesta conversa |
| Estado das RN/CA | **Aprovadas** em 2026-10-16 como texto oficial. O enunciado abaixo não foi reescrito. O registro está em `09-review-e-testes.md` |
| Critérios | CA01–CA05 permanecem os da história. Não são reescritos aqui |
| Documentos preservados | O texto original das dúvidas em `01-entendimento.md` continua. O status delas foi atualizado |

Legenda: **Fato** · **Proposta** · **Decisão** · **Conflito** · **Lacuna** · **Hipótese** · **Pergunta**.

---

## Problema e objetivo

| Elemento | Enunciado | Estado | Fonte |
| --- | --- | --- | --- |
| Problema | Uma compra concluída não pode ser desfeita. O “cancelar” de hoje só fecha a confirmação antes da compra | **Fato** do produto + demanda da história | `docs/ui.md`; texto CP-003 |
| Objetivo | Cancelar integralmente uma compra concluída, devolver o valor debitado ao mesmo cartão e conservar o rastro | **Proposta** | Texto CP-003 |
| Resultado | Saldo creditado uma vez, compra original marcada cancelada e um estorno vinculado | **Proposta** | Texto CP-003 |

**História:**  
Como cliente do CardPlay, quero cancelar uma compra e receber seu valor de volta no cartão, para corrigir uma compra sem perder a rastreabilidade.

---

## Usuário e valor

| Elemento | Enunciado | Estado | Fonte |
| --- | --- | --- | --- |
| Usuário | Cliente do CardPlay | **Fato** da história. Sem perfil novo e sem autenticação | Texto CP-003 |
| Valor | Corrigir uma compra sem perder a rastreabilidade | **Proposta** | Texto CP-003 |
| Quem aprova a história | Não nomeado no texto. As decisões de caminho foram tomadas nesta conversa | **Lacuna** de dono formal; **Decisão** de caminho | `01-entendimento.md` |

---

## Contexto confirmado

### O que a CP-003 reaproveita

| ID | Contexto | Estado | Fonte |
| --- | --- | --- | --- |
| C1 | Compra concluída com produto, quantidade, preço unitário e total. `Valor` é o total debitado | **Fato** | `AGENTS.md`; `MovimentacaoCartao` |
| C2 | A compra pertence a um cartão. O aplicativo usa o cartão do contexto | **Fato** | `MovimentacaoCartao.CartaoId`; `docs/ui.md` |
| C3 | Recarga e compra na mesma lista. Compra sem “+”. Recarga com “+” | **Fato** | `docs/ui.md` |
| C4 | Dinheiro no servidor em `decimal`. Datas em UTC | **Fato** | `AGENTS.md` |
| C5 | Falha de gravação da compra já é provocada no teste, sem interruptor no produto | **Fato** da CP-002. **Decisão** P1.5 repete esse meio no cancelamento | `07-decisoes.md` da CP-002; P1.5 |

### O que o produto faz e a CP-003 muda

| ID | Contexto | Estado | Fonte |
| --- | --- | --- | --- |
| C6 | “Cancelar” não desfaz compra concluída | **Fato**. **Decisão** P1.2: o cancelamento da compra nasce no Histórico | `docs/ui.md`; P1.2 |
| C7 | O registro não tem marca de cancelada nem vínculo | **Fato**. **Decisão** P1.3: a marca é a data de cancelamento; o estorno guarda o id da compra | P1.3 |
| C8 | Não há crédito de estorno | **Fato**. **Decisão** P1.4: o crédito é o `Valor` da compra, uma vez | P1.4 |
| C9 | A CP-001 deixou cancelamento e estorno fora | **Fato**. Esta história os traz, no recorte sem taxa, sem prazo e sem banco | `01-entendimento.md` da CP-001, F-H8; RN01 |

---

## Escopo

### Dentro

- Cancelamento integral de uma compra concluída ainda não cancelada.
- A compra é o id da movimentação. A ação está no Histórico do cartão que está no contexto.
- O aviso mostra nome, quantidade, preço unitário, total pago e valor do estorno. Desistir não chama a API.
- O crédito é o total debitado, também quando a quantidade é maior que 1. Não usa o preço atual do catálogo.
- No mesmo gravar: crédito no mesmo cartão, data de cancelamento na compra original e uma movimentação de estorno vinculada. A compra original permanece.
- A repetição não gera outro crédito nem outro estorno.
- Compra inexistente, de outro cartão, já cancelada ou falha ao salvar não alteram saldo nem histórico.
- Dados de teste, sem declarar segurança de produção.

Estado das linhas de negócio: **Proposta**. Estado do caminho: **Decisão** de 2026-10-16.

### Fora

- Estorno parcial, de uma unidade só ou de parte do total
- Chargeback bancário, taxa e prazo comercial
- Autenticação e autorização de produção
- Estoque, porque a CP-004 não entra neste cancelamento
- Cancelar recarga
- Interruptor de falha no produto

---

## Regras

| ID | Regra | Estado |
| --- | --- | --- |
| RN01 | Cancelamento integral de compra concluída ainda não cancelada, identificada pelo id da movimentação a partir do cartão do contexto. Sem estorno parcial, sem prazo e sem taxa | **Aprovada**. Identificador **decidido** em P1.2 |
| RN02 | No Histórico, “Cancelar compra” mostra a compra e o valor do estorno e pede confirmação. Desistir não altera dados. O valor é o total debitado, não o preço atual do catálogo | **Aprovada**. Tela e conteúdo **decididos** em P1.2 e P1.4 |
| RN03 | Crédito no mesmo cartão, data de cancelamento na compra e um estorno vinculado nascem juntos. A compra original fica. A repetição não gera outro crédito | **Aprovada**. Marca e vínculo **decididos** em P1.3 |
| RN04 | Compra inexistente, de outro cartão, já cancelada ou falha ao salvar não geram crédito novo. Sem autenticação real. Sem declarar segurança de produção | **Aprovada**. Falha de teste **decidida** em P1.5 |

---

## Fluxo

| Passo | O que acontece | Estado |
| --- | --- | --- |
| 1 | O cartão é o que já está no contexto. Sem cartão, o histórico não oferece cancelamento | **Decisão** P1.2, sobre o fluxo já existente |
| 2 | No item de uma compra ainda não cancelada, o cliente toca “Cancelar compra” | **Decisão** P1.2 |
| 3 | O aviso mostra nome, quantidade, preço unitário, total pago e valor do estorno, iguais ao total debitado | **Decisão** P1.2 e P1.4; **Proposta** RN02 |
| 4 | Desistir fecha o aviso e não chama a API. Saldo e histórico ficam como estavam | **Decisão** P1.2; **Proposta** RN02 e CA04 |
| 5 | Confirmar envia o id do cartão e o id da compra | **Decisão** P1.2 |
| 6 | Se a compra não existe, não é daquele cartão ou já tem data de cancelamento, a operação é recusada. Saldo e histórico não mudam | **Proposta** RN04 e CA02–CA03 |
| 7 | Se a compra vale, o saldo soma o `Valor` original. A compra ganha a data de cancelamento. Nasce um estorno com esse valor e o id da compra. Os três só permanecem se o gravar concluir | **Decisão** P1.3 e P1.4; **Proposta** RN03 |
| 8 | A compra continua na lista, sem “+”, com a linha “Cancelada”. O estorno entra na mesma lista, com “+” e o nome do produto obtido pelo vínculo | **Decisão** P1.3 |
| 9 | Um segundo pedido da mesma compra encontra a marca e não credita de novo | **Proposta** RN03 e CA02; marca **decidida** em P1.3 |

---

## Limites e exceções

| Situação | Efeito | Fonte |
| --- | --- | --- |
| Compra de R$ 20, saldo ficou R$ 30 | Cancelar deixa saldo R$ 50, a compra marcada cancelada e um estorno de R$ 20 vinculado | CA01 |
| Repetir esse cancelamento | Saldo permanece R$ 50. Continua um único estorno | CA02 |
| Compra do cartão A pedida com o cartão B | Recusa. Saldos intactos. Sem estorno | CA03 |
| Compra inexistente | Recusa. Sem alteração e sem estorno | CA03 |
| Desistir no aviso | Saldo e histórico permanecem | CA04 |
| Compra de R$ 20 e preço atual do catálogo R$ 25 | O estorno é R$ 20 | CA04; P1.4 |
| Quantidade maior que 1 | Um crédito do total debitado, não do preço unitário e não por unidade | P1.4 |
| Falha ao salvar | Saldo, marca e histórico anteriores. Sem estorno parcial. O cancelamento não foi concluído | CA05; P1.5 |
| Recarga | Não é compra. Não há “Cancelar compra” | RN01; P1.2 |
| CP-004 | Não entra. Nenhuma unidade volta ao estoque | P1.1 |

Os pares de R$ 20, R$ 30 e R$ 50 são o exemplo escrito na história. A mesma regra vale para outro total debitado. O seed não precisa ter um produto de R$ 20.

Frase decidida para a falha de gravação: o cancelamento não foi concluído.

Frases aprovadas em 2026-10-16, com as RN e os CA: compra inexistente ou de outro cartão responde “Não há compra deste cartão para cancelar.” A compra já cancelada responde “Esta compra já foi cancelada.”

---

## O que a compra original conserva

A compra cancelada continua com nome, quantidade, preço unitário, total sem “+” e a data em que foi feita. Ganha a data de cancelamento e, na tela, a linha “Cancelada”. O estorno não substitui essa linha.

---

## Conflitos que estas decisões encerram

| Conflito | Encerramento |
| --- | --- |
| “Cancelar” hoje só desiste da confirmação de compra | O cancelamento de compra concluída é outra ação, no Histórico |
| A história pede marca e registro original ao mesmo tempo | A marca é a data no registro que já existe. O estorno é outra linha |
| O preço do catálogo pode mudar depois da compra | O crédito usa o `Valor` gravado na compra |
| A CP-003 cita estoque se a CP-004 for escolhida | Nesta conversa a CP-004 não foi escolhida |

---

## Lacunas que continuam

| ID | Lacuna | Por que não bloqueia o caminho |
| --- | --- | --- |
| P3.2 | Onde aparece o aviso de que não há autorização de produção | Fechado na conclusão: sem aviso novo no aplicativo. O registro está em `10-entrega.md` |

---

## O que continua fora deste arquivo

- Código, contrato HTTP detalhado, migration e teste.
- Aprovação formal da história. O caminho foi decidido nesta conversa.
- CP-004 e estoque.

Próxima etapa sugerida: `03-criterios-de-aceite.md`, derivando CA01–CA05 com as decisões acima, ainda sem código.
