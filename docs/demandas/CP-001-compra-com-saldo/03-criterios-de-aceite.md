# CP-001 — Critérios de aceite: Compra com saldo do CardPlay

## Estado deste documento

| Campo | Valor |
| --- | --- |
| Demanda | CP-001 — Compra de produto com saldo CardPlay |
| Artefato | Critérios de aceite observáveis, para revisão humana |
| Fonte primária | `02-historia-rica.md` (RN01–RN04, CA01–CA05, fluxo, limites e lacunas) |
| Rastreio de decisão | `01-entendimento.md` apenas para confirmar que nenhuma regra foi aprovada (F-H9; decisões em aberto) |
| Data | 2026-10-07 |
| Decisões de negócio aprovadas | Nenhuma localizada nas fontes da pasta |
| Estado das regras de origem | **Proposta** (“Proposta para refinamento”). Este arquivo não aprova RN01–RN04 nem CA01–CA05 |
| O que estes critérios verificam | O comportamento que a proposta já escreve, em resultado observável |
| O que este arquivo não faz | Não cria regra, valor-limite, exceção, texto de mensagem, canal nem teste automatizado |

Legenda: **Proposta** (escrito na história, sem aprovação) · **Lacuna** (a história não decide; não virou critério).

Leitura dos números de CA01: o par R$ 50,00 / R$ 20,00 / R$ 30,00 é o exemplo escrito na proposta, não uma faixa de preços nem uma segunda regra.

“Um único registro correspondente” significa um registro desta compra. A proposta não exige que o histórico fique só com esse item: o cartão do exemplo já tem saldo, e o histórico de recargas já existe no produto.

---

## Cobertura

| Grupo | Critérios | Base |
| --- | --- | --- |
| Fluxo principal | ACE-01, ACE-02, ACE-03 | RN01, RN02, RN04, CA01 |
| Limites | ACE-04, ACE-05 | RN03, CA02 |
| Erros | ACE-06, ACE-07, ACE-08 | RN03, RN04, CA03, CA04, CA05 |

Cenário sem decisão suficiente permanece na seção final. Hipótese não entrou como resultado esperado.

---

## Fluxo principal

### ACE-01 — Compra concluída debita o preço do catálogo e gera um registro

| Campo | Conteúdo |
| --- | --- |
| Origem | CA01; RN01; RN02; RN04 |
| Estado da origem | Proposta |

**Cenário escrito (CA01)**

- **Dado** um cartão existente com saldo de R$ 50,00 e um produto existente cujo preço cadastrado no catálogo é R$ 20,00
- **E** o cliente identificou esse cartão e selecionou esse produto
- **E** o cliente visualizou o produto e o valor a debitar
- **Quando** o cliente confirma a compra
- **Então** a compra fica concluída
- **E** o saldo desse cartão passa a ser R$ 30,00
- **E** existe um único registro correspondente a essa compra
- **E** o débito e o registro ocorrem juntos: não resta débito sem essa compra nem essa compra concluída sem débito
- **E** o registro contém o produto, a quantidade, o valor, a data e a hora
- **E** o valor debitado e o valor do registro são o preço cadastrado, R$ 20,00
- **E** a compra contempla uma unidade desse único produto

**Mesma regra, sem outro número**

- **Dado** cartão existente, produto existente e saldo maior que o preço cadastrado desse produto
- **E** o cliente identificou o cartão, selecionou o produto e visualizou o produto e o valor a debitar
- **Quando** o cliente confirma a compra e ela se conclui
- **Então** o saldo final desse cartão é o saldo anterior menos o preço cadastrado
- **E** valem os mesmos efeitos de registro, conteúdo do registro, uma unidade e um único produto descritos no cenário de R$ 50,00

O que a proposta deixa fora deste critério está em P1.1, P1.2, P1.3 e P3.1: qual lista é “o histórico”, o que o cliente vê em cada campo, que artefato além de saldo e registro significa “concluída”, e o formato da data e da hora.

