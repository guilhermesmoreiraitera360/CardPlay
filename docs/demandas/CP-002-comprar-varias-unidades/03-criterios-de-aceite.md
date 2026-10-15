# CP-002 — Critérios de aceite: Comprar várias unidades de um produto

## Estado deste documento

| Campo | Valor |
| --- | --- |
| Demanda | CP-002 — Comprar várias unidades de um produto |
| Artefato | Critérios de aceite observáveis, para revisão humana |
| Fonte primária | `02-historia-rica.md` |
| Decisões de caminho | `01-entendimento.md`, seção “Decisões tomadas em 2026-10-15” |
| Data | 2026-10-15 |
| Estado da história | **Aprovada** em 2026-10-15. RN01–RN04 e CA01–CA05 são o texto oficial. O registro está em `07-decisoes.md` |
| O que estes critérios verificam | CA01–CA05 e o efeito observável das decisões já tomadas |
| O que este arquivo não faz | Não escreve código, não escolhe nome de campo além do que a decisão já chamou de `Valor` e de preço unitário, e não cria teto comercial |

Os valores R$ 100, R$ 60, R$ 50 e R$ 20 são o exemplo escrito na história. Em 2026-10-15 esse exemplo foi aceito como ilustração: o seed não tem produto a R$ 20, e café a R$ 8,50 e adesivo a R$ 4,00 observam a mesma regra. “Exatamente uma compra” significa um registro desta confirmação, não um histórico vazio de recargas.

---

## Cobertura

| Grupo | Critérios | Base |
| --- | --- | --- |
| Fluxo principal | ACE-01, ACE-02, ACE-03 | RN01, RN02, RN03, CA01 |
| Limites | ACE-04, ACE-05, ACE-08 | RN01, RN03, CA02, CA04 |
| Erros | ACE-06, ACE-07, ACE-09, ACE-10 | RN02, RN04, CA03, CA04, CA05 |

---

## Fluxo principal

### ACE-01 — Várias unidades debitam o total e geram um registro

| Campo | Conteúdo |
| --- | --- |
| Origem | CA01; RN01; RN02; RN03 |
| Estado da origem | Proposta, com registro decidido em P1.2 |

**Cenário escrito**

- **Dado** um cartão existente com saldo de R$ 100,00 e um produto existente a R$ 20,00
- **E** o cliente escolheu a quantidade 3
- **E** viu o produto, a quantidade 3, o preço unitário R$ 20,00 e o total R$ 60,00
- **E** a confirmação envia a quantidade 3 e o preço unitário R$ 20,00, igual ao preço vigente
- **Quando** o cliente confirma
- **Então** a compra fica concluída
- **E** o saldo passa a ser R$ 40,00
- **E** existe um único registro desta compra
- **E** o débito e o registro ocorrem juntos
- **E** o registro contém o produto, a quantidade 3, o preço unitário aplicado R$ 20,00, o total R$ 60,00, a data e a hora
- **E** o total debitado é o `Valor` do registro

**Mesma regra, sem outro número**

- **Dado** saldo anterior maior que o preço vigente vezes a quantidade
- **E** a quantidade é inteira, maior ou igual a 1, e o preço confirmado é o vigente
- **Quando** a compra se conclui
- **Então** o saldo final é o saldo anterior menos esse total
- **E** há um registro com essa quantidade, esse preço unitário e esse total

### ACE-02 — A confirmação mostra quantidade, unitário e total

| Campo | Conteúdo |
| --- | --- |
| Origem | RN02; decisão P2.1 |
| Estado da origem | Proposta, com o aviso decidido |

- **Dado** um cartão no contexto, um produto do catálogo e uma quantidade inteira maior ou igual a 1
- **Quando** o cliente confirma
- **Então** essa confirmação aconteceu depois de o aviso mostrar o produto, a quantidade, o preço unitário e o total
- **E** o preço unitário mostrado é o preço do catálogo naquele momento
- **E** o total mostrado é esse preço vezes a quantidade
- **E** cancelar fecha o aviso e não chama a API

Sem cartão no contexto, “Usar cartão” permanece visível e desabilitado. Isso continua o comportamento da CP-001.

### ACE-03 — Débito e um registro juntos

| Campo | Conteúdo |
| --- | --- |
| Origem | RN03; RN04 |
| Estado da origem | Proposta |

- **Dado** uma compra que se conclui
- **Quando** o saldo é debitado
- **Então** o mesmo ato grava um registro com quantidade, preço unitário aplicado, total, data e hora
- **E** não resta débito sem esse registro
- **E** não resta registro de compra concluída sem esse débito

O cliente permanece no catálogo e o saldo exibido passa a ser o saldo devolvido.

---

## Limites

