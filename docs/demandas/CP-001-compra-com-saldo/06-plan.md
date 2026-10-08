# CP-001 — Plano de implementação: Compra com saldo

## Estado deste documento

| Campo | Valor |
| --- | --- |
| Demanda | CP-001 — Compra de produto com saldo CardPlay |
| Artefato | Plano para avaliação humana, antes de autorizar código |
| Fontes | `01-entendimento.md`, `02-historia-rica.md`, `03-criterios-de-aceite.md`, `04-exploracao-repositorio.md`, `05-analise-de-impacto.md` |
| Arquitetura usada | A do artefato 04: o que o repositório já tem. Este plano não desenha outra arquitetura |
| Data | 2026-10-07 |
| Decisões de negócio aprovadas | Nenhuma localizada nas fontes da pasta |
| Estado de RN01–RN04 e ACE-01–ACE-08 | **Proposta**. As etapas os citam como origem observável. Este arquivo não os aprova |
| Efeito deste arquivo | Organiza a ordem e os pontos de inspeção. **Não aprova o plano e não autoriza execução** |

Legenda: **Fato** (código ou documento já citado nos artefatos 04 e 05) · **Proposta** (escrito na história, sem aprovação) · **Lacuna** (a alteração da etapa não pode ser especificada) · **Componente proposto** (ainda não existe; a demanda o exige e este plano só o nomeia).

---

## Veredito

O conjunto **não está pronto para autorização de código**.

Dá para ordenar o trabalho e apontar arquivos reais. Não dá para escrever a alteração de nenhuma etapa de produto: as pendências P1 e as escolhas de arquitetura em aberto no artefato 05 mudam o tipo, a rota, a tela e o teste. Este plano não escolhe essas alternativas.

Nenhuma etapa abaixo deve ser executada a partir da publicação deste arquivo. A primeira ação, se a revisão quiser seguir, é responder os bloqueios da etapa 0. Sem isso, as etapas 1 a 8 permanecem paradas.

---

## Escopo e limites

Dentro do que a proposta escreve, e só como proposta:

- Uma unidade de um único produto, pelo preço cadastrado no catálogo.
- Identificar o cartão, selecionar o produto, ver o produto e o valor, e só então confirmar.
- Cartão existente, produto existente, saldo maior ou igual ao preço, saldo final maior ou igual a zero.
- Conclusão com débito e registro juntos. O registro contém produto, quantidade, valor, data e hora.
- Recusa com saldo insuficiente, dado inválido (cartão ou produto inexistente) ou falha de efetivação, sem débito e sem compra concluída.

Fora do que a proposta escreve:

- Carrinho, múltiplas unidades, cancelamento e estorno.

Este plano também não acrescenta o que a proposta não escreve e o código atual não faz: autenticação, estoque, administração de catálogo, checkout, gateway, uso de `Produto.Disponivel` como trava, token de concorrência e aba nova. O conflito K4 continua aberto. Excluir esses assuntos das etapas evita ampliar a demanda; não decide que a história os exclui.

Comportamento atual que cada etapa futura precisa continuar mostrando, porque a CP-001 não o redefine (artefato 05):

- Solicitar cartão, saldo inicial zero, nome de 2 a 80 caracteres.
- Recarga com valor menor ou igual a zero rejeitada, sem mudar saldo nem movimentação.
- Recarga válida soma o saldo e grava a movimentação no mesmo `SaveChanges`.
- Descrição de recarga em branco vira “Recarga”.
- Histórico de recargas da mais nova para a mais antiga.
- Catálogo em uma coluna abaixo de 720 px e duas a partir disso.
- Botão “Usar cartão” visível e desabilitado, com “Em breve”, até a etapa de interface ser autorizada em separado. `docs/ui.md` e `AGENTS.md` ainda mandam mantê-lo assim.

O par R$ 50,00 / R$ 20,00 / R$ 30,00 é o exemplo de CA01, não uma segunda regra (`03-criterios-de-aceite.md`). O seed não tem produto a R$ 20,00 (artefato 05, I-03). Nenhuma etapa cria produto, altera preço ou muda o seed para reproduzir esse par. A evidência nomeada para ACE-01 é a forma geral já escrita: saldo final igual ao saldo anterior menos o preço cadastrado de um produto que já existe.

---

## O que o plano considera e o que deixa de fora

Considera como fato a arquitetura do artefato 04 e os impactos confirmados do artefato 05: não há compra; `Cartao.Recarregar` só soma; `MovimentacaoCartao` não tem produto nem quantidade; `IProdutoRepositorio` só lista; `TelaCatalogo` não lê o cartão; o botão do catálogo não tem ação; a recarga junta saldo e movimentação num `SaveChanges`; os testes localizados não abrem SQLite.

Não usa como premissa as hipóteses H1, H2, H3, H4, H5 e H6. Não trata o id em AsyncStorage como a identificação de RN02, não trata o 404 atual como o “dado inválido” de ACE-07, e não trata o 400 da recarga como o “saldo insuficiente” de ACE-06.

---

## Lacunas que impedem especificar a alteração