### ACE-02 — A confirmação ocorre depois de ver o produto e o valor

| Campo | Conteúdo |
| --- | --- |
| Origem | RN02; RN01 |
| Estado da origem | Proposta |

- **Dado** um cartão existente, um produto existente e saldo maior ou igual ao preço cadastrado
- **Quando** o cliente confirma a compra
- **Então** essa confirmação aconteceu depois de o cliente ter visto o produto e o valor a debitar
- **E** o valor a debitar apresentado nessa visualização é o preço cadastrado no catálogo

A proposta não fixa tela, gesto nem texto do passo de confirmação (P2.2). Este critério observa a ordem escrita em RN02 e o preço de RN01. Mudança de preço entre a visualização e a confirmação está em aberto (P2.3) e não faz parte do resultado.

### ACE-03 — Conclusão junta débito e registro

| Campo | Conteúdo |
| --- | --- |
| Origem | RN04; CA01 |
| Estado da origem | Proposta |

- **Dado** uma compra que se conclui nos termos de ACE-01 ou ACE-04
- **Quando** a conclusão é observada
- **Então** o saldo do cartão identificado foi debitado e existe o registro dessa compra
- **E** o registro contém produto, quantidade, valor, data e hora
- **E** o valor do registro é o valor debitado
- **E** não há compra concluída sem débito nem débito sem a compra correspondente

---

## Limites

### ACE-04 — Saldo exatamente igual ao preço

| Campo | Conteúdo |
| --- | --- |
| Origem | CA02; RN03; RN04 |
| Estado da origem | Proposta |

- **Dado** um cartão existente, um produto existente e saldo exatamente igual ao preço cadastrado desse produto
- **E** o cliente identificou o cartão, selecionou o produto e visualizou o produto e o valor a debitar
- **Quando** o cliente confirma a compra
- **Então** a compra fica concluída
- **E** o saldo final desse cartão é zero
- **E** débito e registro dessa compra existem juntos, com o conteúdo exigido em ACE-03

CA02 nomeia a conclusão e o saldo zero. O registro entra porque RN04 define a conclusão como débito e registro juntos. A proposta não fornece um par de valores para este limite; qualquer preço cadastrado com saldo igual a ele satisfaz o cenário.

### ACE-05 — O saldo não fica negativo

| Campo | Conteúdo |
| --- | --- |
| Origem | RN03; CA02; CA03 |
| Estado da origem | Proposta |

- **Dado** uma tentativa de compra abrangida por ACE-01, ACE-04 ou ACE-06
- **Quando** a tentativa termina, concluída ou recusada
- **Então** o saldo do cartão é maior ou igual a zero

O limite escrito de saldo zero é ACE-04. A recusa que impede atravessar zero é ACE-06. Não há outro piso numérico na proposta.

---

## Erros

### ACE-06 — Saldo menor que o preço

| Campo | Conteúdo |
| --- | --- |
| Origem | CA03; RN03 |
| Estado da origem | Proposta |

- **Dado** um cartão existente, um produto existente e saldo menor que o preço cadastrado desse produto
- **Quando** o cliente confirma a compra, no único fluxo de compra que a proposta nomeia (RN02)
- **Então** o cliente é informado de que o saldo é insuficiente
- **E** o saldo permanece o anterior
- **E** não há débito
- **E** não há registro de compra concluída

A informação exigida é a de saldo insuficiente. Texto literal, canal e lugar da mensagem continuam em aberto (P2.1) e não são resultado deste critério.

### ACE-07 — Cartão ou produto inexistente

| Campo | Conteúdo |
| --- | --- |
| Origem | CA04; RN03 |
| Estado da origem | Proposta |

Dois cenários, o mesmo resultado. A proposta usa uma informação para os dois: o dado inválido.

**Cenário A — cartão inexistente**

- **Dado** um cartão que não existe e um produto existente
- **Quando** o cliente tenta concluir a compra com esse cartão e esse produto
- **Então** vale o resultado abaixo

