# CP-001 — Análise de impacto: Compra com saldo

## Estado deste documento

| Campo | Valor |
| --- | --- |
| Demanda | CP-001 — Compra de produto com saldo CardPlay |
| Artefato | Análise de impacto, antes de planejar ou implementar |
| Fontes da demanda | `02-historia-rica.md`, `03-criterios-de-aceite.md`, `01-entendimento.md` |
| Mapa de arquitetura | `04-exploracao-repositorio.md`, recarregado neste cruzamento com o código citado abaixo |
| Data | 2026-10-07 |
| Decisões de negócio aprovadas | Nenhuma localizada nas fontes da pasta |
| Estado de RN01–RN04 e ACE-01–ACE-08 | **Proposta**. O impacto abaixo descreve o que essas regras encontrariam no sistema atual. Não as aprova e não escolhe desenho |
| Escrita desta etapa | Somente este arquivo |

Legenda de grau:

| Grau | Significado |
| --- | --- |
| **Confirmado** | Visto no código ou no texto da proposta, com caminho e símbolo |
| **Hipótese de impacto** | Consequência possível se uma pergunta aberta for respondida de um jeito. Não é requisito |
| **Lacuna** | Componente, campo, rota, teste ou decisão ausente |
| **Decisão pendente** | Precisa de revisão humana. Lista fechada no final |

O artefato 04 foi relido contra o código. O mapa de recarga, catálogo, histórico e ausência de compra permanece válido. Este arquivo acrescenta o que esse mapa não ligava ainda a cada regra, e o que ele não destacava para o impacto. A API não foi chamada nesta leitura: o processo em `localhost:5080` não estava escutando. O contrato HTTP abaixo vem dos controllers, DTOs e do cliente do aplicativo.

---

## Dependências que uma compra atravessaria

Caminho que existe hoje, confirmado no código. A compra não entra nele.

```text
TelaCatalogo → produtoApi.listar → GET /api/produtos
                                 → ProdutoServico.ListarAsync
                                 → ProdutoRepositorio.ListarAsync

TelaRecarga → CartaoProvider.recarregar → cartaoApi.recarregar
           → POST /api/cartoes/{id}/recargas
           → CartaoServico.RecarregarAsync
           → Cartao.Recarregar
           → CartaoRepositorio.AdicionarMovimentacao + SalvarAlteracoesAsync
           → em seguida GET /api/cartoes/{id}/movimentacoes
           → TelaMovimentacoes lê o contexto
```

Dependências confirmadas que separam catálogo e cartão:

| De | Para | O que existe |
| --- | --- | --- |
| `CartaoServico` | `ICartaoRepositorio` | Única dependência do serviço. Não há referência a produto |
| `ProdutoServico` | `IProdutoRepositorio` | Só `ListarAsync` |
| `TelaCatalogo` | `produtoApi` | Não usa `useCartao` nem `CartaoProvider` |
| `TelaMovimentacoes` | `useCartao` | Não chama a API sozinha |
| `CardPlay.Api` | `TratadorExcecoes` | Mapeia três exceções de domínio |
| `CardPlay.Services` | — | Projeto referenciado pela API, sem classe e sem registro em `Program.cs` |

`Program.cs` registra `CardPlayDbContext`, `CartaoRepositorio`, `ProdutoRepositorio`, `CartaoServico` e `ProdutoServico`.

---

## Impactos

Cada item liga uma regra ou critério a uma evidência. A consequência é o que muda, ou o que fica sem caminho, se a proposta continuar como está escrita. Nenhuma linha escolhe a solução.

### I-01 — Oferecer a compra na interface

| Campo | Conteúdo |
| --- | --- |
| Regra ou critério | RN02; ACE-02; conflito K5; pergunta P3.2 |
| Grau | **Confirmado** o estado atual. **Decisão pendente** o ponto de entrada |

Evidência: `CardProduto` renderiza um `Pressable` com `disabled`, rótulos “Usar cartão” e “Em breve”, e sem `onPress` (`mobile/src/componentes/CardProduto.tsx`). `TelaCatalogo` informa “A compra ainda não está disponível nesta versão.” O preço visível é `formatarMoeda(produto.preco)`. `docs/ui.md` e `AGENTS.md` mandam manter esse botão visível e desabilitado. `docs/product.md` também cita “Comprar”; esse texto não aparece em `.tsx`.