Enquanto estes itens seguirem abertos, a etapa correspondente não tem diff definido. Quem decide o negócio não está nomeado nas fontes. Onde a pendência é de arquitetura, o código também não escolhe.

| Bloqueio | O que falta | Etapas que ficam sem alteração |
| --- | --- | --- |
| K3 | Aprovar RN01–RN04 e ACE-01–ACE-08, ou registrar o que muda | 1 a 8. O plano inteiro verifica a proposta |
| P1.5 / K2 | Se a CP-001 refina, substitui ou convive com a frase de `docs/backlog.md` | 0 e 8. `AGENTS.md` ainda manda não implementar compra enquanto ela estiver só no backlog |
| P1.1 / K1 | Se o registro entra na mesma lista das recargas ou em outro histórico | 1, 4, 6 e 7 |
| P1.2 | O que o cliente vê para produto, quantidade, valor, data e hora | 1, 4, 6 e 7 |
| H4 | Qual número o campo quantidade grava | 1 |
| Pendência de arquitetura: onde o caso de uso vive e qual contrato HTTP usa | Não há operação que receba cartão e produto. Escolher classe, método, rota ou DTO seria desenho | 3 e 4 |
| Pendência de arquitetura: como persistir produto e quantidade | Coluna nova, entidade nova ou texto em `Descricao` são modelos diferentes | 1 e 2 |
| P2.1 e a pendência sobre reutilizar o 400 e o 404 atuais | Texto, canal e se as mensagens de recarga e de cartão ausente servem para ACE-06 e ACE-07 | 3, 4 e 5 |
| P1.4 | Se a falha pode deixar rastro que não seja compra concluída | 3. Também impede especificar a evidência de ACE-08 |
| P2.2 | Resumo dedicado ou gesto depois de ver o preço | 5 |
| P3.2 / K5 | Como a compra passa a ser oferecida, inclusive o botão desabilitado | 5 |
| P2.5 | Sinal, rótulo e distinção do valor gasto | 6 |
| P3.1 | Formato de data e hora da compra ao cliente | 6. `formatarDataHora` só descreve a recarga |
| P1.3 | Se “compra concluída” exige algo além de saldo debitado e registro | Nenhuma etapa adiciona comprovante ou tela de sucesso. Outro artefato continuaria fora do plano |
| P3.3 | Para onde o cliente vai depois da compra | Nenhuma etapa define navegação posterior |
| P2.3 | Qual preço vale se o cadastrado mudar entre a visualização e a confirmação | Nenhuma etapa. Não há administração de preço no código atual |
| P2.4 | Cartão não selecionado ou id local inválido conta como dado inválido | ACE-07 no plano cobre só cartão inexistente e produto inexistente, como o artefato 03 |
| K4 | Se autenticação, estoque, checkout e banco entram nesta história | Nenhuma etapa os inclui. Uma resposta positiva criaria etapas que este plano não tem |
| ACE-08 | A proposta não diz como provocar a falha de débito ou de registro | A evidência dessa etapa não é especificável. O repositório em memória não grava de verdade (I-10) |

---

## Ordem

A ordem segue o caminho que a recarga já percorre (artefato 04): regra no domínio, gravação no repositório, orquestração na Application, HTTP na API, cliente em `mobile/src/api/`, tela por último. A interface não debita saldo. `CardPlay.Services` não entra: a proposta não cita serviço externo, e essa camada não tem classe.

```text
Etapa 0  decisões que desbloqueiam o diff
   ↓
Etapa 1  domínio: débito e registro juntos, com testes de domínio
   ↓
Etapa 2  mapeamento EF e migration, só se o modelo da etapa 1 mudar o esquema
   ↓
Etapa 3  orquestração e um SaveChanges, com testes de serviço
   ↓
Etapa 4  HTTP e tradução de erro
   ↓
Etapa 5  confirmação na interface e cliente HTTP
   ↓
Etapa 6  consulta do registro pelo cliente
   ↓
Etapa 7  documentos que hoje afirmam que a compra não existe
```

A etapa 8 não entra nessa linha: a evidência de ACE-08 continua sem mecanismo. Colocá-la no meio obrigaria a inventar uma falha.

Cada etapa para para inspeção humana antes da próxima. Juntar duas etapas consecutivas de código produz um diff grande demais para o critério deste plano; a seção de tamanho registra por quê.

---

## Etapas

### Etapa 0 — Registrar as decisões que o diff precisa

| Campo | Conteúdo |
| --- | --- |
| Objetivo | Ter resposta explícita para os bloqueios que mudam arquivo, contrato ou evidência |
| Critério de aceite | Nenhum comportamento de compra. A etapa libera, ou não, o planejamento executável das seguintes. K3 e P1.5 são a condição para qualquer código |
| Alteração prevista | Nenhuma em código de produto, teste ou documentação de produto. As respostas ficam num registro que a revisão indicar. Este plano não as antecipa |
| Componentes | Nenhum arquivo de `backend/` ou `mobile/`. Os documentos candidatos a receber a decisão já existem: `docs/backlog.md` e os artefatos 01 a 03. Não há dono de negócio nomeado |
| Motivo da ordem | `AGENTS.md` impede implementar compra enquanto ela estiver só no backlog. O modelo do registro, a rota e a tela dependem das outras respostas. Código antes disso fixaria uma hipótese |
| Validação e evidência | Lista escrita, uma resposta por bloqueio da tabela de lacunas, com o que muda em relação à proposta quando a resposta não for “segue o texto”. Ausência de resposta é evidência de que a etapa 1 continua bloqueada |
| Ponto de parada | Inspeção dessa lista. Sem ela, não há autorização para a etapa 1 |

