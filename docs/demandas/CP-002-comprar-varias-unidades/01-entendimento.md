# CP-002 — Entendimento: Comprar várias unidades de um produto

## Metadados

| Campo | Valor |
| --- | --- |
| Demanda | CP-002 — Comprar várias unidades de um produto |
| Tipo | História para implementação no laboratório |
| Fonte principal | Texto da história colado na conversa de 2026-10-14. Não há PDF desta demanda na pasta |
| Fontes de contraste | CP-001 já entregue: `docs/product.md`, `docs/ui.md`, `AGENTS.md`, `docs/demandas/CP-001-compra-com-saldo/07-decisoes.md` |
| Status do entendimento | Dúvidas P1 e P2 fechadas em 2026-10-15, nesta conversa. RN e CA continuam o texto da história |
| Data | 2026-10-15 |
| Documento recebido | Preservado nesta conversa; não há arquivo-fonte separado para alterar |
| Próxima etapa sugerida | `02-historia-rica.md`, depois que as dúvidas P1 forem lidas. Sem código |

---

## Problema e resultado desejado

| Elemento | Conteúdo | Classificação | Fonte |
| --- | --- | --- | --- |
| Usuário | Cliente do CardPlay | **Fato** (história) | Texto CP-002 |
| Valor | Concluir uma compra de várias unidades com total e registro claros | **Fato** (história) | Texto CP-002 |
| Problema | A compra com saldo existe, mas a quantidade gravada e confirmada é uma unidade | **Fato** (produto) + demanda na história | `docs/product.md`; `07-decisoes.md` da CP-001; texto CP-002 |
| Resultado desejado | Escolher quantas unidades de um único produto comprar com o saldo, reaproveitando catálogo, saldo e histórico | **Fato** (história) | Texto CP-002 |

**História:**  
Como cliente do CardPlay, quero escolher quantas unidades de um produto comprar com o saldo do cartão, para concluir uma compra de várias unidades com total e registro claros.

**Descrição:** ampliar a quantidade de um único produto por compra, reaproveitando catálogo, saldo e histórico. Não criar carrinho de produtos diferentes.

**Prioridade didática escrita na história:** expansão direta da RN01 da compra já existente. Permite praticar mudança controlada em contrato, regra, interface e histórico. A frase seguinte diz que isso não substitui a correção da quantidade obrigatória da CP-001. O sentido de “correção” não está definido no texto.

---

## Fatos conhecidos (com fonte)

### Da história CP-002

| ID | Fato | Fonte |
| --- | --- | --- |
| F-H1 | A compra passa a aceitar quantidade inteira positiva de um único produto | RN01 |
| F-H2 | Neste laboratório não há teto comercial adicional. A entrada deve respeitar limites técnicos de representação e não causar estouro numérico | RN01 |
| F-H3 | O total é calculado no servidor: preço vigente do catálogo multiplicado pela quantidade | RN02 |
| F-H4 | Antes da confirmação o cliente vê produto, quantidade, preço unitário e total | RN02 |
| F-H5 | Se o preço mudar entre a apresentação e a confirmação, aquela confirmação é rejeitada e outra só vale com valores atualizados | RN02 e CA05 |
| F-H6 | A compra só conclui com saldo suficiente para o total. Débito e um registro nascem juntos. O registro contém quantidade, preço unitário aplicado, total, data e hora | RN03 |
| F-H7 | Quantidade inválida, saldo insuficiente ou falha de gravação não concluem a compra e não alteram saldo nem histórico | RN04 |
| F-H8 | Estoque só condiciona a compra se a CP-004 também tiver sido escolhida | RN04 |
| F-H9 | CA01: saldo R$ 100, produto R$ 20, quantidade 3, total R$ 60, saldo final R$ 40, exatamente uma compra com quantidade 3 | CA01 |
| F-H10 | CA02: saldo R$ 60, produto R$ 20, quantidade 3, saldo final zero | CA02 |
| F-H11 | CA03: saldo R$ 50, produto R$ 20, quantidade 3, recusa, saldo permanece R$ 50, sem compra concluída | CA03 |
| F-H12 | CA04: quantidades 0, −1 e 1,5 são recusadas na API, sem débito e sem compra concluída. Quantidade 1 mantém o fluxo da CP-001 | CA04 |
| F-H13 | CA05 pede simular falha de persistência e mudança de preço. Na falha, nenhum débito nem registro concluído. Na mudança de preço, nova confirmação com total atualizado, sem débito da confirmação anterior | CA05 |
| F-H14 | Fora do escopo: carrinho, desconto, frete, compra parcelada, bancos e autenticação | Texto CP-002 |
| F-H15 | Dependência: CP-001 funcional. Correções necessárias devem ser identificadas antes da expansão | Texto CP-002 |
| F-H16 | A representação monetária e os limites técnicos devem ser documentados conforme os padrões existentes, preservando a nova confirmação quando o preço mudar | Definições de implementação |

