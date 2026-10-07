# CP-001 — Entendimento: Compra com saldo do CardPlay

## Metadados

| Campo | Valor |
| --- | --- |
| Demanda | CP-001 — Compra de produto com saldo CardPlay |
| Tipo | História de usuário (proposta para refinamento) |
| Fonte principal | `docs/demandas/CP-001-compra-com-saldo/CardPlay_Historia_de_Usuario_Visual.pdf` |
| Fontes de contraste (produto atual) | `docs/product.md`, `docs/backlog.md`, `README.md`, `AGENTS.md`, `docs/ui.md` |
| Status do entendimento | Rascunho analítico — **não aprovado**; a história está marcada como proposta para refinamento |
| Data | 2026-10-06 |
| Documento recebido | Preservado; não alterado nesta etapa |
| Próxima etapa sugerida | Esclarecer dúvidas P1 e registrar decisões de negócio antes de planejar |

**Seleção da fonte:** na pasta da demanda há um PDF candidato (`CardPlay_Historia_de_Usuario_Visual.pdf`). O arquivo `Unconfirmed 642000.crdownload` foi ignorado (download incompleto, não é história utilizável).

---

## Problema e resultado desejado

| Elemento | Conteúdo | Classificação | Fonte |
| --- | --- | --- | --- |
| Usuário | Cliente do CardPlay | **Fato** (história) | PDF CP-001 |
| Valor | Utilizar créditos do cartão e acompanhar o valor gasto | **Fato** (história) | PDF CP-001 |
| Problema | É possível recarregar, ver saldo/histórico e o catálogo, mas **não** comprar com o saldo | **Fato** (produto) + demanda na história | `docs/product.md`, README, backlog; PDF CP-001 |
| Resultado desejado | Comprar um produto do catálogo com o saldo do cartão, com saldo e registro da operação consistentes | **Fato** (história) | PDF CP-001 |

**História (texto do PDF):**  
Como cliente do CardPlay, quero comprar um produto do catálogo usando o saldo do meu cartão, para utilizar meus créditos e acompanhar o valor gasto.

---

## Fatos conhecidos (com fonte)

### Da história CP-001 (PDF)

| ID | Fato | Fonte |
| --- | --- | --- |
| F-H1 | CardPlay já permite recarregar, consultar saldo e histórico e visualizar o catálogo (contexto da proposta) | PDF CP-001 |
| F-H2 | A demanda é permitir compra de produto com créditos disponíveis, mantendo saldo e registro consistentes | PDF CP-001 |
| F-H3 | RN01: cada compra contempla uma unidade de um único produto, pelo preço cadastrado no catálogo | PDF CP-001 |
| F-H4 | RN02: o cliente identifica o cartão, seleciona o produto e confirma a compra após visualizar o produto e o valor a debitar | PDF CP-001 |
| F-H5 | RN03: exige cartão e produto existentes e saldo ≥ valor da compra; o saldo não pode ficar negativo | PDF CP-001 |
| F-H6 | RN04: conclusão exige débito e registro no histórico juntos; o registro deve conter produto, quantidade, valor, data e hora; falha não deixa débito sem compra nem compra concluída sem débito | PDF CP-001 |
| F-H7 | CA01–CA05 definidos (sucesso com saldo restante / saldo zero / insuficiente / inexistente / falha de efetivação) | PDF CP-001 |
| F-H8 | Fora do escopo desta história: carrinho, múltiplas unidades, cancelamento e estorno | PDF CP-001 |
| F-H9 | Documento intitulado “Proposta para refinamento” | PDF CP-001 |

### Do produto atual (repositório — contraste; **não** misturar com regras da história)