### Etapa 1 — Débito e registro juntos no domínio

| Campo | Conteúdo |
| --- | --- |
| Objetivo | Aplicar, num único gesto de domínio, a redução do saldo e o nascimento do registro da compra, ou nenhum dos dois |
| Critério de aceite | Proposta ACE-01 (forma geral), ACE-03, ACE-04, ACE-05 e a parte de domínio de ACE-06: saldo insuficiente não altera saldo nem gera registro de compra concluída. ACE-01 no exemplo R$ 20,00 não é meta desta etapa |
| Alteração prevista | **Não especificada.** O método novo não pode ser `Recarregar`: esse método soma e os testes atuais fixam essa soma (I-04). O registro precisa carregar produto, quantidade, valor, data e hora (RN04), e `MovimentacaoCartao` não tem produto nem quantidade (I-05). Qual tipo guarda esses dados continua na pendência de arquitetura do artefato 05 |
| Componentes existentes | `backend/src/CardPlay.Domain/Entidades/Cartao.cs` (`Solicitar`, `Recarregar`). `backend/src/CardPlay.Domain/Entidades/MovimentacaoCartao.cs`. `backend/src/CardPlay.Domain/Entidades/Produto.cs` (`Preco`). `backend/src/CardPlay.Domain/Excecoes/RecargaInvalidaException.cs`, que permanece a recusa da recarga. Testes em `backend/tests/CardPlay.Tests/CartaoTestes.cs` |
| Componente proposto | Uma operação de débito em `Cartao`, distinta de `Recarregar`. A demanda a exige porque o saldo hoje só cresce. Se a decisão de persistência for uma entidade nova, em vez de campos em `MovimentacaoCartao`, essa entidade também seria proposta: não existe tipo de compra no domínio. O nome dela não está escolhido. Uma exceção só de saldo insuficiente também seria proposta, e só se a revisão recusar reutilizar `RecargaInvalidaException`; essa escolha está aberta |
| Motivo da ordem | A Application e o EF consomem o que o domínio definir. A recarga já nasce assim: saldo e movimentação no mesmo método, gravação depois |
| Validação e evidência | Quando desbloqueada: testes de domínio, sem banco, no estilo de `CartaoTestes`. Evidência esperada: saldo anterior menos o preço cadastrado; saldo zero quando os valores forem iguais; saldo e movimentações intactos quando o saldo for menor; saldo final maior ou igual a zero; registro com produto, quantidade, valor, data e hora no mesmo caso em que o saldo muda. Os testes já existentes de recarga continuam passando. Enquanto a alteração estiver bloqueada, a evidência é a própria ausência de diff |
| Ponto de parada | Inspeção do domínio e dos testes novos, antes de migration, serviço ou HTTP |

Bloqueios desta etapa: K3, P1.5, P1.1, P1.2, H4 e a forma de persistir produto e quantidade.

O número gravado em quantidade espera H4. ACE-01 exige uma unidade e exige o campo; não fixa o inteiro. A etapa não assume 1.

### Etapa 2 — Levar o modelo do registro ao SQLite

| Campo | Conteúdo |
| --- | --- |
| Objetivo | Fazer o esquema acompanhar só o que a etapa 1 tiver passado a guardar, no mesmo estilo de migration que o repositório já usa |
| Critério de aceite | Proposta ACE-03, no que depende de o débito e o registro existirem depois de gravar. A etapa sozinha não conclui compra |
| Alteração prevista | **Não especificada.** Pode ser coluna em `MovimentacoesCartao`, tabela nova, ou nenhuma mudança de esquema se a decisão for reutilizar `Descricao`. As três opções estão no artefato 05 (I-13). Este plano não escolhe |
| Componentes existentes | `CardPlayDbContext`, `MovimentacaoCartaoConfiguracao`, `CartaoConfiguracao`, `ProdutoConfiguracao`, migration `20260917150057_Inicial`. `CartaoRepositorio.SalvarAlteracoesAsync` já é um `SaveChangesAsync` e não precisa de outro método de gravação para repetir o padrão da recarga |
| Componente proposto | Nenhum tipo de repositório novo. Uma migration nova só entra se a etapa 1 mudar o esquema. O comando que o repositório já documenta para isso está em `AGENTS.md`; executá-lo fica fora deste plano |
| Motivo da ordem | A migration descreve o modelo já fechado no domínio. Fazê-la antes da etapa 1 gravaria um esquema ainda não decidido. O caso de uso da etapa 3 precisa do mapa já compilando |
| Validação e evidência | Quando houver mudança de esquema: a solution compila e a migration nova aparece em `backend/src/CardPlay.Repository/Migrations/`. Quando a decisão não mudar esquema: evidência de que nenhum arquivo de migration foi criado. Não abrir `cardplay.db` como prova; o artefato 04 não o inspecionou |
| Ponto de parada | Inspeção do diff de persistência, antes do caso de uso |

