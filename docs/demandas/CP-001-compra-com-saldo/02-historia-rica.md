# CP-001 — História rica: Compra com saldo do CardPlay

## Estado deste documento

| Campo | Valor |
| --- | --- |
| Demanda | CP-001 — Compra de produto com saldo CardPlay |
| Artefato | História rica para revisão de negócio |
| Fontes | `01-entendimento.md`; PDF `CardPlay_Historia_de_Usuario_Visual.pdf`; contraste em `docs/product.md`, `docs/backlog.md`, `README.md`, `AGENTS.md`, `docs/ui.md` |
| Data | 2026-10-06 |
| Decisões de negócio aprovadas | **Nenhuma localizada** nas fontes desta pasta |
| Estado das regras RN/CA | **Proposta** escrita no PDF (“Proposta para refinamento”). Fato do texto; **não** aceite oficial nem decisão aprovada (`01-entendimento.md`, F-H9 e seção de decisões) |
| Critérios de aceite | Os CA01–CA05 existem na história recebida. Esta etapa **não os deriva nem os aprova**; ficam preservados para derivação posterior |
| Documentos preservados | `01-entendimento.md` e o PDF original não foram alterados |

Legenda de estado: **Fato** (afirmado na fonte) · **Proposta** (escrito na história, sem aprovação) · **Conflito** (fontes divergem) · **Lacuna** (ausente) · **Hipótese** (não aprovada) · **Pergunta** (aberta) · **Decisão necessária** (pendente).

---

## Problema e objetivo

| Elemento | Enunciado | Estado | Fonte |
| --- | --- | --- | --- |
| Problema | O cliente recarrega o cartão, consulta saldo e histórico e vê o catálogo, e **não consegue comprar** um produto com esse saldo | **Fato** do produto atual + demanda da proposta | PDF (contexto); `docs/product.md`; README; `docs/backlog.md`; `01-entendimento.md` |
| Objetivo da demanda | Permitir a compra de um produto com os créditos disponíveis, mantendo o saldo e o registro da operação consistentes | **Proposta** | PDF CP-001 |
| Resultado desejado na história | Comprar um produto do catálogo usando o saldo do cartão, para utilizar os créditos e acompanhar o valor gasto | **Proposta** | PDF CP-001 |

**História (texto do PDF, preservado):**  
Como cliente do CardPlay, quero comprar um produto do catálogo usando o saldo do meu cartão, para utilizar meus créditos e acompanhar o valor gasto.

---

## Usuário e valor

| Elemento | Enunciado | Estado | Fonte |
| --- | --- | --- | --- |
| Usuário | Cliente do CardPlay | **Fato** da história (papel nomeado); sem persona, autenticação ou perfil adicional | PDF CP-001 |
| Valor | Utilizar os créditos e acompanhar o valor gasto | **Proposta** | PDF CP-001 |
| Quem aprova a história | Não identificado no material | **Lacuna** | `01-entendimento.md` (decisões: responsável pelo negócio / product owner não documentado) |

---

## Contexto confirmado

### O que a proposta assume como já existente

| ID | Contexto | Estado | Fonte |
| --- | --- | --- | --- |
| C1 | Recarregar o cartão | **Fato** (proposta e produto) | PDF; `docs/product.md`; README |
| C2 | Consultar saldo | **Fato** (proposta e produto) | PDF; `docs/product.md` |
| C3 | Consultar histórico | **Fato** de que a consulta existe. **Conflito** sobre o conteúdo: a proposta fala em histórico da operação de compra; o produto documenta histórico de **recargas** | PDF; `docs/product.md`; `01-entendimento.md` (F-P8, contraste) |
| C4 | Visualizar o catálogo | **Fato** (proposta e produto; catálogo por seed) | PDF; `docs/product.md`; README |

### O que o produto atual documenta e a proposta não redefine

| ID | Contexto | Estado | Fonte |
| --- | --- | --- | --- |
| C5 | Cartão virtual interno, sem banco, gateway, PAN ou CVV | **Fato** do produto atual. A proposta não altera nem menciona isso | `docs/product.md`; README |
| C6 | Saldo inicial zero; recarga só com valor maior que zero | **Fato** do produto atual. Fora do texto da CP-001 | `docs/product.md`; README |
| C7 | Vários cartões podem existir na API; o aplicativo guarda localmente o cartão selecionado; não há login | **Fato** do produto atual. A proposta diz “identificar o cartão” e não descreve esse mecanismo | `docs/product.md` |
| C8 | Botão “Usar cartão” / “Comprar” visível, desabilitado, marcado como “Em breve” | **Fato** do produto atual. A proposta não diz o que acontece com esse botão | `docs/product.md`; `docs/ui.md`; AGENTS.md |
| C9 | Compra ainda não implementada. Backlog: “Permitir que o usuário utilize o saldo do cartão para adquirir produtos disponíveis na plataforma”, sem regras | **Fato**. **Conflito** com o detalhe da CP-001 (ver conflitos) | `docs/backlog.md`; `docs/product.md`; AGENTS.md |
| C10 | A versão atual também não faz autenticação, administração de catálogo, estoque, carrinho nem checkout | **Fato** do produto atual. **Não** está na lista “fora do escopo” da CP-001 | `docs/product.md`; AGENTS.md; `01-entendimento.md` |

