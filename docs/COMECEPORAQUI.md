# Comece por aqui

Este é o **primeiro material** do CardPlay. Leia este arquivo antes de qualquer outro.

O objetivo desta página é deixar o seu computador pronto para abrir, compilar e executar o backend (.NET) e o aplicativo mobile (Expo). Depois disso, o [README.md](README.md) explica o produto e como usar o repositório no dia a dia.

Você pode:

1. **Pedir para a ferramenta de IA instalar tudo** — copie o prompt da seção seguinte e cole no Cursor, Claude, Copilot, Windsurf ou equivalente.
2. **Instalar você mesmo** — siga os passos manuais mais abaixo.

Não é preciso instalar Visual Studio, Docker, banco de dados separado nem Expo CLI global. O SQLite nasce com a API. O Expo sobe com `npx`.

## O que precisa existir na máquina

| Ferramenta | Versão | Para quê |
| --- | --- | --- |
| Git | qualquer recente | clonar e versionar |
| SDK .NET | **10** (LTS). Este repositório foi criado com `10.0.100` | backend, testes, EF |
| Node.js | **20 ou superior** (LTS). O ambiente de criação usou v24 | aplicativo Expo |
| npm | vem com o Node | pacotes do `mobile/` |
| Editor | Cursor, VS Code ou similar | abrir o projeto e usar IA |

Opcional, só se você quiser o app no celular ou em emulador:

- Expo Go no telefone, **ou**
- emulador Android / simulador iOS

No Windows, o caminho mais simples para validar o app é o **Expo web** (`npx expo start --web`). Não instale Android Studio só para começar o curso.

## Prompt para a ferramenta de IA

Copie o bloco inteiro, cole no chat da ferramenta que você estiver usando (Cursor, Claude, Copilot, etc.) e envie. Ela deve inspecionar a máquina, instalar o que faltar, restaurar pacotes e provar que o projeto compila.

```text
Quero que você prepare este computador para rodar o projeto CardPlay deste repositório.

Contexto:
- Este é um monorepositório didático (backend .NET + mobile Expo).
- O primeiro material do aluno é o arquivo COMECEPORAQUI.md na raiz. Leia também README.md, global.json, mobile/package.json e a pasta backend/.
- Não altere código de produto, regras de negócio, paleta, arquitetura nem o backlog de compra.
- Não faça commit, push, force push nem altere git config.
- Não instale Visual Studio, Docker, SQL Server, Android Studio ou Expo CLI global só para este setup.
- Não implemente compra de produtos.

O que precisa existir:
- Git
- SDK .NET 10 (LTS). O global.json pede 10.0.100 com rollForward latestFeature. Aceite 10.0.x; não instale .NET 8/9 no lugar do 10 e não faça upgrade desnecessário se o 10 já estiver ok.
- Node.js 20 ou superior (prefira LTS) e npm
- Pacotes NuGet da solution backend/CardPlay.sln
- Ferramenta local dotnet-ef (manifesto dotnet-tools.json na raiz; se `dotnet tool restore` procurar .config/dotnet-tools.json e falhar, use `dotnet tool restore --tool-manifest dotnet-tools.json`)
- Dependências npm em mobile/
- Arquivo mobile/.env a partir de mobile/.env.example, com EXPO_PUBLIC_API_URL=http://localhost:5080 se eu for usar Expo web ou ferramentas no PC

Como trabalhar:
1. Detecte o sistema operacional.
2. Confira o que já está instalado: git --version, dotnet --version, node --version, npm --version.
3. Instale somente o que faltar, com o gerenciador nativo da máquina (Windows: winget; macOS: Homebrew; Linux: o gerenciador da distro ou o instalador oficial da Microsoft / Node).
   - Windows (exemplo): winget install Git.Git ; winget install Microsoft.DotNet.SDK.10 ; winget install OpenJS.NodeJS.LTS
   - macOS (exemplo): brew install git node ; SDK .NET 10 pelo instalador oficial ou cask equivalente
   - Se o gerenciador falhar, abra a página oficial e me diga exatamente o que baixar: https://git-scm.com/downloads ; https://dotnet.microsoft.com/download/dotnet/10.0 ; https://nodejs.org
4. Se a instalação exigir permissão de administrador ou um novo terminal por causa do PATH, peça isso de forma clara e espere. Não invente que deu certo sem evidência.
5. Depois das ferramentas no PATH, na raiz do repositório:
   - dotnet tool restore (ou com --tool-manifest, se necessário)
   - dotnet restore backend/CardPlay.sln
   - dotnet build backend/CardPlay.sln
   - dotnet test backend/CardPlay.sln
6. No mobile: copie .env.example para .env se ainda não existir; npm install.
7. Não precisa subir servidores longos a menos que eu peça. Se subir a API, use `dotnet run --project backend/src/CardPlay.Api --urls http://localhost:5080`.

Ao terminar, mostre evidência:
- versões de git, dotnet (deve começar com 10), node (>= 20) e npm
- resultado do build e dos testes
- se mobile/.env existe
- o que você instalou versus o que já estava na máquina
- comandos exatos para eu subir a API e o Expo
- o que você não conseguiu validar