Consequência: o único controle de compra na interface está desligado. Aceitar RN02 exige um gesto de seleção e de confirmação que esse controle não dispara. Onde esse gesto fica — esse botão, outro controle no catálogo, ou os dois — continua em P3.2. Até essa decisão, habilitar o botão e manter a regra atual de `docs/ui.md` são leituras incompatíveis.

### I-02 — Identificar o cartão

| Campo | Conteúdo |
| --- | --- |
| Regra ou critério | RN02; ACE-01; ACE-07; pergunta P2.4; hipótese H3 |
| Grau | **Confirmado** o mecanismo atual. **Hipótese de impacto** tratá-lo como a identificação da proposta |

Evidência: não há login (`docs/product.md`, C7). O aplicativo guarda um id em AsyncStorage, chave `@cardplay/cartaoId` (`mobile/src/armazenamento/cartaoSelecionado.ts`). `CartaoProvider` carrega esse id ao abrir e o reutiliza. `TelaRecarga`, sem cartão no contexto, não chama a API e mostra “Solicite um cartão na primeira aba para adicionar saldo.” A API identifica o cartão pelo `Guid` da rota (`CartoesController`).

Consequência: qualquer chamada de compra precisaria de um id. O único id que o aplicativo já conserva é esse. A proposta não diz que esse id é a identificação de RN02. Cartão ausente no aparelho e id local que a API não encontra continuam fora de ACE-07 (P2.4).

### I-03 — Preço cadastrado mostrado ao cliente

| Campo | Conteúdo |
| --- | --- |
| Regra ou critério | RN01; ACE-01; ACE-02; pergunta P2.3; hipótese H2 |
| Grau | **Confirmado** a origem do preço exibido. **Decisão pendente** o preço se ele mudar antes da confirmação |

Evidência: `Produto.Preco` é `decimal`. O seed grava os preços na migration `Inicial` e em `ProdutoConfiguracao.HasData`. `ProdutoServico.ListarAsync` devolve `ProdutoResposta.Preco`. O card mostra esse valor. Não há outro preço no domínio, nem leitura de preço dentro de `Cartao`.

Consequência: o valor que o cliente vê no catálogo é o preço cadastrado devolvido por `GET /api/produtos`. ACE-02 exige que o valor visto antes da confirmação seja esse preço. A proposta não diz o que vale se o cadastrado mudar entre a visualização e a confirmação (P2.3). Não há, no código, tela de administração que altere preço.

Os preços do seed são 8,50; 32,90; 79,00; 4,00; 18,75 e 59,90. Nenhum é 20,00. O par R$ 50,00 / R$ 20,00 / R$ 30,00 de CA01 não se reproduz com o catálogo gravado. A forma geral de ACE-01, saldo maior que o preço cadastrado do produto existente, não depende desse par.

### I-04 — Debitar o saldo

| Campo | Conteúdo |
| --- | --- |
| Regra ou critério | RN01; RN03; ACE-01; ACE-04; ACE-05 |
| Grau | **Confirmado** |

Evidência: `Cartao.Saldo` só é alterado em `Solicitar` (zero) e em `Recarregar` (`Saldo += valor`). `Recarregar` rejeita valor `<= 0` com `RecargaInvalidaException` e, no caso válido, soma. Não há método de débito. A coluna `Cartoes.Saldo` na migration `Inicial` é `decimal` com precisão 18 e escala 2, sem restrição de saldo não negativo.

Consequência: ACE-01 e ACE-04 pedem saldo anterior menos o preço, inclusive zero. Esse cálculo não existe. ACE-05 (saldo final maior ou igual a zero) também não é regra do banco: hoje o saldo não diminui. Introduzir a subtração em `Recarregar` alteraria a regra de recarga que os testes já fixam (seção de preservação).

### I-05 — Registro com produto, quantidade, valor, data e hora

| Campo | Conteúdo |
| --- | --- |
| Regra ou critério | RN04; ACE-01; ACE-03; ACE-04; conflito K1; perguntas P1.1, P1.2, P2.5, P3.1; hipótese H4 |
| Grau | **Confirmado** a ausência dos campos. **Decisão pendente** o modelo e a lista em que o cliente vê o registro |

