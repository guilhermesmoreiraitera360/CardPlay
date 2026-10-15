# CP-002 — História rica: Comprar várias unidades de um produto

## Estado deste documento

| Campo | Valor |
| --- | --- |
| Demanda | CP-002 — Comprar várias unidades de um produto |
| Artefato | História rica para revisão |
| Fontes | `01-entendimento.md`; texto da história recebido em 2026-10-14; produto depois da CP-001 |
| Data | 2026-10-15 |
| Decisões | As da seção “Decisões tomadas em 2026-10-15” de `01-entendimento.md`, autorizadas nesta conversa |
| Estado das RN/CA | **Aprovadas** em 2026-10-15 como texto oficial. O enunciado abaixo não foi reescrito. O registro está em `07-decisoes.md` |
| Critérios | CA01–CA05 permanecem os da história. Não são reescritos aqui |
| Documentos preservados | O texto original das dúvidas em `01-entendimento.md` continua. O status delas foi atualizado |

Legenda: **Fato** · **Proposta** · **Decisão** · **Conflito** · **Lacuna** · **Hipótese** · **Pergunta**.

---

## Problema e objetivo

| Elemento | Enunciado | Estado | Fonte |
| --- | --- | --- | --- |
| Problema | O cliente já compra um produto com o saldo, mas só uma unidade | **Fato** do produto + demanda da história | `docs/product.md`; texto CP-002 |
| Objetivo | Ampliar a quantidade de um único produto por compra, reaproveitando catálogo, saldo e histórico | **Proposta** | Texto CP-002 |
| Resultado | Escolher quantas unidades comprar, com total e registro claros | **Proposta** | Texto CP-002 |

**História:**  
Como cliente do CardPlay, quero escolher quantas unidades de um produto comprar com o saldo do cartão, para concluir uma compra de várias unidades com total e registro claros.

---

## Usuário e valor

| Elemento | Enunciado | Estado | Fonte |
| --- | --- | --- | --- |
| Usuário | Cliente do CardPlay | **Fato** da história. Sem perfil novo e sem autenticação | Texto CP-002 |
| Valor | Concluir a compra de várias unidades com total e registro claros | **Proposta** | Texto CP-002 |
| Quem aprova a história | Aprovada nesta conversa em 2026-10-15, sem nome de dono no texto original | **Decisão** | `07-decisoes.md` |

---

## Contexto confirmado

### O que a CP-002 reaproveita

| ID | Contexto | Estado | Fonte |
| --- | --- | --- | --- |
| C1 | Catálogo com preço por produto | **Fato** | `docs/product.md` |
| C2 | Saldo do cartão e recarga | **Fato** | `docs/product.md` |
| C3 | Histórico na mesma lista das recargas e das compras | **Fato** da CP-001. A CP-002 manda reaproveitar | `07-decisoes.md` da CP-001; texto CP-002 |
| C4 | Confirmação depois de ver o que será debitado. Cancelar não compra | **Fato** da CP-001. A CP-002 acrescenta quantidade, unitário e total a essa visão | `docs/ui.md`; RN02 |
| C5 | Dinheiro no servidor em `decimal` | **Fato** | `AGENTS.md`; decisão P2.3 |

### O que a CP-001 fez e a CP-002 muda

| ID | Contexto | Estado | Fonte |
| --- | --- | --- | --- |
| C6 | Quantidade da compra é sempre 1 | **Fato** da CP-001. **Decisão** P1.1: isso permanece para a quantidade 1 e deixa de ser o único caso | `07-decisoes.md` da CP-001; P1.1 |
| C7 | O valor gravado é o preço da unidade. Não há preço unitário separado do total | **Fato** da CP-001. **Decisão** P1.2: `Valor` passa a ser o total; o unitário ganha campo próprio | P1.2 |
| C8 | Preço mudado entre ver e confirmar não é tratado | **Fato** da CP-001. **Decisão** P1.3 fecha o comportamento | P2.3 da CP-001; P1.3 |
| C9 | Falha de gravação sem mecanismo | **Fato** da CP-001. **Decisão** P1.4: a falha é do teste, no salvar, sem rastro | Etapa 8 da CP-001; P1.4 |

---

## Escopo

### Dentro

- Quantidade inteira maior ou igual a 1 de um único produto.
- Total = preço vigente do catálogo × quantidade, calculado no servidor, sem estouro de `decimal`.
- Antes de confirmar: produto, quantidade, preço unitário e total.
- Confirmação envia a quantidade e o preço unitário visto. Preço diferente do vigente recusa sem débito.
- Saldo suficiente para o total. Um débito e um registro juntos.
- Registro com quantidade, preço unitário aplicado, total, data e hora, mais o produto já mostrado na lista.
- Quantidade 1 no mesmo botão, no mesmo cartão e na mesma lista.
- Depois da compra, o cliente permanece no catálogo.

Estado das linhas de negócio: **Proposta**. Estado do caminho: **Decisão** de 2026-10-15.

### Fora

- Carrinho de produtos diferentes
- Desconto, frete e compra parcelada
- Bancos e autenticação
- Estoque, porque a CP-004 não foi escolhida
- Teto comercial além do que `int` e `decimal` conseguem representar
- Rastro de tentativa que não seja compra concluída
- Interruptor de falha no produto