Se algo falhar, pare, explique o erro e o próximo passo. Não afirme que “está funcionando” só porque um instalador rodou.
```

Depois que a ferramenta terminar, confira os números que ela mostrou (`dotnet --version` começando com **10**, `node --version` **v20** ou mais). Aí suba os projetos na seção [Executar API e aplicativo](#executar-api-e-aplicativo).

## Instalação manual

Faça na ordem. Depois de instalar Git, .NET ou Node, **feche e abra o terminal** (e o editor) para o PATH atualizar.

### 1. Git

- Windows: [git-scm.com/download/win](https://git-scm.com/download/win) ou `winget install --id Git.Git -e`
- macOS: `xcode-select --install` ou `brew install git`
- Linux: `sudo apt install git` (Debian/Ubuntu) ou o equivalente da sua distro

Confira:

```bash
git --version
```

### 2. SDK .NET 10

Baixe o **SDK** (não só o Runtime) da linha 10:

[https://dotnet.microsoft.com/download/dotnet/10.0](https://dotnet.microsoft.com/download/dotnet/10.0)

Atalhos:

- Windows: `winget install --id Microsoft.DotNet.SDK.10 -e`
- macOS: instalador oficial, ou Homebrew se oferecer o SDK 10
- Linux: siga [Install .NET on Linux](https://learn.microsoft.com/dotnet/core/install/linux)

Confira:

```bash
dotnet --version
```

O resultado deve começar com `10.`. O arquivo `global.json` deste repositório pede `10.0.100` e permite roll-forward na mesma faixa (`latestFeature`). Não use .NET 8 ou 9 no lugar.

### 3. Node.js 20+

Instale a versão **LTS** em [https://nodejs.org](https://nodejs.org) (20, 22 ou 24 servem).

- Windows: `winget install --id OpenJS.NodeJS.LTS -e`
- macOS: `brew install node`
- Linux: [NodeSource](https://github.com/nodesource/distributions) ou o gerenciador da distro, desde que a versão seja 20+

Confira:

```bash
node --version
npm --version
```

`node` deve ser `v20` ou maior. O npm vem junto.

### 4. Clonar ou abrir o repositório

Se você ainda não tem a pasta do projeto:

```bash
git clone <url-do-repositorio> ia360
cd ia360
```

Se já recebeu o material em uma pasta, abra essa pasta no editor. Os comandos abaixo assumem a **raiz** do repositório (onde estão `COMECEPORAQUI.md`, `README.md` e `backend/`).

### 5. Pacotes do backend e ferramenta EF

Na raiz:

```bash
dotnet tool restore --tool-manifest dotnet-tools.json
dotnet restore backend/CardPlay.sln
dotnet build backend/CardPlay.sln
dotnet test backend/CardPlay.sln
```

Se `dotnet tool restore` (sem `--tool-manifest`) funcionar na sua máquina, pode usá-lo. A ferramenta local restaurada é o `dotnet-ef` na versão 10.0.12, usada só quando você for criar migrations.

Os testes devem passar. Eles cobrem criação de cartão, recarga inválida, saldo e movimentação.

### 6. Pacotes do mobile

```bash
cd mobile
```

Windows (PowerShell):

```powershell
Copy-Item .env.example .env
npm install
```

macOS / Linux:

```bash
cp .env.example .env
npm install
```

O `.env` padrão usa `EXPO_PUBLIC_API_URL=http://localhost:5080`, adequado para Expo web e para ferramentas no mesmo PC. Ajuste só se for emulador Android (`http://10.0.2.2:5080`) ou celular físico (`http://IP_DA_SUA_MAQUINA:5080`). Detalhes no [README.md](README.md).

## Executar API e aplicativo

Use **dois terminais**, ambos a partir da raiz do repositório.

**Terminal 1 — API**

```bash
dotnet run --project backend/src/CardPlay.Api --urls http://localhost:5080
```

Na primeira subida a API aplica a migration e cria `cardplay.db` (arquivo local, fora do Git).

- API: http://localhost:5080
- OpenAPI: http://localhost:5080/openapi/v1.json
- Scalar: http://localhost:5080/scalar

**Terminal 2 — Expo**

```bash
cd mobile
npx expo start
```

No Windows, sem emulador, o mais simples é:

```bash
cd mobile
npx expo start --web
```

Isso abre o app no navegador contra `http://localhost:5080`.

Celular físico: instale o Expo Go, aponte `EXPO_PUBLIC_API_URL` para o IP da sua máquina, suba a API em `http://0.0.0.0:5080` e libere a porta 5080 no firewall se o telefone não alcançar o PC.

## Conferência rápida

Você está pronto quando:

- `dotnet --version` começa com `10`
- `node --version` é v20 ou maior
- `dotnet build` e `dotnet test` da solution passam
- a API responde em http://localhost:5080
- o Expo sobe (web, Expo Go ou emulador)

Se a API não sobe, leia a mensagem do `dotnet run`. Causas comuns: SDK 8/9 no PATH no lugar do 10, ou a porta 5080 ocupada.

Se o Expo não acha a API, confira `mobile/.env` e se a API está de pé **antes** do app.

## O que não instalar agora

- Visual Studio completo (o SDK .NET basta)
- Docker, SQL Server, PostgreSQL
- Android Studio, só para o primeiro dia
- Expo CLI global (`npm install -g expo-cli` é antigo e desnecessário)
- Compra, autenticação, banco real ou qualquer item do [docs/backlog.md](docs/backlog.md)

## Depois que estiver rodando

Siga esta ordem de leitura:

1. [README.md](README.md) — o que o produto faz e como executar no dia a dia
2. [AGENTS.md](AGENTS.md) — regras para pessoas e para a IA mexerem no código
3. [docs/architecture.md](docs/architecture.md) — caminho de uma ação até o SQLite
4. [docs/product.md](docs/product.md) e [docs/ui.md](docs/ui.md) — comportamento e visual
5. [docs/exemplo-prompt-criacao.md](docs/exemplo-prompt-criacao.md) — como o projeto foi pedido à IA