---

## Escopo

### Dentro da proposta (escrito no PDF)

Estado de cada item: **Proposta**, não decisão aprovada.

- Compra de uma unidade de um único produto.
- Preço usado: o cadastrado no catálogo.
- O cliente identifica o cartão, seleciona o produto e confirma depois de visualizar o produto e o valor a debitar.
- Exigência de cartão existente, produto existente e saldo maior ou igual ao valor; saldo não negativo.
- Conclusão com débito e registro no histórico juntos.
- Registro com produto, quantidade, valor, data e hora.

Fonte: PDF CP-001 (RN01–RN04); consolidado em `01-entendimento.md`.

### Fora da proposta (escrito no PDF)

Estado: **Proposta** de exclusão, não decisão aprovada de produto.

- Carrinho
- Múltiplas unidades
- Cancelamento
- Estorno

Fonte: PDF CP-001; `01-entendimento.md` (F-H8).

### Limite de leitura

Autenticação, estoque, administração de catálogo, checkout e integração bancária estão fora da **versão atual** do produto. A CP-001 **não** os lista como fora do escopo desta história. Tratar essa lista como exclusão da CP-001 seria decidir o que a fonte não decide.

---

## Regras escritas na proposta

Estado comum: **Proposta**. Nenhuma linha abaixo foi localizada como decisão de negócio aprovada.

| ID | Regra | Fonte |
| --- | --- | --- |
| RN01 | Cada compra contempla uma unidade de um único produto, pelo preço cadastrado no catálogo | PDF CP-001 |
| RN02 | O cliente identifica o cartão, seleciona o produto e confirma a compra após visualizar o produto e o valor a debitar | PDF CP-001 |
| RN03 | A compra exige cartão e produto existentes e saldo maior ou igual ao valor da compra. O saldo não pode ficar negativo | PDF CP-001 |
| RN04 | A conclusão exige débito e registro no histórico juntos. O registro deve conter produto, quantidade, valor, data e hora. Uma falha não pode deixar débito sem compra nem compra concluída sem débito | PDF CP-001 |

---

## Fluxo sustentado pelas fontes

Somente os passos nomeados em RN02, na ordem escrita. O modo de cada passo (tela, gesto, texto) **não** está na fonte.

| Passo | O que a proposta exige | Estado | O que permanece aberto |
| --- | --- | --- | --- |
| 1 | Identificar o cartão | **Proposta** (RN02) | Como se identifica; relação com o cartão já selecionado no app → pergunta P2.4, hipótese H3 |
| 2 | Selecionar o produto | **Proposta** (RN02) | Ponto de entrada na interface → pergunta P3.2 |
| 3 | Visualizar o produto e o valor a debitar | **Proposta** (RN02) | Se a visualização é um passo dedicado → pergunta P2.2 |
| 4 | Confirmar a compra | **Proposta** (RN02) | Gesto mínimo de confirmação → pergunta P2.2 |
| 5 | Se a compra se conclui: débito e um registro no histórico, juntos | **Proposta** (RN04, CA01, CA02) | O que o cliente vê como “concluída”; onde fica o histórico → P1.1, P1.2, P1.3 |

Não há, nas fontes, fluxo posterior à compra (permanecer no catálogo, abrir histórico ou voltar ao cartão). Isso está em aberto (P3.3).

---

## Limites e exceções sustentados pelas fontes

Estado: **Proposta** (PDF). Efeito de negócio escrito na história; canal e texto da informação continuam em aberto (P2.1).

| Situação | Efeito escrito na proposta | Fonte |
| --- | --- | --- |
| Saldo exatamente igual ao preço | Compra concluída; saldo final zero | CA02 |
| Saldo menor que o preço | Informar saldo insuficiente; sem débito; sem registro de compra concluída | CA03; coerente com RN03 |
| Cartão ou produto inexistente | Informar o dado inválido; sem débito; sem registro de compra concluída | CA04; coerente com RN03 |
| Falha ao efetivar o débito ou o registro | Informar que a compra não foi concluída; saldo anterior mantido; sem registro de compra concluída | CA05; RN04 |
| Saldo resultante negativo | Não permitido | RN03 |
| Débito sem compra, ou compra concluída sem débito | Não permitido | RN04 |