| ID | Fato | Fonte |
| --- | --- | --- |
| F-P1 | Já existe: solicitar cartão, saldo, recarga (valor > 0), histórico de **recargas**, catálogo via seed | `docs/product.md`, README |
| F-P2 | Botão “Usar cartão” / “Comprar” visível, desabilitado e marcado como “Em breve” / funcionalidade futura | `docs/product.md`, `docs/ui.md`, README, AGENTS.md |
| F-P3 | Compra **não** implementada nesta versão | `docs/product.md`, README, AGENTS.md |
| F-P4 | Backlog genérico: “utilizar saldo em produtos” — sem regras nem critérios de aceite | `docs/backlog.md` |
| F-P5 | Sem autenticação, estoque, carrinho, checkout, banco/gateway | `docs/product.md`, AGENTS.md, README |
| F-P6 | Cartão interno (não bancário); a API pode ter vários cartões; o app guarda o cartão selecionado localmente | `docs/product.md`, README |
| F-P7 | AGENTS.md orienta não implementar compra enquanto estiver só no backlog | AGENTS.md |
| F-P8 | A tela de histórico descreve “histórico simples das **recargas**”; cada item exibe descrição, valor com sinal “+” e data/hora | `docs/product.md`; UI de movimentações no mobile |
| F-P9 | O registro atual de movimentação expõe valor, descrição e data/hora — **não** produto nem quantidade como campos da história RN04 | Comportamento/DTO atuais do produto (contraste com F-H6) |

### Contraste explícito (ambiguidade preservada)

| O que a história diz | O que o produto atual faz / documenta |
| --- | --- |
| Registro da compra no **histórico** (produto, quantidade, valor, data/hora) | Histórico documentado e apresentado como de **recargas** (descrição, valor, data/hora) |
| Compra com saldo | Compra não existe; botão desabilitado |
| Critérios e RNs detalhados | Backlog só com frase genérica |

---

## Escopo (in / out) — só o confirmado na história

### In (confirmado no PDF CP-001)

- Compra de **uma unidade** de **um único** produto.
- Uso do **preço cadastrado** no catálogo.
- Identificação do cartão, seleção do produto e **confirmação** após ver produto e valor a debitar.
- Validação: cartão existente, produto existente, saldo ≥ preço; saldo não negativo.
- Efetivação consistente no sentido de negócio: débito + registro juntos; ou nenhum dos dois em caso de falha.
- Registro contendo: produto, quantidade, valor, data e hora.

### Out (confirmado no PDF CP-001)

- Carrinho
- Múltiplas unidades
- Cancelamento
- Estorno

> **Nota:** Autenticação, estoque, gateway etc. estão fora da **versão atual do produto** (docs), mas **não** estão listados no “fora do escopo” da história CP-001. Não tratar como exclusão formal da demanda até decisão explícita.

---

## Regras e critérios de aceite confirmados na proposta

> Classificação: **Fato** do texto da proposta. **Não** é decisão aprovada para implementação — o PDF ainda é “proposta para refinamento”.

### Regras de negócio (PDF)

| ID | Regra |
| --- | --- |
| RN01 | Uma unidade, um produto, preço do catálogo |
| RN02 | Identificar cartão → selecionar produto → confirmar após ver produto e valor |
| RN03 | Cartão e produto existentes; saldo ≥ valor; saldo não negativo |
| RN04 | Débito e registro juntos; registro com produto, quantidade, valor, data/hora; sem débito órfão nem compra sem débito |

### Critérios de aceite (PDF)

| ID | Critério |
| --- | --- |
| CA01 | Cartão R$ 50,00 + produto R$ 20,00 → compra concluída, saldo R$ 30,00, **um único** registro correspondente no histórico |
| CA02 | Saldo exatamente igual ao preço → compra concluída, saldo final zero |
| CA03 | Saldo menor que o preço → informar saldo insuficiente; **sem** debitar nem registrar compra concluída |
| CA04 | Cartão ou produto inexistente → informar o dado inválido; **sem** debitar nem registrar compra concluída |
| CA05 | Falha ao efetivar débito ou registro → informar que a compra não foi concluída; manter saldo anterior; sem registro de compra concluída |

