# AGENTS.md

Orientações para pessoas e para agentes de IA que forem modificar este repositório.

Alunos que ainda não têm o ambiente: o primeiro material é o `COMECEPORAQUI.md`. Se pedirem para instalar SDK, Node, Git ou pacotes, siga esse arquivo; não invente versões.

## Antes de mudar qualquer arquivo

1. Leia este arquivo, o `README.md`, `docs/architecture.md` e `docs/ui.md`. Para setup de máquina, leia `COMECEPORAQUI.md`.
2. Explore a estrutura atual e o Git. Não sobrescreva trabalho existente.
3. Entenda o impacto da mudança. Prefira o menor diff que resolva o problema.
4. A compra de um produto com saldo já está implementada. Não acrescente estoque, carrinho, checkout, autenticação nem integração bancária sem uma demanda nova.

Este projeto ensina o ciclo **Entender → Planejar → Desenvolver → Validar → Entregar → Evoluir**. Planeje antes de implementar.

## Arquitetura e dependências

```text
CardPlay.Api            → Application, Repository, Services, Domain
CardPlay.Repository     → Application, Domain
CardPlay.Services       → Application, Domain
CardPlay.Application    → Domain
CardPlay.Domain         → (nenhuma)
CardPlay.Tests          → Domain, Application, Repository
```

Não crie referências circulares. Domain não referencia ASP.NET, EF Core nem o frontend. Repository e Services estão no mesmo nível: infraestrutura acionada pelos contratos da Application.

### Responsabilidades

- **Domain:** entidades (`Cartao`, `MovimentacaoCartao`, `Produto`) e regras essenciais. Recarga com valor `<= 0` é inválida. A compra debita o preço e cria o registro juntos, ou rejeita saldo insuficiente antes de alterar os dois. A quantidade da compra é 1. O saldo e a movimentação nascem juntos no domínio.
- **Application:** DTOs, contratos, mapeamentos e **orquestração dos casos de uso**. Sem infraestrutura e sem clientes HTTP externos.
- **Repository:** DbContext, EF Core, SQLite, migrations, seed de produtos, repositórios concretos.
- **Services:** apenas clientes de APIs e serviços externos. Nesta versão não há integrações externas; não coloque casos de uso aqui.
- **Api:** HTTP, composition root, ProblemDetails, OpenAPI. Controllers pequenos: recebem, chamam a Application, devolvem.

## Nomenclatura

Código em português (`Cartao`, `Produto`, `CartoesController`, `/api/cartoes`).  
`CardPlay` permanece só no nome do produto e dos projetos.

## Regras de implementação

- Lógica de negócio não fica no controller.
- A orquestração dos casos de uso fica na Application, não em Services.
- Services só entra quando existir integração com API ou serviço externo.
- A API não expõe entidades do EF. Use DTOs.
- Prefira repositórios específicos. Não crie Generic Repository, MediatR, CQRS, mensageria ou microserviços sem uma necessidade atual.
- Use `async`/`await` e `CancellationToken` onde fizer sentido.
- Valores monetários são `decimal`.
- Datas em UTC.
- Não armazene PAN, CVV, credenciais ou secrets.
- Não implemente estoque, carrinho, checkout, autenticação ou integração bancária nesta versão.
- O botão “Usar cartão” permanece visível. Com cartão no contexto, confirma a compra de uma unidade pelo preço do catálogo. Sem cartão, permanece desabilitado.

## Persistência

SQLite via EF Core. O arquivo `cardplay.db` não entra no Git.  
A API aplica migrations na subida. Novas migrations:

```bash
dotnet ef migrations add Nome --project backend/src/CardPlay.Repository --startup-project backend/src/CardPlay.Api --output-dir Migrations
```

Uma recarga e uma compra precisam persistir saldo e movimentação no mesmo `SaveChanges`.

## Frontend

- Cliente HTTP apenas em `mobile/src/api/`.
- URL apenas em `mobile/src/config/ambiente.ts` (`EXPO_PUBLIC_API_URL`).
- O id do cartão selecionado fica no AsyncStorage.
- Cores só em `mobile/src/tema/cores.ts`. Azul = identidade e navegação. Laranja = saldo, preços, recarga e compra. Vermelho só para erro.
- O histórico mostra a compra com nome do produto, quantidade e valor sem “+”. A recarga continua com a descrição e com “+”.
- A barra inferior precisa mostrar ícone e rótulo por completo, acima da área segura. Não reduza a altura se isso cortar o texto.
- Conteúdo das telas respeita o recorte superior (`usePaddingTela`).
- Catálogo: um produto por linha abaixo de 720px; duas colunas a partir disso.
- Padrões visuais: `docs/ui.md`.

## Comandos

```bash
dotnet tool restore --tool-manifest dotnet-tools.json
dotnet build backend/CardPlay.sln
dotnet test backend/CardPlay.sln
dotnet run --project backend/src/CardPlay.Api --urls http://localhost:5080

cd mobile
npx tsc --noEmit
npx expo start
```

## Testes

Teste comportamento relevante, não cobertura artificial.

Mínimo esperado:

- criação de cartão válida
- recarga com valor `<= 0` rejeitada
- saldo atualizado após recarga
- movimentação correspondente registrada
- compra com saldo suficiente debita o preço e registra produto, quantidade, valor e data
- compra com saldo igual ao preço termina em zero
- compra com saldo insuficiente não debita nem registra compra

## Validação

Não afirme que “está funcionando” só porque compilou. Mostre evidência: teste, chamada HTTP, tela ou log. Depois de cada mudança relevante, rode o build e os testes afetados.