Exemplo numérico escrito na proposta (não é regra extra): cartão com R$ 50,00 e produto de R$ 20,00, após confirmar → compra concluída, saldo R$ 30,00, um único registro correspondente no histórico (CA01).

O que a proposta **não** define nessas exceções: texto da mensagem, onde a mensagem aparece, e se uma falha pode deixar rastro de tentativa que não seja “compra concluída” (P1.4, P2.1).

---

## Conflitos para revisão

Estes pontos impedem ler a proposta como especificação fechada. Não foram resolvidos neste documento.

| ID | Conflito | Fonte A | Fonte B | Impacto na revisão |
| --- | --- | --- | --- | --- |
| K1 | Conteúdo do histórico | PDF RN04/CA01: registro da compra com produto, quantidade, valor, data e hora | Produto atual: histórico de recargas com descrição, valor e data/hora (`docs/product.md`; `01-entendimento.md` F-P8, F-P9) | “Acompanhar o valor gasto” e o aceite de CA01 dependem de decidir o que é “histórico” |
| K2 | Maturidade da demanda | PDF: RN01–RN04 e CA01–CA05 | `docs/backlog.md`: uma frase, sem regras nem critérios | Não está decidido se a CP-001 refina, substitui ou convive com o item do backlog (P1.5) |
| K3 | Status das regras | PDF: regras e critérios escritos | PDF e `01-entendimento.md`: “proposta para refinamento”; decisões de negócio **abertas** | Regra escrita ≠ regra aprovada |
| K4 | Fora de escopo | PDF exclui só carrinho, múltiplas unidades, cancelamento e estorno | Produto atual também exclui autenticação, estoque, checkout e banco | A interseção não foi decidida como escopo da CP-001 |
| K5 | Compra na interface | PDF: o cliente seleciona e confirma a compra | UI atual: botão “Usar cartão” visível e desabilitado (“Em breve”) | A proposta não diz como a compra passa a ser oferecida |

---

## Critérios já escritos na história recebida

Preservados para derivação posterior. Estado: **Proposta**. Esta etapa não os reescreve, não os aprova e não acrescenta cenários.

| ID | Critério na fonte | Fonte |
| --- | --- | --- |
| CA01 | Cartão com R$ 50,00 e produto de R$ 20,00; ao confirmar, compra concluída, saldo R$ 30,00 e um único registro correspondente no histórico | PDF CP-001 |
| CA02 | Saldo exatamente igual ao preço; ao confirmar, compra concluída e saldo final zero | PDF CP-001 |
| CA03 | Saldo menor que o preço; informar saldo insuficiente; sem debitar nem registrar compra concluída | PDF CP-001 |
| CA04 | Cartão ou produto inexistente; informar o dado inválido; sem debitar nem registrar compra concluída | PDF CP-001 |
| CA05 | Falha ao efetivar débito ou registro; informar que a compra não foi concluída; saldo anterior mantido; sem registro de compra concluída | PDF CP-001 |

Derivar critérios de aceite oficiais depende de fechar, no mínimo, os conflitos K1 e K3 e as perguntas P1.

---

## Hipóteses, perguntas e decisões pendentes

Nada nesta seção é regra. Origem indicada em cada item. Decisor, quando citado: responsável pelo negócio / product owner — **não nomeado** no material (`01-entendimento.md`).

### Hipóteses (não aprovadas)

| ID | Hipótese | Origem |
| --- | --- | --- |
| H1 | O histórico da compra seria a mesma lista das recargas | `01-entendimento.md`; nasce do conflito K1 |
| H2 | O preço seria o vigente no momento da confirmação | `01-entendimento.md`; RN01 não fixa o momento da leitura |
| H3 | “Identificar o cartão” seria o cartão já selecionado no aplicativo | `01-entendimento.md`; produto guarda cartão localmente; RN02 não descreve a UI |
| H4 | A quantidade no registro seria sempre 1 | `01-entendimento.md`; combinação de RN01 e RN04, não explícita |
| H5 | Não haveria entrega, estoque nem pedido além de débito e registro | `01-entendimento.md`; **não** está no PDF |
| H6 | As mensagens de CA03–CA05 apareceriam no fluxo do cliente no aplicativo | `01-entendimento.md`; a história diz “informar” e não fixa o canal |

### Perguntas abertas