---

## Hipóteses (explícitas, **não aprovadas**)

| ID | Hipótese | Por que é hipótese |
| --- | --- | --- |
| H1 | O “histórico” da compra é o **mesmo** histórico hoje usado para recargas (mesma lista na aba Histórico) | História fala em histórico de compra; produto atual só documenta histórico de recargas |
| H2 | O preço usado é o vigente no momento da confirmação (sem cotação separada) | RN01 cita preço cadastrado; não detalha o momento de leitura do preço |
| H3 | “Identificar o cartão” = usar o cartão já selecionado no app, sem um fluxo novo de escolha a cada compra | Produto atual guarda cartão localmente; história não especifica a UI de identificação |
| H4 | A quantidade no registro será sempre `1` (coerente com RN01) | RN04 exige quantidade; RN01 limita a uma unidade — combinação sugerida, não explícita |
| H5 | Não há entrega física, estoque nem “pedido” além do débito + registro | Não está no PDF; alinhado ao produto lúdico atual, mas **não** escrito na história |
| H6 | As mensagens de CA03/CA04/CA05 serão exibidas no fluxo do cliente (mobile), não só como resposta de API | História fala “informar”; canal não especificado |

> Nenhuma hipótese acima é decisão aprovada.

---

## Dúvidas abertas priorizadas

### P1 — Bloqueantes (impedem entendimento, escopo ou aceite)

| ID | Pergunta | Por que importa | Quem decide |
| --- | --- | --- | --- |
| P1.1 | O registro de compra entra na **mesma** lista de histórico das recargas, ou em outro histórico / visão? | CA01 exige “um único registro correspondente no histórico”; o produto atual só tem histórico de recargas — muda o significado de “acompanhar o valor gasto” e o aceite | Responsável pelo negócio / product owner (não documentado no material) |
| P1.2 | O que o registro de compra deve mostrar ao cliente para cumprir RN04 (produto, quantidade, valor, data/hora), dado que o histórico atual só mostra descrição, valor e data/hora? | Sem isso, CA01 não é verificável na experiência atual de “histórico” | Responsável pelo negócio / product owner (não documentado no material) |
| P1.3 | O que constitui “compra concluída” para o cliente além de saldo debitado + registro (comprovante, status, tela de sucesso obrigatória)? | CA01/CA02/CA05 usam “concluída” / “não foi concluída” sem definir o artefato mínimo de sucesso | Responsável pelo negócio / product owner (não documentado no material) |
| P1.4 | Em falha (CA05), o que o cliente deve ver e o que **não** pode aparecer no histórico (parcial, tentativa, erro)? | RN04 e CA05 proíbem registro de compra concluída, mas não dizem se pode existir rastro de tentativa | Responsável pelo negócio / product owner (não documentado no material) |
| P1.5 | Esta proposta CP-001 **substitui/refina** o item genérico do backlog ou é outra demanda? | AGENTS.md bloqueia compra “só no backlog”; sem vínculo formal, o status de “pode seguir no ciclo” fica ambíguo | Responsável pelo negócio / product owner (não documentado no material) |

### P2 — Importantes (afetam aceite ou experiência; a regra central já existe)

| ID | Pergunta | Por que importa | Quem decide |
| --- | --- | --- | --- |
| P2.1 | Textos exatos (ou intenção) de “saldo insuficiente”, “dado inválido” e “compra não concluída”? | CA03–CA05 exigem “informar”; sem intenção/critério, o aceite de mensagem não fecha | Responsável pelo negócio / product owner (não documentado no material) |
| P2.2 | Confirmação (RN02): é um passo dedicado (resumo produto + valor) ou basta um gesto no catálogo após ver o preço? | Define o fluxo mínimo testável de RN02 | Responsável pelo negócio / product owner (não documentado no material) |
| P2.3 | Se o preço do produto mudar entre visualização e confirmação, qual preço vale? | RN01 diz “preço cadastrado”; não cobre mudança entre etapas | Responsável pelo negócio / product owner (não documentado no material) |
| P2.4 | Com vários cartões na API e um selecionado no app: tentativa sem cartão selecionado ou com id inválido local conta como CA04? | Liga RN02/RN03 ao comportamento real do app | Responsável pelo negócio / product owner (não documentado no material) |
| P2.5 | Como o “valor gasto” deve aparecer no histórico (sinal, rótulo, distinção de recarga)? | A história pede acompanhar o valor gasto; a UI atual de histórico só apresenta recargas com “+” | Responsável pelo negócio / product owner (não documentado no material) |