### Do produto depois da CP-001 (contraste; não é regra da CP-002)

| ID | Fato | Fonte |
| --- | --- | --- |
| F-P1 | Já existe compra de um produto com saldo, confirmação com nome e preço, e histórico de recargas e compras | `docs/product.md` |
| F-P2 | A quantidade da compra documentada e decidida é sempre 1 | `docs/product.md`; `AGENTS.md`; `07-decisoes.md` da CP-001, H4 |
| F-P3 | O registro da compra fica na mesma lista das recargas, com produto, quantidade, valor e data. A tela mostra o nome do produto, a quantidade e o valor sem “+” | `07-decisoes.md` da CP-001, P1.1, P1.2, P2.5 |
| F-P4 | O valor gravado na compra da CP-001 é o preço do catálogo daquela unidade. Não há campo separado de preço unitário aplicado | `07-decisoes.md` da CP-001; `MovimentacaoCartao` |
| F-P5 | A confirmação atual mostra nome e preço. Não mostra quantidade escolhida nem total de várias unidades | `docs/product.md`; `docs/ui.md` |
| F-P6 | A mudança de preço entre ver e confirmar ficou em aberto na CP-001 (P2.3) e não foi implementada | `07-decisoes.md` da CP-001 |
| F-P7 | A falha de gravação (ACE-08 da CP-001) ficou sem modo de provocação e sem evidência | `07-decisoes.md` da CP-001, etapa 8 |
| F-P8 | Sem autenticação, estoque, carrinho, checkout ou banco | `docs/product.md`; `AGENTS.md` |
| F-P9 | Dinheiro no servidor é `decimal`. O aplicativo já usava `number` para saldo, valor e preço | `AGENTS.md`; `08-validacao.md` da CP-001 |
| F-P10 | A história da CP-001 colocava múltiplas unidades fora do escopo | `01-entendimento.md` da CP-001, F-H8 |

### Contraste explícito

| O que a CP-002 diz | O que a CP-001 entregou |
| --- | --- |
| Quantidade inteira positiva, inclusive maior que 1 | Quantidade sempre 1 |
| Mostrar quantidade, preço unitário e total antes de confirmar | Confirmação com nome e preço da unidade |
| Registro com quantidade, preço unitário aplicado, total, data e hora | Registro com produto, quantidade 1, valor debitado e data. Sem preço unitário separado |
| Rejeitar a confirmação se o preço mudar e pedir outra | P2.3 em aberto, sem esse comportamento |
| Simular falha de persistência sem débito nem registro | ACE-08 sem mecanismo |
| Quantidade 1 mantém o fluxo da CP-001 | Esse fluxo é o que existe hoje |

---

## Escopo (in / out) — só o escrito na história

### In

- Várias unidades de um único produto na mesma compra.
- Quantidade inteira positiva. Quantidade 1 continua o fluxo da CP-001.
- Total calculado no servidor pelo preço vigente do catálogo vezes a quantidade.
- Antes de confirmar: produto, quantidade, preço unitário e total.
- Recusa da confirmação quando o preço mudou, com nova confirmação nos valores atualizados, sem débito da confirmação anterior.
- Conclusão só com saldo suficiente para o total.
- Um débito e um registro, juntos, com quantidade, preço unitário aplicado, total, data e hora.
- Recusa de quantidade inválida (0, −1 e 1,5 estão nos exemplos), de saldo insuficiente para o total e de falha de gravação, sem alterar saldo nem histórico.
- Reuso de catálogo, saldo e histórico. Sem carrinho de produtos diferentes.

### Out

- Carrinho
- Desconto
- Frete
- Compra parcelada
- Bancos
- Autenticação
- Estoque, enquanto a CP-004 não tiver sido escolhida. Esta demanda não a escolhe

> A história manda identificar correções necessárias da CP-001 antes da expansão. Não lista quais são. A frase “não substitui a correção da quantidade obrigatória de CP001” também não descreve a correção.

---

## Regras e critérios escritos na história

Classificação: **fato do texto recebido**. Não é aceite aprovado para implementação.

### Regras

| ID | Regra |
| --- | --- |
| RN01 | Quantidade inteira positiva de um único produto. Sem teto comercial adicional. Respeitar limites técnicos e não estourar na conta |
| RN02 | Total no servidor: preço vigente × quantidade. Mostrar produto, quantidade, preço unitário e total antes de confirmar. Preço mudado rejeita aquela confirmação |
| RN03 | Concluir só com saldo suficiente para o total. Persistir débito e um registro com quantidade, preço unitário aplicado, total, data e hora |
| RN04 | Quantidade inválida, saldo insuficiente ou falha de gravação não concluem e não alteram saldo nem histórico. Estoque só entra com a CP-004 |

