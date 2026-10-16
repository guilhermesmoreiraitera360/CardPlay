# CP-003 — Critérios de aceite: Cancelar uma compra com estorno integral

## Estado deste documento

| Campo | Valor |
| --- | --- |
| Demanda | CP-003 — Cancelar uma compra com estorno integral |
| Artefato | Critérios de aceite observáveis, para revisão humana |
| Fonte primária | `02-historia-rica.md` |
| Decisões de caminho | `01-entendimento.md`, seção “Decisões tomadas em 2026-10-16” |
| Data | 2026-10-16 |
| Estado da história | **Aprovada** em 2026-10-16. RN01–RN04 e CA01–CA05 são o texto oficial. O enunciado abaixo não foi reescrito. O registro está em `09-review-e-testes.md` |
| O que estes critérios verificam | CA01–CA05 e o efeito observável das decisões já tomadas |
| O que este arquivo não faz | Não escreve código e não inclui estoque. As frases e os status HTTP usados na implementação foram aprovados com os critérios em 2026-10-16 |

Os valores R$ 20, R$ 30 e R$ 50 são o exemplo escrito na história. A mesma regra vale para outro total debitado. O seed não tem produto a R$ 20. “Um estorno” significa um registro desta confirmação, vinculado à compra, não uma recarga.

---

## Cobertura

| Grupo | Critérios | Base |
| --- | --- | --- |
| Fluxo principal | ACE-01, ACE-02, ACE-03 | RN01, RN02, RN03, CA01 |
| Limites | ACE-04, ACE-05, ACE-08 | RN01, RN03, CA02, CA04, P1.4 |
| Erros | ACE-06, ACE-07, ACE-09 | RN04, CA03, CA05 |

---

## Fluxo principal

### ACE-01 — Cancelar devolve o total e marca a compra

| Campo | Conteúdo |
| --- | --- |
| Origem | CA01; RN01; RN03; decisão P1.4 |
| Estado da origem | Proposta, com o crédito decidido em P1.4 |

**Cenário escrito**

- **Dado** uma compra concluída de R$ 20,00 no cartão que está no contexto
- **E** essa compra deixou o saldo em R$ 30,00
- **E** a compra ainda não tem data de cancelamento
- **Quando** o cliente confirma o cancelamento
- **Então** o saldo passa a ser R$ 50,00
- **E** a compra original continua na lista, com o total R$ 20,00
- **E** essa compra fica marcada cancelada
- **E** existe um único estorno de R$ 20,00 vinculado a ela
- **E** o crédito, a marca e o estorno ocorrem juntos

**Mesma regra, sem outro número**

- **Dado** uma compra concluída ainda não cancelada, de qualquer total debitado
- **Quando** o cancelamento se conclui
- **Então** o saldo final é o saldo anterior mais o `Valor` daquela compra
- **E** a compra original permanece marcada cancelada
- **E** há um estorno com esse mesmo valor e o id da compra

### ACE-02 — A confirmação mostra a compra e o valor do estorno

| Campo | Conteúdo |
| --- | --- |
| Origem | RN02; decisão P1.2 |
| Estado da origem | Proposta, com o aviso decidido |

- **Dado** um cartão no contexto e uma compra concluída ainda não cancelada
- **Quando** o cliente toca “Cancelar compra” no Histórico
- **Então** o aviso mostra o nome do produto, a quantidade, o preço unitário, o total pago e o valor do estorno
- **E** o valor do estorno é o total debitado daquela compra
- **E** desistir fecha o aviso e não chama a API
- **E** o saldo e o histórico permanecem os de antes do toque

Sem cartão no contexto, o histórico não oferece cancelamento. Recarga não tem “Cancelar compra”.

### ACE-03 — Crédito, marca e estorno juntos, com a compra original conservada

| Campo | Conteúdo |
| --- | --- |
| Origem | RN03; decisão P1.3 |
| Estado da origem | Proposta, com a marca e o vínculo decididos |

- **Dado** uma compra concluída ainda não cancelada
- **Quando** o cancelamento se conclui
- **Então** o saldo do mesmo cartão aumenta o `Valor` da compra
- **E** a compra conserva produto, quantidade, preço unitário, total e a data em que foi feita
- **E** a compra ganha a data de cancelamento, em UTC
- **E** nasce uma movimentação de descrição “Estorno”, com o mesmo valor e o id dessa compra
- **E** essa movimentação não é uma compra
- **E** no histórico a compra continua sem “+” e mostra “Cancelada”
- **E** o estorno aparece na mesma lista, com “+” e o nome do produto lido pelo vínculo

