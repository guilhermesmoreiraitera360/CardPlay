# Arquitetura atual do CardPlay

Base de consulta da organização **já existente** no repositório. Descreve o que o código e a documentação do produto confirmam. Não define a compra da CP-001.

Fontes lidas: `README.md`, `AGENTS.md`, `docs/architecture.md`, os artefatos `01-entendimento.md`, `02-historia-rica.md` e `03-criterios-de-aceite.md`, e os arquivos de código citados abaixo. Os testes foram localizados; não foram executados.

Legenda: **Fato** (visto no código ou na documentação citada) · **Hipótese** (leitura possível, não decidida) · **Lacuna** (arquivo, comportamento ou decisão ausente).

---

## 1. Visão geral do sistema

**Fato.** CardPlay é um monorepositório didático. A pessoa solicita um cartão virtual interno, vê o saldo, faz uma recarga simulada, consulta o histórico dessas recargas e vê um catálogo gravado por seed. O cartão não é bancário: não há PAN, CVV, banco nem gateway (`README.md`).

O caminho de uma ação até o banco, como `docs/architecture.md` descreve e o código confirma, é:

```text
aplicativo (Expo / React Native)
        ↓  HTTP
CardPlay.Api
        ↓
CardPlay.Application   (orquestra o caso de uso)
        ↓
CardPlay.Domain         (regras e entidades)
        ↓
CardPlay.Repository     (EF Core)
        ↓
SQLite (arquivo cardplay.db)
```

`CardPlay.Services` existe no solution para um lugar de APIs externas. Nesta versão não há cliente externo implementado.

A compra de produto **não** é parte deste mapa. Não há método de compra no domínio, nem rota de compra na API, nem chamada de compra no aplicativo. No catálogo, o botão permanece visível e desabilitado (`mobile/src/componentes/CardProduto.tsx`).

---

## 2. Partes, tecnologias e responsabilidades

Tecnologias confirmadas nos projetos:

| Parte | Onde | Tecnologia confirmada |
| --- | --- | --- |
| Aplicativo | `mobile/` | Expo ~57, React Native 0.86, TypeScript, React Navigation (abas) |
| API | `backend/src/CardPlay.Api` | ASP.NET Core, `net10.0`, OpenAPI, Scalar, ProblemDetails |
| Casos de uso | `backend/src/CardPlay.Application` | biblioteca .NET, sem EF e sem HTTP |
| Regras | `backend/src/CardPlay.Domain` | biblioteca .NET, sem ASP.NET, EF nem frontend |
| Persistência | `backend/src/CardPlay.Repository` | EF Core 10 + SQLite |
| Externos | `backend/src/CardPlay.Services` | projeto .NET vazio de código; só `README.md` |
| Testes | `backend/tests/CardPlay.Tests` | xUnit |

Referências de compilação, iguais em `docs/architecture.md` e nos `.csproj`:

```text
CardPlay.Api         → Application, Repository, Services, Domain
CardPlay.Repository  → Application, Domain
CardPlay.Services    → Application, Domain
CardPlay.Application → Domain
CardPlay.Domain      → (nenhuma)
CardPlay.Tests       → Domain, Application, Repository
```

O aplicativo não referencia esses projetos. Fala com a API por HTTP.

### Aplicativo

Telas em `mobile/src/telas/`, montadas em `NavegacaoAbas`: Cartão, Recarga, Histórico (`TelaMovimentacoes`) e Catálogo.

Responsabilidade: mostrar dados e disparar ações. Não calcula saldo nem grava banco. O cliente HTTP fica só em `mobile/src/api/`. A URL da API sai de `EXPO_PUBLIC_API_URL` em `mobile/src/config/ambiente.ts`. O id do cartão escolhido fica no AsyncStorage (`mobile/src/armazenamento/cartaoSelecionado.ts`, chave `@cardplay/cartaoId`). O estado do cartão e das movimentações fica em `CartaoProvider` (`mobile/src/estado/CartaoContexto.tsx`). O catálogo é carregado pela própria `TelaCatalogo`, sem entrar nesse contexto.

### API

`CardPlay.Api` é o composition root. `Program.cs` registra o `CardPlayDbContext` (SQLite, connection string `CardPlay`), os repositórios, `CartaoServico` e `ProdutoServico`, ProblemDetails e o `TratadorExcecoes`. Na subida chama `Database.Migrate()`.

Controllers pequenos: `CartoesController` e `ProdutosController` recebem o pedido, chamam a interface da Application e devolvem DTO. Não contêm a regra de saldo.

### Application

Orquestra os casos de uso. Contratos:

- `ICartaoServico` / `CartaoServico`: solicitar, obter, recarregar, listar movimentações.
- `IProdutoServico` / `ProdutoServico`: listar produtos.
- `ICartaoRepositorio` e `IProdutoRepositorio`: o que a persistência precisa oferecer. As interfaces ficam aqui; as classes concretas ficam no Repository.

DTOs de resposta: `CartaoResposta`, `MovimentacaoResposta`, `ProdutoResposta`. A API não devolve a entidade do EF.

### Domain

Entidades e regras essenciais.

- `Cartao`: `Id`, `NomeTitular`, `CodigoAmigavel` (`CP-` + 6 caracteres), `Saldo` (`decimal`), `DataCriacao` (UTC). `Solicitar` cria o cartão com saldo zero e rejeita nome com menos de 2 ou mais de 80 caracteres. `Recarregar` rejeita valor `<= 0`; se o valor é válido, soma o saldo e cria a movimentação no mesmo método.
- `MovimentacaoCartao`: `Id`, `CartaoId`, `Valor`, `Descricao`, `DataHora` (UTC). Não há produto nem quantidade.
- `Produto`: `Nome`, `DescricaoCurta`, `Preco`, `Icone`, `Disponivel`. Não há método de venda.

### Repository

`CardPlayDbContext` expõe `Cartoes`, `Movimentacoes` e `Produtos`. Tabelas da migration `Inicial`: `Cartoes`, `MovimentacoesCartao` (chave estrangeira para `Cartoes`, exclusão em cascata) e `Produtos`.

`CartaoRepositorio.SalvarAlteracoesAsync` chama um `SaveChangesAsync`. Na recarga, `CartaoServico` altera o saldo, pede para adicionar a movimentação e só então salva. Os dois vão no mesmo `SaveChanges`.

O catálogo inicial entra por seed em `ProdutoConfiguracao` (seis produtos, por exemplo “Café especial” a 8,50 e “Livro ilustrado” a 32,90). Não há tela de administração.

A connection string em `appsettings.json` é `Data Source=cardplay.db`. O arquivo do banco não entra no Git.

### Services

**Fato.** O projeto está no solution e a API o referencia. Não há classe C# dentro dele. `Program.cs` não registra nenhum tipo de `CardPlay.Services`. O `README.md` dessa pasta diz que a camada seria para clientes externos e que, nesta versão, a orquestração fica na Application.

---

## 3. Conexões e fluxo existente

O fluxo já percorrido de ponta a ponta é a **recarga**. A compra não entra nele.

1. `TelaRecarga` lê o valor e chama `recarregar` do `CartaoProvider`.
2. O contexto exige um cartão já carregado e chama `cartaoApi.recarregar`.
3. `cartaoApi` faz `POST /api/cartoes/{id}/recargas` com `{ valor, descricao }`, via `clienteHttp` (`fetch`).
4. `CartoesController.Recarregar` chama `ICartaoServico.RecarregarAsync`.
5. `CartaoServico` busca o cartão em `ICartaoRepositorio`. Se não existe, lança `CartaoNaoEncontradoException` (a API responde 404).
6. `Cartao.Recarregar` aplica a regra. Valor `<= 0` lança `RecargaInvalidaException` (a API responde 400). Valor válido soma `Saldo` e cria `MovimentacaoCartao`.
7. O serviço pede `AdicionarMovimentacao` e `SalvarAlteracoesAsync`. O repositório grava cartão e movimentação no mesmo `SaveChanges`.
8. A API devolve `CartaoResposta`. O aplicativo busca de novo `GET /api/cartoes/{id}/movimentacoes` e atualiza a lista da aba Histórico.

A aba Histórico não chama a API sozinha. Ela lê `movimentacoes` do contexto, preenchido ao abrir o app, ao solicitar cartão e depois da recarga. Cada item mostra `descricao`, valor com sinal `+` e data/hora (`TelaMovimentacoes`).

Outros caminhos já ligados do mesmo jeito:

| Ação na interface | Cliente | Rota | Caso de uso | Persistência |
| --- | --- | --- | --- | --- |
| Solicitar cartão | `cartaoApi.solicitar` | `POST /api/cartoes` | `CartaoServico.SolicitarAsync` | `AdicionarAsync` + `SaveChanges` |
| Ver cartão | `cartaoApi.obter` | `GET /api/cartoes/{id}` | `ObterAsync` | `ObterPorIdAsync` (inclui movimentações) |
| Histórico | `cartaoApi.listarMovimentacoes` | `GET /api/cartoes/{id}/movimentacoes` | `ListarMovimentacoesAsync` | lista já carregada com o cartão, ordenada da mais nova para a mais antiga |
| Catálogo | `produtoApi.listar` | `GET /api/produtos` | `ProdutoServico.ListarAsync` | `ProdutoRepositorio.ListarAsync` (`AsNoTracking`, ordem por nome) |