Evidência: `MovimentacaoCartao` tem `Id`, `CartaoId`, `Valor`, `Descricao` e `DataHora`. `MovimentacaoResposta` e o tipo `Movimentacao` do aplicativo repetem `id`, `valor`, `descricao` e `dataHora`. A tabela `MovimentacoesCartao` tem essas colunas; `Descricao` tem tamanho máximo 120 (`MovimentacaoCartaoConfiguracao`). Não há produto nem quantidade na entidade, no DTO, na migration nem no tipo do aplicativo.

`TelaMovimentacoes` mostra `descricao`, o valor com o prefixo “+ ” e `formatarDataHora` (`pt-BR`, data curta e hora curta). Os textos da tela falam em recargas: “Histórico simples das recargas deste cartão.” e “Nenhuma recarga ainda.”

Consequência: o registro que ACE-01 descreve não cabe nos campos atuais. Data e hora já existem na movimentação de recarga e já são apresentadas nesse formato; isso não decide o formato pedido em P3.1 para a compra. Colocar a compra na mesma lista, em outra lista, ou dentro de `Descricao` são possibilidades em aberto (P1.1, P1.2). Se a mesma lista fosse reutilizada sem mudar a tela, o valor da compra apareceria com “+ ”, como crédito. Isso é hipótese de impacto, condicionada a P1.1, e não é desenho aprovado. A quantidade gravada também não tem número definido (H4).

### I-06 — Débito e registro na mesma conclusão

| Campo | Conteúdo |
| --- | --- |
| Regra ou critério | RN04; ACE-03; ACE-08 |
| Grau | **Confirmado** o padrão da recarga. **Hipótese de impacto** reutilizar esse padrão na compra. **Lacuna** o comportamento de compra |

Evidência: `Cartao.Recarregar` soma o saldo e cria a movimentação no mesmo método. `CartaoServico.RecarregarAsync` chama `AdicionarMovimentacao` e um único `SalvarAlteracoesAsync`. `CartaoRepositorio.SalvarAlteracoesAsync` é um `SaveChangesAsync`. `AGENTS.md` exige esse par na recarga. Não há segundo `SaveChanges` no fluxo de recarga.

Consequência: o único par “saldo e registro juntos” do repositório é a recarga. ACE-03 pede o mesmo efeito para a compra, e esse efeito não está implementado. Copiar o padrão da recarga seria uma hipótese de mecanismo, não uma decisão desta análise. A proposta também não descreve como provocar a falha de ACE-08.

### I-07 — Saldo menor que o preço

| Campo | Conteúdo |
| --- | --- |
| Regra ou critério | RN03; ACE-05; ACE-06; CA03; pergunta P2.1 |
| Grau | **Confirmado** a ausência. **Decisão pendente** o texto e o canal |

Evidência: não há comparação entre `Cartao.Saldo` e `Produto.Preco`. Não há exceção de saldo insuficiente. `RecargaInvalidaException` diz “A recarga deve ter valor maior que zero.” `TratadorExcecoes` traduz essa exceção em HTTP 400, ProblemDetails, título “Requisição inválida”. `clienteHttp` mostra `detail` ou `title`. `TelaRecarga` mostra a mensagem da exceção na própria tela.

Consequência: ACE-06 pede informar saldo insuficiente e manter saldo e histórico sem compra concluída. Esse ramo não existe. A mensagem de recarga inválida não é a informação de saldo insuficiente. Reutilizar o 400 da recarga, ou mostrar a frase só no aplicativo, continua em P2.1 e na hipótese H6.

### I-08 — Cartão inexistente

| Campo | Conteúdo |
| --- | --- |
| Regra ou critério | RN03; ACE-07 cenário A; CA04; pergunta P2.4 |
| Grau | **Confirmado** o 404 de cartão nas rotas atuais. **Hipótese de impacto** tratá-lo como o “dado inválido” da proposta |

Evidência: `CartaoServico.ObterCartaoAsync` lança `CartaoNaoEncontradoException` quando `ObterPorIdAsync` devolve nulo. A mensagem é “Cartão {id} não foi encontrado.” `TratadorExcecoes` responde 404, título “Recurso não encontrado”. Isso vale para obter, recarregar e listar movimentações. Não há rota de compra que use esse ramo.

Consequência: um id de cartão ausente já produz 404 nas operações existentes, sem alterar saldo. ACE-07 pede informar que o dado é inválido, sem débito e sem compra concluída. A proposta usa a mesma informação para cartão e para produto. O texto atual distingue o cartão e inclui o id. Equiparar esse 404 a ACE-07 é hipótese, não aceite. Identificação vazia no aplicativo continua em P2.4.