Bloqueios: os da etapa 1. Sem o modelo, não há arquivo de migration para revisar.

`Descricao` tem tamanho máximo 120. Se alguém, na etapa 0, escolher enfiar produto e quantidade nesse texto, esse limite passa a ser restrição da compra. Isso não é recomendação; é um fato que a revisão precisa ver antes de escolher esse caminho.

### Etapa 3 — Orquestrar a compra e gravar uma vez

| Campo | Conteúdo |
| --- | --- |
| Objetivo | Buscar o cartão e o produto, aplicar a regra da etapa 1 e persistir saldo e registro no mesmo `SaveChanges`, ou não persistir nenhum dos dois |
| Critério de aceite | Proposta ACE-01 (forma geral), ACE-03, ACE-04, ACE-05, ACE-06 (sem a redação da mensagem) e ACE-07 nos dois cenários do artefato 03: cartão inexistente e produto inexistente. P2.4 fica de fora |
| Alteração prevista | **Não especificada.** `CartaoServico` só depende de `ICartaoRepositorio`. `ProdutoServico` só lista. Nenhum dos dois recebe cartão e produto juntos (I-09, I-12). Colocar a orquestração num método novo de um serviço existente ou num tipo novo de Application são desenhos diferentes. Os dois estão em aberto |
| Componentes existentes | `ICartaoServico`, `CartaoServico`, `IProdutoServico`, `ProdutoServico`, `ICartaoRepositorio`, `IProdutoRepositorio`, `CartaoRepositorio`, `ProdutoRepositorio`, `Program.cs` (registro atual de `CartaoServico` e `ProdutoServico`). Testes em `CartaoServicoTestes.cs`, com `CartaoRepositorioEmMemoria` |
| Componente proposto | Uma leitura de um produto por identificador. O contrato existente `IProdutoRepositorio` não a declara; ACE-07 cenário B precisa dela, porque a listagem não responde a um id ausente. A assinatura não está escolhida. Se a revisão preferir um tipo novo de orquestração, esse tipo seria proposto e não existe hoje; o nome não está escolhido. O duplo de teste correspondente também seria novo, dentro de `CardPlay.Tests`, no mesmo estilo do repositório em memória já usado para cartão. Isso é teste, não componente de produção |
| Motivo da ordem | O serviço chama a regra e o repositório. Sem a etapa 1, não há regra para chamar. Sem a etapa 2, o modelo pode não compilar contra o EF. A API da etapa 4 só encaminha o que este caso de uso devolver |
| Validação e evidência | Quando desbloqueada: testes de serviço com repositório em memória, no estilo de `CartaoServicoTestes`. Evidência esperada: uma compra concluída altera o saldo pelo preço do produto carregado e guarda um registro; saldo igual ao preço termina em zero; saldo menor não grava; cartão ausente não grava; produto ausente não grava e, se o cartão existe, o saldo dele permanece. A suíte de recarga continua passando. Essa evidência não cobre ACE-08 |
| Ponto de parada | Inspeção do caso de uso e dos testes em memória, antes de controller |

Bloqueios adicionais: lugar do caso de uso; P2.1 no que for texto de exceção; P1.4, se o fluxo de falha passar a gravar tentativa. A etapa não cria rastro de tentativa, porque a proposta não o exige nem o proíbe.

`Produto.Disponivel` não participa. Nenhuma RN o cita (I-14).

### Etapa 4 — Expor a compra por HTTP

| Campo | Conteúdo |
| --- | --- |
| Objetivo | Receber a confirmação, chamar a orquestração e devolver saldo e registro, ou a informação de recusa, sem regra de saldo no controller |
| Critério de aceite | Os mesmos da etapa 3, agora observáveis na API: ACE-01 (forma geral), ACE-03, ACE-04, ACE-06 e ACE-07. ACE-02 ainda não, porque a ordem “ver e depois confirmar” é da interface. ACE-08 continua sem mecanismo |
| Alteração prevista | **Não especificada.** Não há rota, DTO de entrada nem corpo de compra (I-12). `RecargaRequisicao` traz valor e descrição, não produto. Inventar verbo, caminho ou status HTTP fecharia a pendência de contrato que o artefato 05 deixou aberta. O controller existente que receber a ação, ou um controller novo, também fica para essa decisão |
| Componentes existentes | `CartoesController`, `ProdutosController`, `TratadorExcecoes` (hoje: 400 para recarga ou nome inválido, 404 para cartão ausente, 500 para o restante), DTOs `CartaoResposta`, `MovimentacaoResposta`, `ProdutoResposta`, `RecargaRequisicao`. Cliente atual em `mobile/src/api/cartaoApi.ts` e `produtoApi.ts`, que esta etapa ainda não altera |
| Componente proposto | Um contrato HTTP de compra e o DTO de entrada correspondente. São propostos porque nenhum DTO existente carrega a confirmação de um produto. Forma, nomes e códigos HTTP não estão escolhidos. Estender `MovimentacaoResposta` com produto e quantidade, ou criar outra leitura, depende de P1.1 e P1.2; as duas opções mudam ou preservam o corpo que `listarMovimentacoes` já consome |
| Motivo da ordem | O aplicativo não grava banco. A tela só pode confirmar depois que a API tiver uma operação para chamar. O contrato também define o que a etapa 6 consegue mostrar |
| Validação e evidência | Quando desbloqueada: uma chamada HTTP de compra concluída, com saldo final igual ao anterior menos o preço cadastrado de um produto do seed, e um registro dessa compra; uma chamada com saldo menor, sem mudança de saldo e sem registro de compra concluída; uma chamada com cartão inexistente e outra com produto inexistente, com o mesmo efeito de não efetivar. O corpo precisa ser o observado na resposta, não só o DTO em C#: o artefato 04 não confirmou o JSON camelCase com uma chamada. Problema de saldo, dado inválido e falha seguem a informação pedida por ACE-06, ACE-07 e ACE-08; o texto literal continua em P2.1 |
| Ponto de parada | Inspeção do contrato e das chamadas, antes do aplicativo |