### Critérios

| ID | Critério |
| --- | --- |
| CA01 | Saldo R$ 100, produto R$ 20, quantidade 3. Total R$ 60. Saldo final R$ 40. Exatamente uma compra com quantidade 3 |
| CA02 | Saldo R$ 60, produto R$ 20, quantidade 3. Saldo final zero |
| CA03 | Saldo R$ 50, produto R$ 20, quantidade 3. Recusa. Saldo permanece R$ 50. Sem compra concluída |
| CA04 | Quantidades 0, −1 e 1,5 recusadas na API, sem débito e sem compra concluída. Quantidade 1 mantém o fluxo da CP-001 |
| CA05 | Falha de persistência: nenhum débito nem registro concluído. Mudança de preço: nova confirmação com total atualizado, sem débito da confirmação anterior |

---

## Hipóteses (explícitas, não aprovadas)

| ID | Hipótese | Por que é hipótese |
| --- | --- | --- |
| H1 | O histórico continua a mesma lista da CP-001 | A história manda reaproveitar o histórico e não descreve uma segunda lista |
| H2 | O cartão continua o que já está no contexto do aplicativo | A CP-001 fechou assim. A CP-002 não reabre a identificação |
| H3 | O ponto de entrada continua “Usar cartão”, agora com a quantidade | A história não nomeia outro botão |
| H4 | O valor debitado é o total, e o preço unitário aplicado precisa de campo próprio | RN03 pede os dois. Hoje `Valor` é o preço da unidade |
| H5 | A confirmação envia ao servidor a quantidade e o preço que o cliente viu, para a rejeição de RN02 | A história exige detectar a mudança. Não diz qual dado viaja na confirmação |
| H6 | Quantidade inválida e saldo insuficiente reutilizam a recusa HTTP já usada na compra, com textos próprios | CA04 exige recusa na API. Não fixa status nem frase |

Nenhuma hipótese acima é decisão aprovada.

---

## Dúvidas abertas priorizadas

### P1 — Bloqueantes

| ID | Pergunta | Por que importa | Quem decide |
| --- | --- | --- | --- |
| P1.1 | O que é “a correção da quantidade obrigatória de CP001” que esta demanda não substitui? | A CP-001 decidiu quantidade sempre 1. A CP-002 passa a aceitar outras quantidades e, ao mesmo tempo, diz que não substitui essa correção | Responsável pelo negócio, não nomeado no texto |
| P1.2 | Onde o preço unitário aplicado fica no registro, se `Valor` hoje é o valor debitado? | RN03 pede quantidade, preço unitário aplicado e total no mesmo registro. Sem isso o CA01 não distingue unitário de total | Responsável pelo negócio, não nomeado no texto |
| P1.3 | Qual dado da confirmação permite saber que o preço mudou? | RN02 e CA05 rejeitam a confirmação antiga. O texto não diz o que se compara com o preço vigente | Responsável pelo negócio, não nomeado no texto |
| P1.4 | Como a falha de persistência da CA05 é provocada? | A CP-001 deixou a falha de gravação sem mecanismo. A CP-002 exige simulá-la e não descreve o meio | Responsável pelo negócio, não nomeado no texto |

### P2 — Importantes

| ID | Pergunta | Por que importa | Quem decide |
| --- | --- | --- | --- |
| P2.1 | Onde a quantidade é escolhida: no card do catálogo, no aviso de confirmação, ou nos dois? | RN02 exige ver quantidade, unitário e total antes de confirmar. Não escolhe o controle | Responsável pelo negócio, não nomeado no texto |
| P2.2 | Quais frases a API e a tela usam para quantidade inválida, saldo insuficiente para o total, preço alterado e falha de gravação? | RN04 e CA03–CA05 exigem recusa e informação. Não fixam o texto | Responsável pelo negócio, não nomeado no texto |
| P2.3 | Qual é o limite técnico da quantidade e do total, além de “não estourar”? | RN01 e as definições de implementação mandam documentar o limite conforme o padrão existente. Não dão o número | Responsável pelo negócio, com o padrão `decimal` já usado no servidor |
| P2.4 | “Quantidade 1 mantém o fluxo da CP-001” inclui a mesma confirmação, o mesmo corpo HTTP e o mesmo histórico, só acrescentando o preço unitário? | CA04 usa o fluxo antigo como resultado. O registro novo pede preço unitário, que a CP-001 não mostra | Responsável pelo negócio, não nomeado no texto |

### P3 — Refinamentos