| ID | Prioridade no entendimento | Pergunta | Por que importa | Origem |
| --- | --- | --- | --- | --- |
| P1.1 | P1 | O registro de compra entra na mesma lista das recargas ou em outro histórico? | CA01 e o valor “acompanhar o valor gasto” | `01-entendimento.md`; conflito K1 |
| P1.2 | P1 | O que o registro mostra ao cliente para cumprir produto, quantidade, valor, data e hora? | O histórico atual não exibe produto nem quantidade | `01-entendimento.md`; F-P9; RN04 |
| P1.3 | P1 | O que é “compra concluída” para o cliente, além de saldo debitado e registro? | CA01, CA02 e CA05 usam o termo sem artefato mínimo | `01-entendimento.md`; PDF |
| P1.4 | P1 | Em falha, pode existir rastro de tentativa que não seja compra concluída? | RN04 e CA05 vetam compra concluída sem débito e débito sem compra; não falam de tentativa | `01-entendimento.md`; PDF |
| P1.5 | P1 | A CP-001 refina, substitui ou é outra demanda em relação ao item do backlog? | Status da demanda no ciclo do repositório | `01-entendimento.md`; conflito K2; AGENTS.md |
| P2.1 | P2 | Qual a intenção das mensagens de saldo insuficiente, dado inválido e compra não concluída? | CA03–CA05 exigem informar | `01-entendimento.md`; PDF |
| P2.2 | P2 | A confirmação é um passo de resumo ou um gesto depois de ver o preço? | Fluxo mínimo de RN02 | `01-entendimento.md`; RN02 |
| P2.3 | P2 | Se o preço mudar entre visualização e confirmação, qual preço vale? | RN01 cita preço cadastrado e não cobre mudança | `01-entendimento.md`; RN01 |
| P2.4 | P2 | Tentativa sem cartão selecionado ou com identificação inválida conta como dado inválido (CA04)? | Liga RN02/RN03 ao produto atual (C7) | `01-entendimento.md` |
| P2.5 | P2 | Como o valor gasto aparece (sinal, rótulo, distinção da recarga)? | A UI atual de histórico mostra recarga com “+” | `01-entendimento.md`; F-P8 |
| P3.1 | P3 | Como data e hora são apresentadas ao cliente? | RN04 exige data e hora; formato de apresentação ausente | `01-entendimento.md` |
| P3.2 | P3 | O botão “Usar cartão” / “Comprar” é o único ponto de entrada? | Escopo de experiência; conflito K5 | `01-entendimento.md`; `docs/ui.md` |
| P3.3 | P3 | Para onde o cliente vai depois da compra? | Ausente na história; não bloqueia saldo e registro | `01-entendimento.md` |

### Decisões necessárias (todas abertas)

| Decisão | Origem | Status |
| --- | --- | --- |
| Aprovar ou ajustar o escopo dentro/fora, inclusive o que a versão atual já exclui e a CP-001 não lista | `01-entendimento.md`; conflito K4 | **Aberta** |
| Definir o significado de histórico e o que o cliente vê no registro | `01-entendimento.md`; conflito K1; P1.1, P1.2 | **Aberta** |
| Definir “compra concluída” e o que a falha pode ou não deixar visível | `01-entendimento.md`; P1.3, P1.4 | **Aberta** |
| Relacionar a CP-001 ao item de `docs/backlog.md` | `01-entendimento.md`; P1.5 | **Aberta** |
| Congelar RN01–RN04 e CA01–CA05 como aceite oficial, ou registrar mudanças | `01-entendimento.md`; conflito K3 | **Aberta** |
| Intenção das mensagens e passo mínimo de confirmação | `01-entendimento.md`; P2.1, P2.2 | **Aberta** |

---

## Referências

| Documento | Papel nesta história |
| --- | --- |
| `docs/demandas/CP-001-compra-com-saldo/CardPlay_Historia_de_Usuario_Visual.pdf` | História recebida: contexto, enunciado, RN01–RN04, CA01–CA05, fora de escopo |
| `docs/demandas/CP-001-compra-com-saldo/01-entendimento.md` | Leitura anterior: fatos, conflitos, hipóteses, perguntas e decisões abertas |
| `docs/product.md` | Comportamento atual: recarga, histórico de recargas, catálogo, compra inexistente |
| `docs/backlog.md` | Demanda genérica de usar saldo em produtos, sem regras |
| `README.md` | Versão atual sem compra |
| `docs/ui.md` | Botão “Usar cartão” visível e desabilitado |
| `AGENTS.md` | Compra permanece fora de implementação enquanto estiver só no backlog |

---

## Lacunas registradas (resumo)

- Nenhuma decisão de negócio aprovada foi encontrada.
- O decisor não está nomeado.
- O vínculo entre CP-001 e o backlog não está decidido.
- Histórico da compra, artefato de “concluída”, mensagens, confirmação e preço em caso de mudança permanecem perguntas.
- Fluxos e exceções acima do que RN/CA escrevem não foram completados.