Bloqueios adicionais: P2.1 e a reutilização de 400/404; P1.1 e P1.2 se a resposta de sucesso incluir o histórico.

### Etapa 5 — Confirmar a compra na interface

| Campo | Conteúdo |
| --- | --- |
| Objetivo | O cliente identifica o cartão, seleciona o produto, vê o produto e o valor a debitar, e só então confirma, usando a API da etapa 4 |
| Critério de aceite | Proposta ACE-02. Também o trecho de ACE-01, ACE-04 e ACE-06 em que o gatilho é a confirmação do cliente. ACE-07 na interface só no que a etapa 0 tiver equiparado a “dado inválido”; P2.4 não entra por omissão |
| Alteração prevista | **Não especificada.** O único controle de compra é o `Pressable` desabilitado de `CardProduto`, sem `onPress`, com “Usar cartão” e “Em breve” (I-01). Habilitar esse botão, criar outro controle, ou usar os dois, é P3.2. Um passo de resumo dedicado seria tela nova; um gesto depois do preço já visível no card não exige tela nova. P2.2 não escolhe. Nenhuma tela nova entra neste plano |
| Componentes existentes | `mobile/src/componentes/CardProduto.tsx`, `mobile/src/telas/TelaCatalogo.tsx` (já mostra `produto.preco` e avisa que a compra não está disponível), `mobile/src/api/cartaoApi.ts`, `mobile/src/api/produtoApi.ts`, `mobile/src/api/clienteHttp.ts`, `mobile/src/estado/CartaoContexto.tsx`, `mobile/src/armazenamento/cartaoSelecionado.ts`, `mobile/src/tipos/index.ts`. O preço exibido já vem de `GET /api/produtos` |
| Componente proposto | Uma função de cliente HTTP para a operação da etapa 4. O aplicativo só fala com a API por `mobile/src/api/`. Em qual arquivo ela fica depende da rota ainda não escolhida. Não existe função de compra hoje. Atualizar o `CartaoProvider` depois de uma resposta de sucesso segue o que `recarregar` já faz: gravar o cartão devolvido e buscar de novo as movimentações. Isso não escolhe a aba de destino (P3.3). Sem essa atualização, Cartão e Histórico permanecem com o estado anterior (I-11) |
| Motivo da ordem | Depende do contrato da etapa 4 e das decisões de gesto (P2.2, P3.2, P2.1). O catálogo já mostra produto e preço; a etapa acrescenta a confirmação depois dessa visualização, não uma segunda origem de preço |
| Validação e evidência | Quando desbloqueada: percorrer no aplicativo o fluxo RN02 com um cartão de saldo maior que um preço do seed, e observar a confirmação só depois de o produto e o valor estarem visíveis; o valor visível igual ao preço do catálogo; em seguida o saldo do contexto coerente com o débito. Repetir com saldo menor e observar a informação de saldo insuficiente, saldo intacto e ausência de compra concluída. Não há pasta de testes em `mobile/`; a evidência desta etapa é o fluxo na interface, mais a chamada HTTP que ela disparar. `npx tsc --noEmit` confere compilação, não o fluxo |
| Ponto de parada | Inspeção do fluxo na interface, antes de mudar a leitura do histórico |

Bloqueios adicionais: P2.2, P3.2, P2.1, P1.3 (se a revisão exigir artefato de sucesso além de saldo e registro) e P3.3 (navegação). A etapa não muda `mobile/src/tema/cores.ts` nem a barra de abas.

O id em `@cardplay/cartaoId` é o único identificador que o aplicativo guarda. A etapa não o adota como RN02 até a etapa 0 dizer isso (P2.4, hipótese H3).

### Etapa 6 — Mostrar o registro da compra a quem consulta o histórico