**Cenário B — produto inexistente**

- **Dado** um cartão existente e um produto que não existe
- **Quando** o cliente tenta concluir a compra com esse cartão e esse produto
- **Então** vale o resultado abaixo

**Resultado dos dois cenários**

- O cliente é informado de que o dado é inválido
- O saldo do cartão, quando o cartão existe, permanece o anterior
- Não há débito
- Não há registro de compra concluída

A proposta não pede mensagens diferentes para cartão e para produto. Identificação ausente no aplicativo, id local inválido e cartão não selecionado não estão decididos como “dado inválido” (P2.4) e ficam de fora.

### ACE-08 — Falha ao efetivar débito ou registro

| Campo | Conteúdo |
| --- | --- |
| Origem | CA05; RN04 |
| Estado da origem | Proposta |

**Cenário A — falha no débito**

- **Dado** uma tentativa de compra em que o débito não se efetiva
- **Quando** a falha ocorre
- **Então** vale o resultado abaixo

**Cenário B — falha no registro**

- **Dado** uma tentativa de compra em que o registro não se efetiva
- **Quando** a falha ocorre
- **Então** vale o resultado abaixo

**Resultado dos dois cenários**

- O cliente é informado de que a compra não foi concluída
- O saldo anterior é mantido
- Não há registro de compra concluída
- Não permanece débito sem compra
- Não permanece compra concluída sem débito

A proposta não descreve como provocar a falha. Também não decide se pode ficar rastro de tentativa que não seja compra concluída (P1.4). Este critério não exige nem proíbe esse rastro.

---

## Rastreabilidade

| Critério | Regra ou critério de origem | Resultado que se observa |
| --- | --- | --- |
| ACE-01 | CA01, RN01, RN02, RN04 | Saldo R$ 30,00 no exemplo; débito do preço cadastrado; uma unidade de um produto; um registro com produto, quantidade, valor, data e hora |
| ACE-02 | RN02, RN01 | Produto e valor vistos antes da confirmação; valor visto igual ao preço cadastrado |
| ACE-03 | RN04, CA01 | Débito e registro juntos na conclusão |
| ACE-04 | CA02, RN03, RN04 | Saldo final zero quando o saldo inicial é igual ao preço |
| ACE-05 | RN03, CA02, CA03 | Saldo final maior ou igual a zero |
| ACE-06 | CA03, RN03 | Informação de saldo insuficiente; saldo intacto; sem compra concluída |
| ACE-07 | CA04, RN03 | Informação de dado inválido; sem débito; sem compra concluída |
| ACE-08 | CA05, RN04 | Informação de compra não concluída; saldo anterior; sem débito órfão e sem compra concluída sem débito |

| Origem na história | Onde foi parar |
| --- | --- |
| RN01 | ACE-01, ACE-02 |
| RN02 | ACE-01, ACE-02, ACE-06 (único gatilho de compra nomeado) |
| RN03 | ACE-04, ACE-05, ACE-06, ACE-07 |
| RN04 | ACE-01, ACE-03, ACE-04, ACE-08 |
| CA01 | ACE-01, ACE-03 |
| CA02 | ACE-04, ACE-05 |
| CA03 | ACE-05, ACE-06 |
| CA04 | ACE-07 |
| CA05 | ACE-08 |

---

## Dúvidas que impedem critérios completos

Nenhum item abaixo é critério. Cada um bloqueia um resultado que a proposta cita e não especifica. Decisor nomeado no material: nenhum (`02-historia-rica.md`; `01-entendimento.md`).