### I-09 — Produto inexistente

| Campo | Conteúdo |
| --- | --- |
| Regra ou critério | RN03; ACE-07 cenário B; CA04 |
| Grau | **Lacuna** confirmada |

Evidência: `IProdutoRepositorio` declara só `ListarAsync`. `ProdutosController` declara só `GET /api/produtos`. Não há `ProdutoNaoEncontradoException` nem ramo correspondente em `TratadorExcecoes`. `ProdutoServico` não busca um produto por id.

Consequência: o cenário B de ACE-07 não tem caminho. A lista do catálogo não responde, por si, a um id que não está nela: não existe operação que receba esse id.

### I-10 — Falha ao efetivar débito ou registro

| Campo | Conteúdo |
| --- | --- |
| Regra ou critério | RN04; ACE-08; CA05; pergunta P1.4 |
| Grau | **Lacuna** confirmada no comportamento. **Decisão pendente** o rastro de tentativa |

Evidência: não há caso de uso de compra cujo débito ou registro possa falhar. Os testes de recarga inválida mostram saldo intacto e lista vazia porque `Recarregar` lança antes de alterar o cartão (`CartaoTestes.Recarregar_ValorIgualOuInferiorAZero_Rejeita`, `CartaoServicoTestes.RecarregarAsync_ValorInvalido_NaoAlteraSaldo`). Não há teste que force falha de `SaveChanges`.

Consequência: ACE-08 não é observável. A proposta não diz como provocar a falha e não decide se pode permanecer rastro que não seja compra concluída (P1.4). O teste em memória não prova esse critério: `CartaoRepositorioEmMemoria.AdicionarMovimentacao` está vazio, e `SalvarAlteracoesAsync` só incrementa um contador. O projeto de testes referencia `CardPlay.Repository`, e os testes localizados não usam `CardPlayDbContext`.

### I-11 — Saldo observado nas outras abas

| Campo | Conteúdo |
| --- | --- |
| Regra ou critério | ACE-01; ACE-04; pergunta P3.3 |
| Grau | **Confirmado** o fluxo de atualização. **Hipótese de impacto** a aba que precisa mudar depois da compra |

Evidência: `CartaoProvider.recarregar` grava o `Cartao` devolvido pela recarga e substitui `movimentacoes` com um novo `GET`. `TelaCartao` e `TelaMovimentacoes` leem esse contexto. `TelaCatalogo` guarda a própria lista de produtos e não lê o saldo. `CartaoVirtual` mostra o saldo do contexto.

Consequência: uma escrita que não passe por `CartaoProvider` deixa a aba Cartão e a aba Histórico com o estado anterior até a próxima carga. A proposta não diz para qual aba o cliente vai depois da compra (P3.3). O saldo debitado, para ser visto no cartão, depende dessa atualização. Isso descreve o acoplamento atual; não fixa a navegação.

### I-12 — Contrato HTTP

| Campo | Conteúdo |
| --- | --- |
| Regra ou critério | RN02; RN04; ACE-01; ACE-06; ACE-07; ACE-08 |
| Grau | **Confirmado** o contrato atual. **Lacuna** o contrato de compra. **Decisão pendente** estender a movimentação ou criar outra leitura |

Evidência, rotas em `CartoesController` e `ProdutosController`, cliente em `mobile/src/api/`:

| Método e rota | Entrada | Saída de sucesso | Erro já mapeado |
| --- | --- | --- | --- |
| `POST /api/cartoes` | `SolicitarCartaoRequisicao` (`NomeTitular`) | `201` `CartaoResposta` | `400` nome inválido |
| `GET /api/cartoes/{id}` | `Guid` | `200` `CartaoResposta` | `404` cartão ausente |
| `POST /api/cartoes/{id}/recargas` | `RecargaRequisicao` (`Valor`, `Descricao`) | `200` `CartaoResposta` | `400` valor `<= 0`; `404` cartão ausente |
| `GET /api/cartoes/{id}/movimentacoes` | `Guid` | `200` lista de `MovimentacaoResposta` | `404` cartão ausente |
| `GET /api/produtos` | nenhuma | `200` lista de `ProdutoResposta` | sem exceção de domínio própria |