| Campo | Conteúdo |
| --- | --- |
| Objetivo | O cliente consegue ver, no lugar decidido na etapa 0, o registro com produto, quantidade, valor, data e hora |
| Critério de aceite | Proposta ACE-01 e ACE-03 na parte “existe um único registro correspondente” e no conteúdo desse registro. P2.5 e P3.1 continuam sem critério de exibição até serem respondidos |
| Alteração prevista | **Não especificada.** A aba Histórico lê `movimentacoes` do contexto e mostra descrição, valor com prefixo “+ ” e `formatarDataHora`. Os textos falam em recargas. Colocar a compra nessa lista sem mudar a tela faria o valor gasto aparecer como crédito (hipótese de impacto do I-05), e isso não está aprovado |
| Componentes existentes | `mobile/src/telas/TelaMovimentacoes.tsx`, `mobile/src/util/formatacao.ts` (`formatarDataHora`), tipo `Movimentacao` em `mobile/src/tipos/index.ts`, `MovimentacaoResposta` |
| Componente proposto | Nenhum, enquanto P1.1 for a mesma lista. Se a decisão for outro histórico, a superfície nova seria componente proposto: não existe segunda lista no aplicativo. Este plano não a cria |
| Motivo da ordem | A apresentação depende do corpo definido na etapa 4 e da compra já confirmável na etapa 5. Mudar a tela de histórico antes disso altera a recarga sem ainda haver compra para mostrar |
| Validação e evidência | Quando desbloqueada: depois de uma compra concluída no fluxo da etapa 5, a consulta decidida mostra um registro dessa compra, com produto, quantidade, valor, data e hora, e as recargas anteriores continuam visíveis com o sinal e o texto que já têm. “Um único registro correspondente” significa um registro daquela compra, não um histórico vazio de recargas (artefato 03) |
| Ponto de parada | Inspeção da consulta, com uma recarga e uma compra no mesmo cartão, antes de alterar a documentação |

Bloqueios: P1.1, P1.2, P2.5, P3.1.

### Etapa 7 — Alinhar os documentos que negam a compra

| Campo | Conteúdo |
| --- | --- |
| Objetivo | Fazer `docs/product.md`, `docs/ui.md`, `docs/architecture.md`, `docs/backlog.md`, `README.md` e `AGENTS.md` descreverem o que tiver sido implementado e decidido, no ponto em que a compra deixar de ser só backlog |
| Critério de aceite | Nenhum ACE novo. A etapa remove a contradição K2 / K5 / I-17 para o comportamento já inspecionado nas etapas anteriores |
| Alteração prevista | **Não especificada.** O texto depende de P1.5, K3 e do que as etapas 5 e 6 tiverem mostrado. Inclui, quando a implementação existir, a regra de `AGENTS.md` que manda não implementar compra e manter o botão desabilitado |
| Componentes existentes | Os seis documentos da tabela I-17. Nenhum outro guia entra no escopo |
| Componente proposto | Nenhum |
| Motivo da ordem | Documentar antes do comportamento aprovado gravaria uma compra que o repositório ainda proíbe. Documentar no mesmo diff do domínio mistura revisão de regra com revisão de texto |
| Validação e evidência | Leitura cruzada: cada frase desses arquivos que hoje diz que a compra não existe, que o histórico é só de recargas, ou que o botão permanece desabilitado, ou foi atualizada para o comportamento já visto, ou permanece de propósito com o motivo escrito na etapa 0 |
| Ponto de parada | Inspeção documental. Fim do plano especificável |

Bloqueios: K3, P1.5 e o resultado das etapas 5 e 6.

### Etapa 8 — Falha ao efetivar débito ou registro

| Campo | Conteúdo |
| --- | --- |
| Objetivo | Observar ACE-08: falha no débito ou no registro informa que a compra não foi concluída, mantém o saldo anterior e não deixa compra concluída nem débito órfão |
| Critério de aceite | Proposta ACE-08. O artefato 03 não exige nem proíbe rastro de tentativa (P1.4) |
| Alteração prevista | **Não há alteração especificável.** A proposta não descreve como provocar a falha. Não existe caso de uso de compra para falhar. Os testes de recarga inválida lançam antes de alterar o cartão; isso não é falha de gravação. `CartaoRepositorioEmMemoria.SalvarAlteracoesAsync` só incrementa um contador (I-10). Introduzir falha artificial, teste de SQLite ou transação extra seria desenho que a demanda não pediu |
| Componentes existentes | `CartaoRepositorio.SalvarAlteracoesAsync`, testes em memória, `TratadorExcecoes` para exceção não mapeada (HTTP 500) |
| Componente proposto | Nenhum. Um mecanismo de falha não está justificado pela proposta |
| Motivo da ordem | Fora da linha principal de propósito. Dependeria da etapa 3 para existir algo que possa falhar, e mesmo assim a evidência continuaria sem definição |
| Validação e evidência | **Lacuna.** Não há evidência observável nomeável sem inventar a falha. O mesmo `SaveChanges` da recarga é o padrão disponível; copiá-lo não demonstra ACE-08 |
| Ponto de parada | A revisão decide se ACE-08 permanece critério sem evidência automatizada, ou se descreve o modo de falha. Este plano para aqui |

---

## Cobertura da demanda