### ACE-04 — Saldo exatamente igual ao total

| Campo | Conteúdo |
| --- | --- |
| Origem | CA02; RN03 |
| Estado da origem | Proposta |

- **Dado** saldo de R$ 60,00, produto a R$ 20,00 e quantidade 3
- **E** o preço confirmado é o vigente
- **Quando** o cliente confirma
- **Então** a compra fica concluída
- **E** o saldo final é zero
- **E** o registro tem quantidade 3, preço unitário R$ 20,00 e total R$ 60,00

### ACE-05 — O saldo não fica negativo

| Campo | Conteúdo |
| --- | --- |
| Origem | RN03; CA02; CA03 |
| Estado da origem | Proposta |

- **Dado** uma tentativa abrangida por ACE-01, ACE-04, ACE-06, ACE-07, ACE-09 ou ACE-10
- **Quando** a tentativa termina, concluída ou recusada
- **Então** o saldo do cartão é maior ou igual a zero

### ACE-08 — Quantidade 1 mantém o fluxo da CP-001

| Campo | Conteúdo |
| --- | --- |
| Origem | CA04; decisão P2.4 |
| Estado da origem | Proposta, com o registro novo decidido |

- **Dado** saldo maior ou igual ao preço vigente e quantidade 1
- **E** o preço confirmado é o vigente
- **Quando** o cliente confirma
- **Então** o débito é o preço do catálogo
- **E** o preço unitário aplicado é esse preço
- **E** o total é esse mesmo preço
- **E** o cartão, o botão “Usar cartão” e a lista do histórico são os da CP-001

---

## Erros

### ACE-06 — Saldo menor que o total

| Campo | Conteúdo |
| --- | --- |
| Origem | CA03; RN04 |
| Estado da origem | Proposta, com a frase decidida |

- **Dado** saldo de R$ 50,00, produto a R$ 20,00 e quantidade 3
- **E** o preço confirmado é o vigente e o total seria R$ 60,00
- **Quando** o cliente confirma
- **Então** a API responde 400
- **E** o detalhe é “Saldo insuficiente para concluir a compra.”
- **E** o saldo permanece R$ 50,00
- **E** não há registro de compra concluída

### ACE-07 — Quantidade inválida ou total que não cabe

| Campo | Conteúdo |
| --- | --- |
| Origem | CA04; RN01; decisão P2.3 |
| Estado da origem | Proposta, com as frases decididas |

- **Dado** um cartão e um produto existentes
- **Quando** a API recebe quantidade 0, −1 ou 1,5
- **Então** responde 400 com “Informe uma quantidade inteira maior que zero.”
- **E** não debita
- **E** não grava compra concluída

- **Dado** quantidade inteira maior ou igual a 1 cujo produto pelo preço vigente não cabe em `decimal`
- **Quando** a API recebe essa confirmação com o preço vigente
- **Então** responde 400 com “Não foi possível calcular o total sem estouro numérico.”
- **E** não debita
- **E** não grava compra concluída

Não há outro teto de quantidade.

### ACE-09 — Preço mudou entre a apresentação e a confirmação

| Campo | Conteúdo |
| --- | --- |
| Origem | CA05; RN02; decisão P1.3 |
| Estado da origem | Proposta, com o 409 decidido |

- **Dado** o cliente viu um preço unitário
- **E** o preço vigente do catálogo, na confirmação, é outro
- **Quando** a confirmação envia a quantidade e o preço unitário visto
- **Então** a API responde 409
- **E** o detalhe é “O preço deste produto mudou. Confirme novamente o total atualizado.”
- **E** o saldo permanece o anterior
- **E** não há débito nem compra concluída dessa confirmação
- **E** uma confirmação posterior, com o preço vigente e o total novo, pode concluir se o saldo bastar

### ACE-10 — Falha ao gravar

| Campo | Conteúdo |
| --- | --- |
| Origem | CA05; RN04; decisão P1.4 |
| Estado da origem | Proposta, com o modo de teste decidido |

- **Dado** uma compra que passaria nas demais regras
- **E** o salvar, no teste, falha antes de concluir a gravação
- **Quando** a falha ocorre
- **Então** o cliente é informado de que a compra não foi concluída
- **E** o saldo gravado permanece o anterior
- **E** não há registro de compra concluída
- **E** não há rastro de tentativa no histórico
- **E** o produto não oferece um interruptor para provocar essa falha

---

## O que a CP-001 continua exigindo

Cartão inexistente e produto inexistente continuam recusa sem débito, como na CP-001. A CP-002 não reescreve esse caso. Estoque não entra neste aceite.

## Fora destes critérios

Carrinho, desconto, frete, parcelamento, banco, autenticação e estoque não têm resultado esperado aqui.