`CartaoResposta`: `Id`, `NomeTitular`, `CodigoAmigavel`, `Saldo`, `DataCriacao`. `ProdutoResposta`: `Id`, `Nome`, `DescricaoCurta`, `Preco`, `Icone`, `Disponivel`. Exceção não mapeada cai em HTTP 500, título “Erro interno”, `detail` igual à mensagem da exceção (`TratadorExcecoes`).

Consequência: não há entrada de compra (cartão, produto, confirmação) nem saída de compra. `README.md` registra a mesma ausência. Acrescentar produto e quantidade em `MovimentacaoResposta` mudaria o corpo que `cartaoApi.listarMovimentacoes` já consome. Fazer isso, ou criar outra rota de leitura, depende de P1.1 e P1.2. Os nomes JSON em camelCase são o que o tipo do aplicativo espera; `Program.cs` não configura o nome JSON, e esta leitura não confirmou o corpo com uma chamada HTTP — a mesma lacuna já registrada no artefato 04.

### I-13 — Persistência

| Campo | Conteúdo |
| --- | --- |
| Regra ou critério | RN04; ACE-03; ACE-08 |
| Grau | **Confirmado** o esquema. **Decisão pendente** de arquitetura a forma de guardar a compra |

Evidência: `CardPlayDbContext` expõe `Cartoes`, `Movimentacoes` e `Produtos`. A migration `Inicial` cria essas três tabelas. `MovimentacoesCartao` referencia `Cartoes` com exclusão em cascata. `Produto` não referencia cartão nem movimentação. O arquivo `cardplay.db` não foi aberto; o formato vem da migration, como no artefato 04.

Consequência: gravar produto e quantidade como dados próprios não cabe nas colunas atuais. Uma migration nova seria o mecanismo já usado pelo repositório para mudar o esquema (`AGENTS.md`). Qual tabela ou quais colunas, e se a compra reutiliza `MovimentacoesCartao`, é escolha de arquitetura. Esta análise não a faz. A exclusão em cascata hoje apaga movimentações quando o cartão é removido; não há endpoint de remoção de cartão.

### I-14 — Disponibilidade do produto

| Campo | Conteúdo |
| --- | --- |
| Regra ou critério | Nenhuma em RN ou ACE. Campo existente no produto atual |
| Grau | **Lacuna** da proposta. Não é impacto obrigatório |

Evidência: `Produto.Disponivel` existe, vai para `ProdutoResposta` e para a coluna `Produtos.Disponivel`. Os seis registros do seed estão com `true`. `ProdutoRepositorio.ListarAsync` não filtra por esse campo. `CardProduto` não o exibe. RN03 exige produto existente, não produto disponível.

Consequência: usar `Disponivel` como trava de compra acrescentaria regra que a proposta não escreve. O campo permanece no catálogo atual e não entra em ACE-01–ACE-08.

### I-15 — Camada Services e integrações

| Campo | Conteúdo |
| --- | --- |
| Regra ou critério | Conflito K4; contexto C5 |
| Grau | **Confirmado** a camada vazia. **Decisão pendente** se a compra permanece só dentro do CardPlay |

Evidência: `backend/src/CardPlay.Services` contém o projeto e um `README.md`, sem classe C#. `Program.cs` não registra tipo desse projeto. `docs/architecture.md` reserva Services para API externa e coloca a orquestração na Application. A proposta não cita banco, gateway nem serviço externo. O produto atual os exclui. A CP-001 não os lista fora de escopo (K4).

Consequência: o código não tem cliente externo para a compra chamar. Incluir gateway ou autenticação seria ampliar o que a versão atual exclui e o que a proposta não especifica. Manter a compra só em Application, Domain e Repository segue a arquitetura já escrita para os casos de uso atuais; isso não fecha K4.

### I-16 — Testes existentes

| Campo | Conteúdo |
| --- | --- |
| Regra ou critério | ACE-01–ACE-08; regras de recarga do produto atual (C1, C6) |
| Grau | **Confirmado** |

Evidência em `backend/tests/CardPlay.Tests/CartaoTestes.cs`: criação com saldo zero, nome inválido, recarga `<= 0` sem mudar saldo nem movimentação, recarga válida atualizando saldo, movimentação com valor e descrição, descrição em branco virando “Recarga”.