| ID | O que falta decidir | Critério que fica incompleto | Origem |
| --- | --- | --- | --- |
| K3 | Aprovar RN01–RN04 e CA01–CA05, ou registrar o que muda. Enquanto isso, ACE-01–ACE-08 verificam a proposta, não um aceite oficial | Todos | Conflito K3; F-H9 |
| P1.1 / K1 | O registro da compra entra na mesma lista das recargas ou em outro histórico | ACE-01, ACE-03: existe o registro; não está decidido onde o cliente o consulta para “acompanhar o valor gasto” | RN04; CA01 |
| P1.2 | O que o cliente vê para produto, quantidade, valor, data e hora | ACE-01, ACE-03 exigem o conteúdo do registro; a apresentação ao cliente não tem critério | RN04 |
| P1.3 | O que é “compra concluída” além de saldo debitado e registro | ACE-01, ACE-04 usam o termo da proposta com esses dois efeitos; outro artefato de sucesso não tem critério | CA01; CA02; CA05 |
| P1.4 | Se uma falha pode deixar rastro de tentativa que não seja compra concluída | ACE-08 cobre saldo, informação e ausência de compra concluída; o rastro adicional não tem critério | RN04; CA05 |
| P1.5 / K2 | Se a CP-001 refina, substitui ou convive com a frase do backlog | Nenhum comportamento de compra a mais; impede tratar este arquivo como aceite do item de `docs/backlog.md` | Conflito K2 |
| P2.1 | Intenção fechada e texto das informações de saldo insuficiente, dado inválido e compra não concluída | ACE-06, ACE-07 e ACE-08 exigem informar o sentido já escrito; redação e canal não têm critério | CA03; CA04; CA05 |
| P2.2 | Se a confirmação é um resumo dedicado ou um gesto depois de ver o preço | ACE-02 exige ver produto e valor antes de confirmar; a forma do passo não tem critério | RN02 |
| P2.3 | Qual preço vale se o cadastrado mudar entre visualização e confirmação | ACE-02 vale para o preço cadastrado mostrado; o caso de mudança não tem critério | RN01 |
| P2.4 | Tentativa sem cartão selecionado, ou com identificação inválida, conta como dado inválido | ACE-07 cobre cartão ou produto inexistente; os outros modos de identificação não têm critério | RN02; RN03; CA04 |
| P2.5 | Como o valor gasto aparece (sinal, rótulo, distinção da recarga) | Sem critério de exibição do valor gasto | K1 |
| P3.1 | Formato de data e hora apresentado ao cliente | ACE-01 e ACE-03 exigem data e hora no registro; o formato não tem critério | RN04 |
| P3.2 / K5 | Como a compra passa a ser oferecida, inclusive o botão hoje desabilitado | Sem critério de ponto de entrada | RN02; conflito K5 |
| P3.3 | Para onde o cliente vai depois da compra | Sem critério de navegação posterior. A história rica indica que isso não bloqueia saldo nem registro | Fluxo, passo 5 |
| H4 | Se a quantidade gravada no registro é sempre 1 | ACE-01 exige compra de uma unidade e presença da quantidade no registro; o número gravado nesse campo não tem critério | RN01; RN04 |
| K4 | Se autenticação, estoque, administração de catálogo, checkout e integração bancária estão fora desta história | Sem critério de exclusão. A versão atual do produto os exclui; a CP-001 não os lista | Conflito K4 |

### Escrito na proposta e ainda assim sem critério

Carrinho, múltiplas unidades, cancelamento e estorno estão fora da proposta. A história não descreve o que acontece se alguém tenta esses caminhos. Por isso não há critério de recusa, mensagem ou saldo para eles.

### Hipóteses que não viraram resultado

| ID | Hipótese deixada de fora | Motivo |
| --- | --- | --- |
| H1 | Histórico da compra na mesma lista das recargas | Conflito K1, pergunta P1.1 |
| H2 | Preço lido no momento da confirmação | RN01 não fixa o momento; P2.3 |
| H3 | “Identificar o cartão” = cartão já selecionado no aplicativo | RN02 não descreve o mecanismo; P2.4 |
| H4 | Quantidade gravada sempre 1 | Combinação sugerida, não explícita |
| H5 | Sem entrega, estoque ou pedido além de débito e registro | Não está no PDF |
| H6 | Mensagens de CA03–CA05 só no aplicativo | A proposta diz “informar” e não fixa o canal |