| Origem | Onde o plano a coloca | Situação |
| --- | --- | --- |
| RN01, uma unidade, um produto, preço do catálogo | Etapas 1, 3 e 5. O preço visto já existe no catálogo | A quantidade gravada espera H4. O preço depois de uma mudança espera P2.3 e não tem etapa |
| RN02, identificar, selecionar, ver, confirmar | Etapa 5 | Bloqueada por P2.2, P3.2 e P2.4 |
| RN03, existentes, saldo suficiente, saldo não negativo | Etapas 1 e 3; informação ao cliente nas etapas 4 e 5 | Texto e canal em P2.1 |
| RN04, débito e registro juntos, com produto, quantidade, valor, data e hora | Etapas 1, 2, 3 e 6 | Modelo e lista em aberto |
| ACE-01 | Etapas 1, 3, 4, 5 e 6, na forma geral | O par R$ 50,00 / R$ 20,00 / R$ 30,00 não tem produto no seed e não ganha etapa |
| ACE-02 | Etapa 5, mais o preço que o catálogo já mostra | Forma do passo em aberto |
| ACE-03 | Etapas 1, 2 e 3 | Depende do modelo |
| ACE-04 | Etapas 1, 3 e 4 | Depende do modelo |
| ACE-05 | Consequência das etapas 1 e 3, nos casos de ACE-01, ACE-04 e ACE-06 | Sem outro piso numérico |
| ACE-06 | Etapas 1, 3, 4 e 5 | A informação está pedida; a redação, não |
| ACE-07 cenários A e B | Etapas 3 e 4. O cenário B precisa da leitura de produto por id, que não existe | Mensagem única de “dado inválido” não está equiparada ao 404 atual |
| ACE-08 | Etapa 8 | Sem evidência especificável |
| Fora da proposta: carrinho, múltiplas unidades, cancelamento, estorno | Sem etapa | Também sem critério de recusa, como no artefato 03 |
| K4, I-14, I-19, P3.3, P1.3 além de saldo e registro | Sem etapa | Permanecem dúvida, não trabalho escondido |

Nenhum ACE fica sem dono de etapa. ACE-08 tem dono e não tem alteração. Vários ACE têm dono e continuam incompletos pelos bloqueios da etapa 0. Cobertura de rastreio não é cobertura executável.

---

## Tamanho das etapas

Nenhuma etapa de código, mesmo depois de desbloqueada, deve absorver a seguinte.

| Se alguém juntar | Por que fica grande demais |
| --- | --- |
| Etapas 1 e 2 | Regra de saldo e esquema SQLite passam a ser um diff só. A inspeção não consegue aceitar a regra com o banco ainda incerto, nem o banco sem a regra já testada |
| Etapas 2 e 3 | Migration e orquestração misturam esquema com caso de uso. O serviço ainda não é necessário para revisar a tabela |
| Etapas 3 e 4 | Caso de uso e HTTP. O controller deixaria de ser só encaminhamento revisável à parte, e o contrato nasceria no mesmo diff da regra |
| Etapas 4 e 5 | API e gesto de interface. P3.2 pode rejeitar o botão sem que a API esteja errada; o diff único esconde isso |
| Etapas 5 e 6 | Confirmação e reformulação do histórico. A tela de recargas muda junto com o catálogo |
| Etapas 6 e 7 | Comportamento e reescrita de `AGENTS.md`, `docs/ui.md` e do backlog. A autorização de documento é outra decisão |
| Uma etapa única “implementar a compra” | Atravessa domínio, EF, Application, API, cliente, catálogo, histórico e documentação. Não há ponto de parada no meio |

A etapa 1, sozinha, ainda é o maior bloco de regra: método de débito, forma do registro e testes de domínio. Ela não inclui EF, serviço nem tela. Dividi-la mais, separando “campos do registro” de “método que debita”, deixaria campo sem regra ou débito sem o registro que RN04 exige no mesmo gesto. Por isso permanecem juntos, e permanecem bloqueados até o modelo existir.

A etapa 5 também concentra cliente HTTP, estado do `CartaoProvider` e o gesto no catálogo. São três arquivos de camadas diferentes, todos necessários para ACE-02 ser observável. Separar o cliente HTTP sem gesto não atende ACE-02 e criaria uma função sem chamada. O limite é: nenhuma tela nova e nenhuma mudança de histórico dentro dessa etapa.

---

## Componentes

### Já existentes, citados por alguma etapa

Domínio: `Cartao`, `MovimentacaoCartao`, `Produto`, `RecargaInvalidaException`, `CartaoNaoEncontradoException`.

Application: `CartaoServico`, `ProdutoServico`, `ICartaoRepositorio`, `IProdutoRepositorio`, DTOs de resposta e `RecargaRequisicao`.

Repository: `CardPlayDbContext`, `CartaoRepositorio`, `ProdutoRepositorio`, configurações, migration `Inicial`.

API: `CartoesController`, `ProdutosController`, `TratadorExcecoes`, `Program.cs`.

Aplicativo: `CardProduto`, `TelaCatalogo`, `TelaMovimentacoes`, `CartaoContexto`, `cartaoApi`, `produtoApi`, `clienteHttp`, `cartaoSelecionado`, `tipos`, `formatarDataHora`.