Evidência em `CartaoServicoTestes.cs`: solicitar persiste saldo zero; recarga válida atualiza saldo e registra uma movimentação; recarga de valor zero não altera saldo. Não há teste de `Produto`, de controller, de EF, de tela, nem pasta de teste em `mobile/`. Nenhum teste nomeia compra, ACE ou saldo insuficiente.

Consequência: a suíte não observa ACE-01–ACE-08. Ela observa a recarga. Um teste novo de compra, no estilo do repositório em memória, não cobriria a falha de gravação no SQLite (I-10).

### I-17 — Documentos que fixam a ausência de compra

| Campo | Conteúdo |
| --- | --- |
| Regra ou critério | Conflitos K2, K3, K5; pergunta P1.5 |
| Grau | **Confirmado** o conflito. **Decisão pendente** o vínculo com o backlog |

Evidência:

| Documento | O que afirma hoje |
| --- | --- |
| `docs/backlog.md` | “Utilizar saldo em produtos”, uma frase, sem regras |
| `docs/product.md` | Compra fora desta versão; histórico de recargas; botão desabilitado |
| `docs/ui.md` | Botão “Usar cartão” visível, desabilitado, “Em breve”; não implementar compra |
| `docs/architecture.md` | Movimentação desta versão é recarga |
| `README.md` | Sem compra; sem endpoint de compra |
| `AGENTS.md` | Não implementar compra enquanto estiver só no backlog |

Consequência: a proposta CP-001 descreve comportamento que esses documentos excluem. Seguir a proposta sem decisão sobre o backlog deixa o repositório com duas leituras (K2, P1.5). Este arquivo não altera esses documentos.

### I-18 — Fora de escopo escrito na proposta

| Campo | Conteúdo |
| --- | --- |
| Regra ou critério | F-H8; carrinho, múltiplas unidades, cancelamento, estorno |
| Grau | **Confirmado** a ausência no código. **Lacuna** de critério se alguém tentar esses caminhos |

Evidência: não há entidade, rota, tela ou teste de carrinho, quantidade maior que um, cancelamento ou estorno. A busca por compra no código de produção não acha caso de uso.

Consequência: não há componente desses assuntos para modificar. A proposta os exclui e não descreve a recusa. O artefato 03 já registrou que não há critério para essa tentativa. Isso permanece lacuna, não impacto de mudança.

### I-19 — Concorrência de duas confirmações

| Campo | Conteúdo |
| --- | --- |
| Regra ou critério | Nenhuma em RN ou ACE |
| Grau | **Hipótese de impacto**. Não vira requisito |

Evidência: `CartaoConfiguracao` não declara token de concorrência. `RecarregarAsync` lê o cartão, altera o objeto e chama um `SaveChanges`. A proposta não fala de duas confirmações ao mesmo tempo.

Consequência: o fluxo atual de escrita não registra proteção contra duas alterações simultâneas do mesmo saldo. Tratar isso como parte da CP-001 seria acrescentar regra. Fica só como risco técnico observado, à espera de alguém decidir se entra no escopo.

---

## Dados lidos e alterados

| Dado | Onde está | Leitura atual | Escrita atual | Relação com a proposta |
| --- | --- | --- | --- | --- |
| Saldo | `Cartao.Saldo`; `CartaoResposta.Saldo`; estado do `CartaoProvider` | Obter cartão, recarga, cartão virtual | Só aumenta em `Recarregar`, ou nasce zero | ACE-01, ACE-04 e ACE-06 exigem débito ou saldo intacto. Débito ausente (I-04) |
| Preço | `Produto.Preco`; seed; `ProdutoResposta`; card | `GET /api/produtos` | Seed na migration | RN01 e ACE-02. Sem leitura para debitar (I-03) |
| Produto (nome, id) | `Produto`; lista do catálogo | Listagem | Seed | ACE-01 pede o produto no registro. O registro não tem esse campo (I-05) |
| Quantidade | — | — | — | ACE-01 pede no registro. Ausente (I-05, H4) |
| Valor da movimentação | `MovimentacaoCartao.Valor` | Histórico | Recarga, valor positivo | Compra pediria o preço debitado. O sinal “+” é só da tela (I-05) |
| Descrição | `MovimentacaoCartao.Descricao`, máximo 120 | Histórico | “Recarga” ou texto da requisição | Não é produto nem quantidade |
| Data e hora | `MovimentacaoCartao.DataHora`, UTC no domínio | Histórico, `formatarDataHora` | `DateTime.UtcNow` na criação | RN04 pede data e hora. O formato ao cliente da compra está em P3.1 |
| Id do cartão | Rota, AsyncStorage `@cardplay/cartaoId` | Abertura do app e recarga | Gravado ao solicitar | RN02. Equivalência com “identificar” em aberto (I-02) |
| Disponibilidade | `Produto.Disponivel` | Vai no DTO; a tela ignora | Seed `true` | Fora de RN/ACE (I-14) |

