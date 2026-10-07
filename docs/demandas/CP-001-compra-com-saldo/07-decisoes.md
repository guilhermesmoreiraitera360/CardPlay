# CP-001 — Decisões que desbloqueiam a etapa 1

Registro da etapa 0 do `06-plan.md`, só no que muda o domínio. Respostas dadas na revisão de 2026-10-07. Este arquivo não atualiza `docs/backlog.md`, `AGENTS.md` nem os demais documentos de produto: isso continua na etapa 7.

## Respondidas

| Bloqueio | Resposta |
| --- | --- |
| K3 | RN01–RN04 e ACE-01–ACE-08 valem como estão escritos nos artefatos 02 e 03 |
| P1.5 / K2 | A CP-001 refina o item “Utilizar saldo em produtos” e passa a ser a demanda autorizada, em etapas |
| P1.1 / K1 | O registro da compra entra na mesma lista de movimentações do cartão, junto com as recargas |
| P1.2 | O registro carrega campos próprios: produto, quantidade, valor, data e hora |
| H4 | A quantidade gravada é sempre 1 |
| Persistência | Colunas novas em `MovimentacaoCartao` para produto e quantidade. Sem entidade nova e sem usar `Descricao` para isso |

## Consequência no modelo da etapa 1

`MovimentacaoCartao` continua sendo o registro da lista. A recarga não preenche produto nem quantidade. A compra preenche:

- `ProdutoId` e `NomeProduto`, para o mesmo registro identificar o produto e mostrar o nome sem outra consulta
- `Quantidade`, sempre 1
- `Valor` e `DataHora`, que já existiam

`Descricao` continua obrigatória, com tamanho máximo 120. Na compra o texto é “Compra”. Produto e quantidade não entram nesse texto.

A recusa de saldo insuficiente é `SaldoInsuficienteException`, distinta de `RecargaInvalidaException`. O texto que o cliente vê, o canal e os status HTTP continuam em P2.1.

`Produto.Disponivel` não entra na regra.

## Persistência fechada na etapa 2

| Ponto | Resposta |
| --- | --- |
| `ValueGeneratedOnAdd` some do snapshot dos `Id` de cartão e de movimentação, sem SQL no `Up` | Permanece assim. `CartaoConfiguracao` e `MovimentacaoCartaoConfiguracao` já pedem `ValueGeneratedNever()` desde o commit inicial. O designer da migration `Inicial` ainda diz `ValueGeneratedOnAdd` porque esse modelo foi gravado sem acompanhar essa configuração. O snapshot novo acompanha a configuração. A coluna `Id` não muda no SQLite, então o `Up` não tem SQL de `Id`. `dotnet ef migrations has-pending-model-changes` respondeu que não há mudança de modelo depois da migration `ProdutoEQuantidadeNaMovimentacao` |
| `ProdutoId` sem chave estrangeira para `Produtos` | Permanece sem chave. O registro guarda o id e a cópia do nome na própria linha. Apagar ou alterar o produto no catálogo não redefine essa linha. Não há `HasOne` para `Produto` |

## HTTP fechado na etapa 4

| Ponto | Resposta |
| --- | --- |
| Rota | `POST /api/cartoes/{id}/compras` em `CartoesController`, no mesmo recurso da recarga |
| Entrada | `CompraRequisicao` com `ProdutoId` |
| Sucesso | `CompraResposta` com `CartaoId`, `Saldo` e `Registro`. O registro usa `MovimentacaoResposta`, agora com `ProdutoId`, `NomeProduto` e `Quantidade`. A listagem de movimentações devolve os mesmos campos; recarga deixa os três nulos |
| Saldo insuficiente | HTTP 400, título “Requisição inválida”, `detail` “Saldo insuficiente para concluir a compra.” |
| Cartão ou produto ausente | HTTP 404, título “Recurso não encontrado”. O `detail` continua a frase de cada exceção |

## Interface fechada na etapa 5

| Ponto | Resposta |
| --- | --- |
| P2.2 | O preço já está no card. “Usar cartão” abre a confirmação com o nome e esse preço. Cancelar não chama a API. Não há tela nova |
| P3.2 | O botão que já existia passa a confirmar a compra quando há cartão no contexto. Sem cartão, permanece visível e desabilitado |
| Cartão usado na compra | O cartão já carregado em `CartaoProvider`. P2.4 continua fora: cartão ausente no aplicativo não é tratado como dado inválido de ACE-07 |
| P2.1 na interface | A tela mostra o `detail` devolvido pela API. Não há outro texto de saldo insuficiente |
| P3.3 | Depois da compra o cliente permanece no catálogo. O saldo do contexto é atualizado |

## Ainda abertas

Estas respostas não existem. A etapa correspondente segue sem diff até serem registradas.

| Bloqueio | Etapas que continuam paradas |
| --- | --- |
| P1.4, rastro de tentativa que não seja compra concluída | 3 e 8 |
| P1.3, algo além de saldo debitado e registro | fora do plano, se a revisão exigir |
| P3.3, para onde o cliente vai depois | fora do plano |
| P2.3, preço se o cadastro mudar antes da confirmação | fora do plano |
| P2.4, cartão não selecionado ou id local inválido | fora do plano |
| P2.5, sinal e rótulo do valor gasto | 6 |
| P3.1, formato de data e hora da compra | 6 |
| K4, autenticação, estoque, checkout e banco | plano novo, se a resposta for incluí-los |
| ACE-08, como provocar a falha de efetivação | 8 |