Testes: `CartaoTestes`, `CartaoServicoTestes`.

Documentos: `docs/product.md`, `docs/ui.md`, `docs/architecture.md`, `docs/backlog.md`, `README.md`, `AGENTS.md`.

`CardPlay.Services` existe e não recebe etapa.

### Propostos, porque a demanda não cabe no que existe

Estes itens **não existem** no repositório. Entram só como necessidade da proposta, sem nome fechado e sem implementação.

| Proposto | Por que a demanda o pede | Por que não é um tipo que já está no código |
| --- | --- | --- |
| Operação de débito em `Cartao` | RN03 e ACE-01 pedem saldo anterior menos o preço | `Recarregar` só soma. Reutilizá-lo muda a recarga |
| Leitura de produto por identificador em `IProdutoRepositorio` | ACE-07 cenário B exige produto inexistente | A interface só declara `ListarAsync` |
| Orquestração que recebe cartão e produto | RN02 junta os dois na confirmação | `CartaoServico` não conhece produto; `ProdutoServico` não conhece cartão. Se isso virar tipo novo, o tipo é proposto. Se virar método num serviço já citado, o tipo não é novo; o método é |
| Contrato HTTP de compra e DTO de entrada | O aplicativo não debita sozinho, e nenhum DTO atual confirma um produto | Rotas atuais: solicitar, obter, recarregar, listar movimentações, listar produtos |
| Função em `mobile/src/api/` para essa operação | O cliente HTTP vive só nessa pasta | `cartaoApi` e `produtoApi` não compram |
| Entidade nova de compra, ou campos novos no registro | RN04 pede produto e quantidade | Só se a etapa 0 recusar os campos atuais. `MovimentacaoCartao` não os tem |
| Superfície nova de histórico | Só se P1.1 escolher outra lista | A aba Histórico é a única lista |

Não são propostos: tela de confirmação, comprovante, exceção de saldo insuficiente, repositório genérico, uso de `CardPlay.Services`, filtro por `Disponivel`, controle de concorrência. Cada um dependeria de uma decisão que a proposta não toma.

---

## Riscos

| Risco | Onde aparece | O que a inspeção olha |
| --- | --- | --- |
| A recarga passa a debitar ou a rejeitar com outra regra | Etapa 1, se o método novo alterar `Recarregar` | Testes já nomeados em `CartaoTestes` e `CartaoServicoTestes` continuam descrevendo soma e valor `<= 0` |
| O histórico mostra compra com “+ ” | Etapa 6, se a mesma lista for reutilizada sem decisão de P2.5 | Uma recarga e uma compra no mesmo cartão, antes de aceitar a tela |
| Saldo velho na aba Cartão | Etapa 5, se a resposta não atualizar o `CartaoProvider` | Saldo da aba Cartão depois da confirmação, sem reabrir o aplicativo |
| Duas confirmações no mesmo saldo | I-19. Fora da proposta; o código não tem token de concorrência | Não vira etapa. Se a revisão incluir o assunto, o plano muda |
| `number` no TypeScript e `decimal` na API | O aplicativo já usa `number` em saldo, valor e preço. A proposta não fala disso | A evidência HTTP da etapa 4 observa o valor de um preço do seed com centavos, por exemplo 8,50 ou 32,90 |
| JSON em camelCase não confirmado com chamada | Artefatos 04 e 05 | A etapa 4 não aceita só a leitura do DTO |
| ACE-08 dado como coberto por teste em memória | Etapa 8 | Esse teste não abre SQLite e o `SaveChanges` em memória não grava |
| Documentos e botão divergentes do código durante as etapas 1 a 4 | I-17. O botão segue desabilitado até a etapa 5 ser autorizada | Nenhuma etapa anterior muda `CardProduto` nem `AGENTS.md` |
| K4 respondido depois com gateway ou estoque | Etapa 0 | As etapas 1 a 7 não servem mais; Services e estoque entrariam como plano novo |

---

## Dúvidas

Nenhuma foi respondida aqui. A lista fechada está nos artefatos 01, 02, 03 e 05. As que impedem este plano de ficar executável são as da tabela de lacunas. Em especial, sem resposta não há diff:

- K3, P1.5 / K2
- P1.1 / K1, P1.2, H4 e a forma de persistir produto e quantidade
- O lugar do caso de uso e o contrato HTTP
- P2.1 e o uso dos status já mapeados
- P2.2 e P3.2 / K5
- P1.4, para a evidência de ACE-08

As demais continuam abertas e fora das etapas: P1.3 além de saldo e registro, P2.3, P2.4, P2.5, P3.1, P3.3, K4, `Disponivel` e concorrência.

---

## O que este arquivo não faz

- Não aprova RN, ACE nem este plano.
- Não autoriza implementação, migration, teste ou edição dos documentos de produto.
- Não escolhe rota, DTO, entidade nova, texto de erro, botão ou lista de histórico.
- Não cria componente no código. Onde um componente novo seria necessário, ele está marcado como proposto e justificado pela demanda.
- Não trata hipótese como decisão.