| ID | Pergunta | Por que importa | Quem decide |
| --- | --- | --- | --- |
| P3.1 | Depois da compra de várias unidades, o cliente permanece no catálogo, como na CP-001? | A história não muda o destino. Não bloqueia total nem registro | Responsável pelo negócio, não nomeado no texto |
| P3.2 | O histórico mostra o preço unitário além da quantidade e do total? | RN03 põe o unitário no registro. P2.5 da CP-001 não o mencionava | Responsável pelo negócio, não nomeado no texto |

---

## Decisões tomadas em 2026-10-15

Autorizadas nesta conversa para desbloquear a história rica. Não alteram o texto da história. Não são código.

| ID | Decisão |
| --- | --- |
| P1.1 | A quantidade sempre 1 da CP-001 não é defeito a corrigir. A CP-002 a conserva quando a quantidade escolhida é 1 e acrescenta quantidades inteiras maiores. Não há correção prévia separada |
| P1.2 | `Valor` continua sendo o total debitado. O preço unitário aplicado entra num campo próprio do mesmo registro. Quantidade, nome do produto, data e hora permanecem |
| P1.3 | A confirmação envia a quantidade e o preço unitário que o cliente viu. O servidor compara esse preço com o preço vigente. Se forem diferentes, recusa sem debitar. O total só é o preço vigente vezes a quantidade quando os preços coincidem |
| P1.4 | A falha de gravação é provocada no teste, no momento de salvar, e não por um interruptor no produto. Não grava débito nem compra. Não fica rastro de tentativa. O cliente é informado de que a compra não foi concluída |
| P2.1 | A quantidade é escolhida no card do catálogo. O aviso de confirmação mostra produto, quantidade, preço unitário e total. Cancelar não chama a API. O botão continua “Usar cartão” |
| P2.2 | Quantidade inválida e estouro: HTTP 400, “Informe uma quantidade inteira maior que zero.” ou “Não foi possível calcular o total sem estouro numérico.” Saldo insuficiente: o mesmo 400 e a frase já usada na CP-001. Preço alterado: HTTP 409, “O preço deste produto mudou. Confirme novamente o total atualizado.” Falha de gravação: informar que a compra não foi concluída, sem compra no histórico |
| P2.3 | Não há teto comercial. A quantidade é inteira maior ou igual a 1 e cabe em `int`. O total é `decimal`. Se a multiplicação estourar, a compra é recusada antes de alterar o cartão |
| P2.4 | Quantidade 1 usa o mesmo cartão, o mesmo botão e a mesma lista. O corpo passa a levar a quantidade. O registro também guarda o preço unitário, igual ao preço do catálogo, e o total, igual a esse preço |
| P3.1 | Depois da compra o cliente permanece no catálogo |
| P3.2 | O histórico da compra mostra nome, quantidade, preço unitário, total sem “+” e a data no formato já usado. A recarga não muda |

## Decisões necessárias

| Decisão | Por que | Quem decide | Status |
| --- | --- | --- | --- |
| Aprovar RN01–RN04 e CA01–CA05 como aceite, ou listar o que muda | O texto ainda é história de laboratório, não aceite fechado neste arquivo | Responsável pelo negócio, não nomeado no texto | **Fechada** em 2026-10-15. Aprovadas como texto oficial, registradas em `07-decisoes.md` |
| Explicar a frase sobre a quantidade obrigatória da CP-001 | Desbloqueia o que pode mudar no fluxo de quantidade 1 | Esta conversa | **Fechada** em P1.1 |
| Definir o registro com preço unitário e total | Desbloqueia CA01 e a persistência | Esta conversa | **Fechada** em P1.2 |
| Definir como a confirmação detecta preço alterado | Desbloqueia RN02 e a parte de preço da CA05 | Esta conversa | **Fechada** em P1.3 |
| Definir como simular a falha de gravação | Desbloqueia a parte de persistência da CA05 | Esta conversa | **Fechada** em P1.4 |
| Documentar representação monetária e limite técnico sem estouro | Pedido explícito das definições de implementação | Esta conversa | **Fechada** em P2.3 |

---

## O que não foi decidido / fora desta etapa

- **Não decidido:** aprovação da história; textos de recusa; controle de quantidade; campo do preço unitário; comparação de preço; modo da falha de gravação; limite numérico concreto.
- **Fora desta etapa:** código, API, migration, tela, teste e alteração dos documentos de produto.
- **Não inventado aqui:** carrinho, desconto, frete, parcelamento, banco, autenticação, estoque da CP-004, teto comercial.
- **Perguntas não são decisões.** Permanecem abertas até registro explícito.

*Classificação: **Fato** (afirmado na fonte citada) · **Hipótese** (possível, não aprovada) · **Pergunta** (aberta) · **Decisão necessária** (bloqueia avanço sem dono explícito).*
