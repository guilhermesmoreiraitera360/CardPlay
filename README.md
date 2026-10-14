# CardPlay

**Aluno, comece por aqui:** [COMECEPORAQUI.md](COMECEPORAQUI.md) — instalação do SDK, Node, Git, pacotes e um prompt para a IA preparar a máquina.

Projeto-base do curso **Desenvolvimento de Software com IA**.

CardPlay é uma plataforma lúdica pequena: a pessoa solicita um cartão virtual interno, adiciona saldo com uma recarga simulada, consulta um catálogo e compra um produto com esse saldo. O cartão **não** é um cartão bancário. Não há PAN, CVV, banco nem gateway.

O objetivo não é uma arquitetura impressionante. É um repositório simples, compreensível e adequado para ensinar o ciclo:

**Entender → Planejar → Desenvolver → Validar → Entregar → Evoluir**

O prompt que originou este estado inicial está em [docs/exemplo-prompt-criacao.md](docs/exemplo-prompt-criacao.md).

## O que existe nesta versão

- Solicitar um cartão informando o nome
- Visualizar titular, código amigável e saldo
- Recarregar saldo (valor maior que zero)
- Consultar o histórico de recargas e de compras
- Ver produtos cadastrados por seed
- Comprar um produto com o saldo, confirmando o nome e o preço. Sem cartão, o botão **Usar cartão** fica visível e desabilitado

## Arquitetura resumida

```text
React Native
     ↓
ASP.NET Core API
     ↓
Application
     ↓
Domain
     ↙                    ↘
Repository / EF Core      Services / APIs externas
     ↓
SQLite
```

Detalhes e dependências reais entre projetos: [docs/architecture.md](docs/architecture.md).  
Como trabalhar no código (humanos e agentes): [AGENTS.md](AGENTS.md).  
Comportamento atual do produto: [docs/product.md](docs/product.md).  
Cores e usabilidade: [docs/ui.md](docs/ui.md).  
Ambiente de desenvolvimento: [COMECEPORAQUI.md](COMECEPORAQUI.md).

## Estrutura do repositório

```text
/
  backend/
    CardPlay.sln
    src/CardPlay.Api|Application|Domain|Repository|Services
    tests/CardPlay.Tests
  mobile/          # Expo + TypeScript
  docs/            # architecture, product, ui, backlog, prompt
  COMECEPORAQUI.md # primeiro material do aluno (instalação)
  README.md
  AGENTS.md
```

## Pré-requisitos

Instalação completa, checagem de versões e prompt para a IA configurar a máquina: [COMECEPORAQUI.md](COMECEPORAQUI.md).

- SDK .NET 10 (LTS). Este repositório foi criado com `10.0.100`.
- Node.js 20+ (o ambiente de criação usou v24)
- Git
- Expo Go no celular, ou um emulador Android / simulador iOS (no Windows, Expo web também serve)

Ferramenta local do Entity Framework:

```bash
dotnet tool restore --tool-manifest dotnet-tools.json
```

## Como executar o backend

Na raiz do repositório:

```bash
dotnet restore backend/CardPlay.sln
dotnet run --project backend/src/CardPlay.Api --urls http://localhost:5080
```

Na primeira execução a API aplica a migration e cria `cardplay.db` na pasta do projeto da API, com os produtos de seed.

- API: http://localhost:5080
- OpenAPI: http://localhost:5080/openapi/v1.json
- Scalar (documentação interativa): http://localhost:5080/scalar

Para o celular físico enxergar a API na sua máquina:

```bash
dotnet run --project backend/src/CardPlay.Api --urls http://0.0.0.0:5080
```

Libere a porta 5080 no firewall do Windows, se necessário.

### Banco e migrations

A API chama `Database.Migrate()` ao subir. Para gerar uma nova migration:

```bash
dotnet ef migrations add NomeDaMigration --project backend/src/CardPlay.Repository --startup-project backend/src/CardPlay.Api --output-dir Migrations
```

Para aplicar sem subir a API:

```bash
dotnet ef database update --project backend/src/CardPlay.Repository --startup-project backend/src/CardPlay.Api
```

## Como executar o mobile

```bash
cd mobile
cp .env.example .env
npm install
npx expo start
```

No Windows, se `cp` não existir:

```powershell
Copy-Item .env.example .env
```

Ajuste `EXPO_PUBLIC_API_URL` em `mobile/.env`:

| Onde o app roda | URL típica |
| --- | --- |
| Expo web / ferramentas no PC | `http://localhost:5080` |
| Emulador Android | `http://10.0.2.2:5080` |
| Simulador iOS | `http://localhost:5080` |
| Dispositivo físico | `http://IP_DA_SUA_MAQUINA:5080` |

No Windows, sem emulador, `npx expo start --web` também funciona com `http://localhost:5080`.

Se `EXPO_PUBLIC_API_URL` não for definida, o app usa `http://10.0.2.2:5080` no Android e `http://localhost:5080` nos demais.

A URL da API vive em `mobile/src/config/ambiente.ts`. Não espalhe endereços pelos componentes.

## Visual e usabilidade

Azul identifica o produto (cartão, abas). Laranja identifica dinheiro e ação (saldo, recarga e compra). A paleta fica em `mobile/src/tema/cores.ts`.

A barra inferior mostra ícone e rótulo acima da área segura. No celular, o catálogo lista um produto por linha; em telas largas usa duas colunas.

Regras completas: [docs/ui.md](docs/ui.md).

## Testes

```bash
dotnet test backend/CardPlay.sln
```

Os testes cobrem criação válida de cartão, recarga inválida, atualização de saldo, registro da movimentação, compra que debita o preço, saldo igual ao preço e saldo insuficiente sem débito.

## Endpoints

| Método | Rota | Função |
| --- | --- | --- |
| POST | `/api/cartoes` | Solicitar cartão |
| GET | `/api/cartoes/{id}` | Consultar cartão |
| POST | `/api/cartoes/{id}/recargas` | Recarga |
| GET | `/api/cartoes/{id}/movimentacoes` | Histórico de recargas e compras |
| POST | `/api/cartoes/{id}/compras` | Comprar um produto com o saldo |
| GET | `/api/produtos` | Catálogo |