---

## Limites

### ACE-04 — Repetir não gera outro crédito

| Campo | Conteúdo |
| --- | --- |
| Origem | CA02; RN03; RN04 |
| Estado da origem | Proposta, com a marca decidida em P1.3 |

- **Dado** a compra do ACE-01 já cancelada, com saldo R$ 50,00 e um estorno de R$ 20,00
- **Quando** o mesmo cancelamento é pedido de novo
- **Então** o saldo permanece R$ 50,00
- **E** continua existindo um único estorno vinculado
- **E** não nasce outra marca nem outro crédito

A frase dessa recusa, aprovada em 2026-10-16, é “Esta compra já foi cancelada.”

### ACE-05 — Quantidade maior que 1 estorna o total uma vez

| Campo | Conteúdo |
| --- | --- |
| Origem | Decisão P1.4; RN01 |
| Estado da origem | Decisão. A história não usa este número |

- **Dado** uma compra concluída com quantidade maior que 1
- **E** o `Valor` é o total debitado, preço unitário vezes a quantidade
- **Quando** o cancelamento se conclui
- **Então** o crédito é esse total, uma vez
- **E** não é o preço unitário
- **E** não é um estorno por unidade
- **E** o preço vigente do catálogo não entra na conta

### ACE-06 — Desistir, e preço atual diferente do valor pago

| Campo | Conteúdo |
| --- | --- |
| Origem | CA04; RN02; decisão P1.4 |
| Estado da origem | Proposta, com o valor decidido |

- **Dado** o aviso de cancelamento aberto
- **Quando** o cliente desiste
- **Então** o saldo permanece o anterior
- **E** a compra continua sem data de cancelamento
- **E** não existe estorno

- **Dado** uma compra de R$ 20,00
- **E** o preço atual do mesmo produto no catálogo é R$ 25,00
- **Quando** o cliente confirma o cancelamento
- **Então** o crédito é R$ 20,00
- **E** o estorno registra R$ 20,00

---

## Erros

### ACE-07 — Compra de outro cartão ou inexistente

| Campo | Conteúdo |
| --- | --- |
| Origem | CA03; RN04 |
| Estado da origem | Aprovada. A frase é “Não há compra deste cartão para cancelar.” |

- **Dado** uma compra concluída do cartão A
- **Quando** o cancelamento é pedido com o cartão B
- **Então** a operação é recusada
- **E** o saldo de A e o saldo de B permanecem
- **E** a compra não fica marcada cancelada
- **E** não nasce estorno

- **Dado** um id que não é compra daquele cartão
- **Quando** o cancelamento é pedido
- **Então** a operação é recusada
- **E** o saldo permanece
- **E** não nasce estorno

### ACE-08 — Compra já cancelada

| Campo | Conteúdo |
| --- | --- |
| Origem | RN04; CA02 |
| Estado da origem | Proposta, com a marca decidida |

- **Dado** uma compra que já tem data de cancelamento e um estorno vinculado
- **Quando** chega outro pedido de cancelamento dessa compra
- **Então** não há crédito novo
- **E** o saldo permanece
- **E** o histórico continua com a compra original e um único estorno

Este critério observa o mesmo efeito do ACE-04. O ACE-04 usa o par escrito. Este observa a marca.

### ACE-09 — Falha ao gravar

| Campo | Conteúdo |
| --- | --- |
| Origem | CA05; RN04; decisão P1.5 |
| Estado da origem | Proposta, com o modo de teste decidido |

- **Dado** um cancelamento que passaria nas demais regras
- **E** o salvar, no teste, falha antes de concluir a gravação
- **Quando** a falha ocorre
- **Então** o cliente é informado de que o cancelamento não foi concluído
- **E** o saldo gravado permanece o anterior
- **E** a compra continua sem data de cancelamento
- **E** não há estorno
- **E** não há rastro de tentativa no histórico
- **E** o produto não oferece um interruptor para provocar essa falha

---

## O que a CP-002 continua exigindo

Comprar, recarregar e listar o histórico continuam como já estão. Este aceite não muda a confirmação de “Usar cartão”. Estoque não entra.

## Fora destes critérios

Estorno parcial, chargeback, taxa, prazo, autenticação, autorização de produção e a CP-004 não têm resultado esperado aqui.