O catálogo não passa pelo `CartaoProvider`. `TelaCatalogo` chama `produtoApi` direto. Abaixo de 720 px mostra um produto por linha; a partir disso, duas colunas.

Não há login. Vários cartões podem existir na API. O aplicativo guarda um id local e o reutiliza ao abrir.

---

## 4. Arquivos, contratos e testes de referência

### Cartão, saldo e movimentações

| Papel | Caminho | Símbolo |
| --- | --- | --- |
| Regra de saldo | `backend/src/CardPlay.Domain/Entidades/Cartao.cs` | `Solicitar`, `Recarregar` |
| Registro da operação | `backend/src/CardPlay.Domain/Entidades/MovimentacaoCartao.cs` | `Valor`, `Descricao`, `DataHora` |
| Caso de uso | `backend/src/CardPlay.Application/Servicos/CartaoServico.cs` | `RecarregarAsync` |
| Contrato HTTP interno | `backend/src/CardPlay.Application/Contratos/Servicos/ICartaoServico.cs` | quatro operações, sem compra |
| Contrato de persistência | `backend/src/CardPlay.Application/Contratos/Repositorios/ICartaoRepositorio.cs` | `AdicionarMovimentacao`, `SalvarAlteracoesAsync` |
| HTTP | `backend/src/CardPlay.Api/Controllers/CartoesController.cs` | rotas `/api/cartoes` |
| Erros | `backend/src/CardPlay.Api/Tratamento/TratadorExcecoes.cs` | 400 para recarga ou nome inválido; 404 para cartão ausente |
| Tela e estado | `mobile/src/telas/TelaRecarga.tsx`, `mobile/src/estado/CartaoContexto.tsx` | `recarregar` |
| Tipos da interface | `mobile/src/tipos/index.ts` | `Cartao`, `Movimentacao` |

`MovimentacaoResposta` expõe `Id`, `Valor`, `Descricao` e `DataHora`. O mesmo formato está no tipo `Movimentacao` do aplicativo.

### Catálogo

| Papel | Caminho | Símbolo |
| --- | --- | --- |
| Entidade | `backend/src/CardPlay.Domain/Entidades/Produto.cs` | `Preco`, `Disponivel` |
| Caso de uso | `backend/src/CardPlay.Application/Servicos/ProdutoServico.cs` | `ListarAsync` |
| HTTP | `backend/src/CardPlay.Api/Controllers/ProdutosController.cs` | `GET /api/produtos` |
| Seed | `backend/src/CardPlay.Repository/Configuracoes/ProdutoConfiguracao.cs` | `HasData` |
| Tela | `mobile/src/telas/TelaCatalogo.tsx` | texto de que a compra não está disponível |
| Botão futuro | `mobile/src/componentes/CardProduto.tsx` | “Usar cartão” e “Em breve”, `disabled` |

### Padrões que organizam esse desenho

- Caso de uso na Application; controller só encaminha.
- Repositório específico (`CartaoRepositorio`, `ProdutoRepositorio`), não um repositório genérico.
- Valores de dinheiro em `decimal`. Datas de criação e de movimentação em UTC no domínio.
- Resposta de erro no formato ProblemDetails (`title`, `detail`). O `clienteHttp` lê `detail` ou `title`.
- Cores só em `mobile/src/tema/cores.ts`. Azul na navegação; laranja no saldo, no preço e na recarga (`docs/ui.md`).

### Testes localizados (não executados)

`backend/tests/CardPlay.Tests/CartaoTestes.cs` (domínio, sem banco):

- `Solicitar_ComNomeValido_CriaCartaoComSaldoZero`
- `Solicitar_ComNomeInvalido_Rejeita`
- `Recarregar_ValorIgualOuInferiorAZero_Rejeita`
- `Recarregar_ValorValido_AtualizaSaldo`
- `Recarregar_ValorValido_RegistraMovimentacao`
- `Recarregar_SemDescricao_UsaDescricaoPadrao`

`backend/tests/CardPlay.Tests/CartaoServicoTestes.cs` (serviço com `CartaoRepositorioEmMemoria`, não com SQLite):

- `SolicitarAsync_PersisteCartaoComSaldoZero`
- `RecarregarAsync_ValorValido_AtualizaSaldoERegistraMovimentacao`
- `RecarregarAsync_ValorInvalido_NaoAlteraSaldo`

Não há teste de `Produto`, de controller, de EF nem de tela. O projeto de testes referencia `CardPlay.Repository`, mas os arquivos de teste localizados não usam `CardPlayDbContext`.

---

## 5. Relação com os artefatos 01, 02 e 03