---

## O que a proposta não reescreve e o código já garante

Preservar estes comportamentos evita regressão da versão atual. A CP-001 não os redefine.

| Comportamento | Evidência | Por que permanece |
| --- | --- | --- |
| Solicitar cartão com nome de 2 a 80 caracteres e saldo zero | `Cartao.Solicitar`; `CartaoTestes` | Fora do texto da CP-001 (C6) |
| Recarga com valor `<= 0` rejeitada, saldo e movimentações intactos | `Cartao.Recarregar`; testes de domínio e de serviço | A proposta não altera recarga |
| Recarga válida soma o saldo e grava uma movimentação no mesmo `SaveChanges` | `CartaoServico.RecarregarAsync` | Regra atual de `AGENTS.md`; é recarga, não compra (I-06) |
| Descrição em branco vira “Recarga” | `Cartao.Recarregar`; `Recarregar_SemDescricao_UsaDescricaoPadrao` | A proposta não fala da descrição de recarga |
| Histórico de recargas ordenado da mais nova para a mais antiga | `CartaoServico.ListarMovimentacoesAsync` | A proposta não manda reordenar recargas |
| Catálogo: uma coluna abaixo de 720 px, duas a partir disso | `TelaCatalogo`, `LARGURA_DUAS_COLUNAS` | Fora de RN/ACE |
| Cores só em `mobile/src/tema/cores.ts`; barra com ícone e rótulo | `NavegacaoAbas`; `docs/ui.md` | Fora de RN/ACE |
| URL da API só em `mobile/src/config/ambiente.ts` | `clienteHttp` usa `URL_API` | Fora de RN/ACE |
| Cartão interno, sem PAN, CVV ou gateway no código | Entidades e Services vazio | C5. K4 ainda não decide se a história passa a incluir isso |
| Quatro abas: Cartão, Recarga, Histórico, Catálogo | `NavegacaoAbas` | A proposta não pede aba nova |

Risco de regressão **confirmado** se uma implementação futura mexer nesses pontos para “caber” a compra: os testes de `CartaoTestes` e `CartaoServicoTestes` deixam de descrever a recarga, ou a tela de histórico passa a tratar toda movimentação como crédito (I-05), ou o botão do catálogo muda antes da decisão P3.2 (I-01).

Risco **hipotético**, dependente de decisão ainda aberta: saldo velho na aba Cartão se a escrita não atualizar o `CartaoProvider` (I-11); duas confirmações no mesmo saldo (I-19); `number` do TypeScript no lugar de `decimal` para o valor enviado. O aplicativo já usa `number` em `Cartao.saldo`, `Movimentacao.valor` e `Produto.preco`. A proposta não fala dessa diferença.

---

## Cruzamento com o artefato 04

Confirmado de novo, sem divergência de fato:

- Não há método, rota nem função de compra.
- O botão do catálogo está desabilitado e o rótulo em `.tsx` é “Usar cartão”, não “Comprar”.
- `MovimentacaoCartao` não tem produto nem quantidade.
- A recarga junta saldo e movimentação num `SaveChanges`.
- Os testes localizados não abrem SQLite.
- `CardPlay.Services` não tem cliente.
- As decisões dos artefatos 01, 02 e 03 continuam abertas.

Fatos que o artefato 04 cita e que esta análise usa como impacto, porque mudam o caminho da proposta:

| Fato | Onde | Impacto |
| --- | --- | --- |
| `CartaoServico` não depende de produto | construtor de `CartaoServico` | I-04, I-09 |
| `IProdutoRepositorio` não busca por id | interface | I-09 |
| `TratadorExcecoes` só conhece recarga inválida, nome inválido e cartão ausente | `TryHandleAsync` | I-07, I-08, I-12 |
| Seed sem produto de R$ 20,00 | `ProdutoConfiguracao` | I-03 |
| Catálogo desligado do contexto do cartão | `TelaCatalogo` | I-02, I-11 |
| `Pressable` sem ação | `CardProduto` | I-01 |
| `AdicionarMovimentacao` do teste em memória é vazio | `CartaoServicoTestes` | I-10 |
| `Disponivel` não entra na tela | `CardProduto` | I-14 |
| `Descricao` limitada a 120 | `MovimentacaoCartaoConfiguracao` | I-05 |
| Data e hora da recarga já formatadas em `pt-BR` | `formatarDataHora` | I-05; não fecha P3.1 |
| Saldo sem restrição de não negativo no esquema | migration `Inicial` | I-04 |

Nada disso foi promovido a regra nova.

---

## Decisões pendentes para revisão humana

Nenhuma foi fechada aqui. O decisor de negócio não está nomeado nas fontes. Onde a pendência é de arquitetura, também não há escolha.

### Negócio, já abertas nos artefatos 01–03

| ID | Decisão | Impactos que ficam incompletos |
| --- | --- | --- |
| K3 | Aprovar RN01–RN04 e ACE-01–ACE-08, ou registrar o que muda | Todos. Enquanto isso, o impacto é sobre a proposta |
| P1.5 / K2 | A CP-001 refina, substitui ou convive com a frase de `docs/backlog.md` | I-17. `AGENTS.md` ainda trata compra como item só de backlog |
| P1.1 / K1 | O registro da compra entra na mesma lista das recargas ou em outro histórico | I-05, I-12, I-13 |
| P1.2 | O que o cliente vê para produto, quantidade, valor, data e hora | I-05 |
| P1.3 | O que é “compra concluída” além de saldo debitado e registro | I-11, e o artefato de sucesso de ACE-01 e ACE-04 |
| P1.4 | Se a falha pode deixar rastro que não seja compra concluída | I-10 |
| P2.1 | Intenção e texto de saldo insuficiente, dado inválido e compra não concluída, e o canal | I-07, I-08, I-12 |
| P2.2 | Confirmação em passo de resumo ou gesto depois de ver o preço | I-01, ACE-02 |
| P2.3 | Qual preço vale se o cadastrado mudar entre a visualização e a confirmação | I-03 |
| P2.4 | Tentativa sem cartão selecionado, ou com id local inválido, conta como dado inválido | I-02, I-08 |
| P2.5 | Sinal, rótulo e distinção do valor gasto em relação à recarga | I-05 |
| P3.1 | Formato de data e hora apresentado ao cliente | I-05. `formatarDataHora` só descreve a recarga |
| P3.2 / K5 | Como a compra passa a ser oferecida, inclusive o botão desabilitado | I-01 |
| P3.3 | Para onde o cliente vai depois da compra | I-11 |
| H4 | Se a quantidade gravada é sempre 1 | I-05. ACE-01 exige uma unidade e exige o campo; não fixa o número gravado |
| K4 | Se autenticação, estoque, administração de catálogo, checkout e integração bancária ficam fora desta história | I-14, I-15, I-18 |

### Arquitetura, aberta porque o código não contém a compra e a proposta não escolhe o mecanismo

| Pendência | Por que não dá para inferir do repositório | Impactos |
| --- | --- | --- |
| Onde o caso de uso vive e qual contrato HTTP ele usa | Há serviços e rotas de cartão e de produto. Não há operação que receba os dois. Inventar rota, DTO ou método seria desenho | I-09, I-12 |
| Como persistir produto e quantidade | As colunas atuais não os têm. Nova coluna, nova entidade ou texto em `Descricao` são modelos diferentes | I-05, I-13 |
| Se o 404 e o 400 já existentes servem para ACE-07 e ACE-06 | As mensagens atuais falam de cartão não encontrado e de recarga. A proposta fala de dado inválido e de saldo insuficiente | I-07, I-08 |
| Se `Disponivel` participa da compra | O campo existe. Nenhuma RN o cita | I-14 |
| Se concorrência de saldo entra na história | O código não tem token. A proposta não fala no assunto | I-19 |

Carrinho, múltiplas unidades, cancelamento e estorno continuam fora da proposta, sem critério de recusa (I-18). Hipóteses H1, H2, H3, H5 e H6 continuam não aprovadas, como no artefato 03.