---

## Regras

| ID | Regra | Estado |
| --- | --- | --- |
| RN01 | Quantidade inteira positiva de um único produto. Sem teto comercial. Recusar o que não couber em `int` ou estourar `decimal` antes de alterar o cartão | **Aprovada**. Limite técnico **decidido** em P2.3 |
| RN02 | Total no servidor. Mostrar produto, quantidade, unitário e total antes de confirmar. Preço visto diferente do vigente rejeita aquela confirmação | **Aprovada**. Comparação **decidida** em P1.3 |
| RN03 | Concluir só com saldo maior ou igual ao total. Débito e um registro juntos. O registro guarda quantidade, preço unitário, total, data e hora | **Aprovada**. `Valor` = total **decidido** em P1.2 |
| RN04 | Quantidade inválida, saldo insuficiente ou falha de gravação não concluem e não alteram saldo nem histórico | **Aprovada**. Falha de teste **decidida** em P1.4 |

---

## Fluxo

| Passo | O que acontece | Estado |
| --- | --- | --- |
| 1 | O cartão é o que já está no contexto. Sem cartão, “Usar cartão” continua desabilitado | **Decisão** H2 / fluxo da CP-001 |
| 2 | No card do catálogo, o cliente escolhe a quantidade inteira e vê o preço unitário | **Decisão** P2.1 |
| 3 | “Usar cartão” abre o aviso com produto, quantidade, preço unitário e total | **Decisão** P2.1; **Aprovada** RN02 |
| 4 | Cancelar não chama a API | **Decisão** P2.1 |
| 5 | Confirmar envia a quantidade e o preço unitário visto | **Decisão** P1.3 |
| 6 | O servidor lê o preço vigente. Se for diferente, responde 409 e não debita. O cliente confirma de novo com o total novo | **Decisão** P1.3 e P2.2 |
| 7 | Se o preço coincidir, o total é preço × quantidade. Saldo menor que o total: 400, saldo e histórico intactos | **Aprovada** RN02 e RN04; frase **decidida** |
| 8 | Se o saldo bastar, debita o total e grava um registro. O cliente permanece no catálogo e vê o saldo novo | **Decisão** P3.1 |
| 9 | O histórico mostra nome, quantidade, preço unitário, total sem “+” e a data já usada. A recarga conserva o “+” | **Decisão** P3.2 |

---

## Limites e exceções

| Situação | Efeito | Fonte |
| --- | --- | --- |
| Saldo R$ 100, preço R$ 20, quantidade 3 | Total R$ 60, saldo R$ 40, uma compra com quantidade 3 | CA01 |
| Saldo R$ 60, preço R$ 20, quantidade 3 | Saldo zero, uma compra | CA02 |
| Saldo R$ 50, preço R$ 20, quantidade 3 | Recusa. Saldo R$ 50. Sem compra concluída | CA03 |
| Quantidade 0, −1 ou 1,5 | Recusa na API. Sem débito e sem compra | CA04 |
| Quantidade 1 | Mesmo fluxo da CP-001, com unitário igual ao preço e total igual a esse preço | CA04; P2.4 |
| Preço mudou depois da apresentação | 409. Sem débito. Nova confirmação com o total atualizado | CA05; P1.3 |
| Falha ao salvar | Compra não concluída. Sem débito e sem registro. Sem rastro | CA05; P1.4 |
| Multiplicação estoura `decimal` | 400. Sem débito | P2.3 |

Frases decididas:

- Quantidade inválida: “Informe uma quantidade inteira maior que zero.”
- Estouro: “Não foi possível calcular o total sem estouro numérico.”
- Saldo insuficiente: “Saldo insuficiente para concluir a compra.”
- Preço alterado: “O preço deste produto mudou. Confirme novamente o total atualizado.”
- Falha de gravação: a compra não foi concluída.

---

## O que a quantidade 1 conserva

Para quantidade 1, o cliente usa o mesmo cartão, o mesmo botão e a mesma lista. A confirmação passa a mostrar também a quantidade 1, o preço unitário e o total. Esses dois últimos são iguais. O débito continua sendo o preço do catálogo. Isso é a expansão do registro, não outro fluxo.

---

## Conflitos que estas decisões encerram

| Conflito | Encerramento |
| --- | --- |
| CP-001 exige quantidade 1; CP-002 pede várias | Quantidade 1 continua válida. Quantidade maior que 1 passa a ser aceita |
| `Valor` único não separa unitário e total | `Valor` é o total. O unitário é outro campo do mesmo registro |
| P2.3 da CP-001 não dizia qual preço vale | Vale o preço vigente, e só se for o preço que o cliente confirmou |
| ACE-08 não tinha como falhar | A CP-002 falha no teste, na hora de salvar, sem interruptor no produto |

---

## O que continua fora deste arquivo

- Código, contrato HTTP detalhado, migration e teste.
- CP-004 e estoque.

Próxima etapa sugerida: `03-criterios-de-aceite.md`, derivando CA01–CA05 com as decisões acima, ainda sem código.