Os artefatos 01 e 02 separam o produto atual da proposta CP-001. O artefato 03 transforma RN/CA em critérios observáveis e os marca como **proposta**, não como aceite oficial. Este mapa só mostra onde cada assunto já aparece no sistema.

| Assunto dos artefatos | Onde está hoje | O que ainda é só proposta |
| --- | --- | --- |
| Recarregar, ver saldo, ver catálogo (C1, C2, C4 no 02) | Fluxo da seção 3. **Fato** no código | — |
| Consultar histórico (C3) | `GET /api/cartoes/{id}/movimentacoes` e a aba Histórico. O texto da tela fala em recargas | A proposta fala em registro de compra. Isso não existe |
| Cartão interno, sem login, id guardado no app (C5, C7) | `Cartao`, AsyncStorage, ausência de autenticação | “Identificar o cartão” (RN02) não tem outro mecanismo além desse id local |
| Botão desabilitado (C8, conflito K5) | `CardProduto`: “Usar cartão” / “Em breve” | A proposta não diz como a compra passa a ser oferecida |
| Preço do catálogo (RN01) | Campo `Produto.Preco`, vindo do seed, exibido no card | Não há leitura desse preço para debitar |
| Saldo que não fica negativo (RN03, ACE-05) | A recarga só soma. O saldo nasce em zero e a recarga inválida não altera | Não há débito. O saldo não é reduzido em lugar nenhum do código |
| Débito e registro juntos (RN04, ACE-03) | Na **recarga**, saldo e movimentação nascem juntos em `Cartao.Recarregar` e seguem no mesmo `SaveChanges` | Esse par é recarga, não compra. Não é implementação da CP-001 |
| Conteúdo do registro (RN04, ACE-01, conflito K1) | `Valor`, `Descricao`, `DataHora`. A interface acrescenta o sinal `+` | Produto e quantidade não são campos |
| ACE-01 a ACE-08 | Não há teste, rota nem tela com esses nomes | Continuam proposta no artefato 03 |

**Fato.** Nada em `CartaoServico`, `CartoesController` ou `produtoApi` conclui uma compra. Os critérios ACE-01–ACE-08 não são comportamento atual.

**Hipótese não usada como arquitetura.** O artefato 01 lista H1–H6 (mesmo histórico, preço na confirmação, cartão já selecionado, quantidade 1, ausência de pedido, mensagens no app). Nenhuma delas foi transformada em tipo, rota ou tela.

---

## 6. Lacunas e pontos a confirmar

### Divergências entre documentação e código

- `docs/product.md` e `AGENTS.md` citam o botão “Usar cartão” / “Comprar”. No código localizado o rótulo é só “Usar cartão”, com “Em breve”. Não foi encontrado o texto “Comprar” em `.tsx`.
- `docs/architecture.md` desenha `CardPlay.Services` ligado a APIs externas. O projeto não contém cliente. A API referencia o projeto, mas não registra serviço nenhum dele.
- O `README.md` de `CardPlay.Services` diz que Repository e Services implementam contratos da Application. O Repository faz isso. Em Services não há implementação.
- `AGENTS.md` espera que uma recarga persista saldo e movimentação no mesmo `SaveChanges`. O código de `CartaoServico` e `CartaoRepositorio` faz isso. Os testes localizados cobrem a regra e um repositório em memória; não foi localizado teste que abra o SQLite.

### Lacunas de evidência

- Não há classe de compra, endpoint de compra nem função de compra no `mobile/src/api/`.
- `MovimentacaoCartao` não tem produto nem quantidade.
- Não há autenticação, estoque, carrinho nem checkout no código percorrido.
- Não há teste automatizado de catálogo, de API HTTP nem de tela. Não há pasta de testes em `mobile/`.
- Os DTOs C# usam PascalCase (`NomeTitular`) e os tipos do app usam camelCase (`nomeTitular`). `Program.cs` não configura o nome JSON. O alinhamento depende do padrão do ASP.NET Core; isso não foi confirmado com uma chamada HTTP nesta leitura.
- O arquivo `cardplay.db` não foi aberto. O formato das tabelas vem da migration `20260917150057_Inicial`, não de uma inspeção do banco em execução.
- Decisões de negócio da CP-001 continuam abertas nos artefatos 01, 02 e 03 (histórico da compra, o que é “concluída”, mensagens, vínculo com o backlog). Este arquivo não as fecha.

### Arquivos da pasta da demanda

Presentes e usados: `01-entendimento.md`, `02-historia-rica.md`, `03-criterios-de-aceite.md` e o PDF `CardPlay_Historia_de_Usuario_Visual.pdf` (lido por meio dos artefatos 01–03, que o citam). `Unconfirmed 642000.crdownload` permanece download incompleto; o artefato 01 já o descartou. Não foi inventado conteúdo para arquivo ausente.