### P3 — Refinamentos

| ID | Pergunta | Por que importa | Quem decide |
| --- | --- | --- | --- |
| P3.1 | Formato de apresentação de data/hora do registro ao cliente? | RN04 exige data e hora; detalhe de apresentação não está na história | Responsável pelo negócio / product owner (não documentado no material) |
| P3.2 | O botão “Usar cartão” / “Comprar” passa a ser o único ponto de entrada, ou há outros pontos no catálogo? | Afeta escopo de UX; docs atuais só descrevem o botão desabilitado | Responsável pelo negócio / product owner (não documentado no material) |
| P3.3 | Após compra, o cliente deve permanecer no catálogo, ir ao histórico ou ver o cartão atualizado primeiro? | Não está na história; não bloqueia a regra de saldo/registro | Responsável pelo negócio / product owner (não documentado no material) |

---

## Decisões necessárias

| Decisão | Por que | Quem decide | Status |
| --- | --- | --- | --- |
| Aprovar ou ajustar escopo in/out da CP-001 (incl. se autenticação/estoque permanecem fora) | Separar o que a história exclui do que o produto atual já exclui | Responsável pelo negócio / product owner (não documentado no material) | **Aberta** |
| Definir o significado de “histórico” para compra vs recargas e o que o cliente vê no registro (RN04) | Desbloqueia CA01 e o valor “acompanhar o valor gasto” | Responsável pelo negócio / product owner (não documentado no material) | **Aberta** |
| Definir “compra concluída” e falha CA05 (o que o usuário vê / o que não pode existir) | Torna CA01–CA05 testáveis de ponta a ponta | Responsável pelo negócio / product owner (não documentado no material) | **Aberta** |
| Relacionar CP-001 ao item do `docs/backlog.md` (refino, substituição ou novo item) | Status formal da demanda no ciclo Entender → Planejar | Responsável pelo negócio / product owner (não documentado no material) | **Aberta** |
| Congelar RN01–RN04 e CA01–CA05 como aceite oficial (ou lista de mudanças) | Documento ainda é “proposta para refinamento” | Responsável pelo negócio / product owner (não documentado no material) | **Aberta** — proposta, não aprovada |
| Textos / intenção das mensagens CA03–CA05 e passo mínimo de confirmação RN02 | Aceite de experiência e validação | Responsável pelo negócio / product owner (não documentado no material) | **Aberta** |

---

## O que NÃO foi decidido / fora desta etapa

- **Não decidido:** aprovação da história; textos de erro/sucesso; modelo único vs dual de histórico; fluxo de confirmação detalhado; vínculo formal com o backlog; prioridade.
- **Fora desta etapa:** solução técnica, desenho de API, modelo de dados, implementação, issues no GitHub, estimativa.
- **Não inventado aqui:** regras além das do PDF; critérios além de CA01–CA05; inclusão de carrinho, multiunidade, cancelamento ou estorno.
- **Perguntas ≠ decisões:** itens da seção de dúvidas permanecem abertos até registro explícito pelo responsável de negócio.

---

*Classificação usada neste documento: **Fato** (afirmado na fonte citada) · **Hipótese** (possível, não aprovada) · **Pergunta** (aberta) · **Decisão necessária** (bloqueia avanço sem dono explícito).*
