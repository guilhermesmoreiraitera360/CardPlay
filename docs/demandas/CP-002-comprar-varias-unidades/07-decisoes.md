# CP-002 — Decisões

Registro das decisões que desbloquearam o plano e do que cada etapa aprovada fechou em seguida. As de 2026-10-15 autorizaram o caminho da história. A inspeção de cada etapa aceitou o nome ou o contrato proposto naquela etapa. Este arquivo não reescreve a história nem os artefatos 01 a 06.

## Respondidas em 2026-10-15

| ID | Resposta |
| --- | --- |
| P1.1 | A quantidade sempre 1 da CP-001 não é defeito. A CP-002 a conserva quando a quantidade escolhida é 1 e acrescenta quantidades inteiras maiores. Não há correção prévia separada |
| P1.2 | `Valor` continua sendo o total debitado. O preço unitário aplicado entra num campo próprio do mesmo registro. Quantidade, nome do produto, data e hora permanecem |
| P1.3 | A confirmação envia a quantidade e o preço unitário que o cliente viu. O servidor compara esse preço com o vigente. Se forem diferentes, recusa sem debitar. O total só é o preço vigente vezes a quantidade quando os preços coincidem |
| P1.4 | A falha de gravação é provocada no teste, no momento de salvar, e não por um interruptor no produto. Não grava débito nem compra. Não fica rastro de tentativa. O cliente é informado de que a compra não foi concluída |
| P2.1 | A quantidade é escolhida no card do catálogo. O aviso mostra produto, quantidade, preço unitário e total. Cancelar não chama a API. O botão continua “Usar cartão” |
| P2.2 | Quantidade inválida e estouro: HTTP 400, “Informe uma quantidade inteira maior que zero.” ou “Não foi possível calcular o total sem estouro numérico.” Saldo insuficiente: o mesmo 400 e “Saldo insuficiente para concluir a compra.” Preço alterado: HTTP 409, “O preço deste produto mudou. Confirme novamente o total atualizado.” Falha de gravação: informar que a compra não foi concluída, sem compra no histórico |
| P2.3 | Não há teto comercial. A quantidade é inteira maior ou igual a 1 e cabe em `int`. O total é `decimal`. Se a multiplicação estourar, a compra é recusada antes de alterar o cartão |
| P2.4 | Quantidade 1 usa o mesmo cartão, o mesmo botão e a mesma lista. O corpo passa a levar a quantidade. O registro guarda o preço unitário, igual ao preço do catálogo, e o total, igual a esse preço |
| P3.1 | Depois da compra o cliente permanece no catálogo |
| P3.2 | O histórico da compra mostra nome, quantidade, preço unitário, total sem “+” e a data no formato já usado. A recarga não muda |

Em 2026-10-15, RN01–RN04 e CA01–CA05 foram aprovadas como texto oficial da história. As decisões acima fecham o caminho; não reescrevem o enunciado.

## Domínio fechado na etapa 1

O nome do campo aceito é `PrecoUnitario`.

`Cartao.Comprar` recebe a quantidade. Quantidade 0, negativa ou fracionária lança `QuantidadeInvalidaException`. Estouro na multiplicação lança `EstouroDeTotalException`. Saldo menor que o total lança `SaldoInsuficienteException`. As três acontecem antes de alterar saldo e lista. `Valor` do registro é o total. A chamada `Comprar(produto)` continua comprando uma unidade.

## Persistência fechada na etapa 2

`MovimentacoesCartao` ganha `PrecoUnitario`, `TEXT`, precisão 18 e escala 2, anulável. A recarga continua sem esse dado. Sem tabela nova e sem chave estrangeira, como `ProdutoId`. A migration é `20261015133903_PrecoUnitarioNaMovimentacao`.

## Orquestração fechada na etapa 3

O caso de uso recebe quantidade e preço unitário visto. Preço diferente do vigente lança `PrecoAlteradoException` e não grava. Preço igual chama o domínio e um `SalvarAlteracoesAsync`.

## HTTP fechado na etapa 4

| Ponto | Resposta |
| --- | --- |
| Rota | A mesma `POST /api/cartoes/{id}/compras` |
| Entrada | `CompraRequisicao` com `ProdutoId`, `Quantidade` e `PrecoUnitarioVisto` |
| Sucesso | `CompraResposta` com saldo e registro. O registro leva `PrecoUnitario`. `Valor` é o total. A listagem de movimentações devolve o mesmo campo; a recarga o deixa nulo |
| Saldo insuficiente | HTTP 400, título “Requisição inválida”, `detail` “Saldo insuficiente para concluir a compra.” |
| Quantidade inválida | HTTP 400, título “Requisição inválida”, `detail` “Informe uma quantidade inteira maior que zero.” |
| Estouro | HTTP 400, título “Requisição inválida”, `detail` “Não foi possível calcular o total sem estouro numérico.” |
| Preço alterado | HTTP 409, título “Conflito”, `detail` “O preço deste produto mudou. Confirme novamente o total atualizado.” |

## Interface fechada na etapa 5

A quantidade é escolhida no card. O aviso é “Confirmar a compra de {produto}, quantidade {n}, preço unitário {unitário}, total {total}?”. Cancelar não chama a API. Confirmar envia a quantidade e o preço unitário mostrado. O cliente permanece no catálogo. Sem cartão, “Usar cartão” permanece visível e desabilitado.

## Histórico fechado na etapa 6

A compra mostra o nome do produto, o total sem “+”, a linha “Quantidade” e a linha “Preço unitário”. A recarga continua com a descrição e com “+”. A data usa o mesmo `formatarDataHora`.

## Falha de gravação fechada na etapa 8

O teste faz `SalvarAlteracoesAsync` falhar. Não há interruptor no produto. A exceção vista pelo caso de uso é `CompraNaoConcluidaException`, com a mensagem “A compra não foi concluída.” O saldo gravado e a lista gravada permanecem os anteriores. P2.2 não escolheu um status HTTP novo para essa falha. O `TratadorExcecoes` continua tratando exceção não mapeada como HTTP 500, e o `detail` é essa mensagem.

## Fechadas na revisão da entrega

| Bloqueio | Situação |
| --- | --- |
| Aprovar RN01–RN04 e CA01–CA05 como o texto oficial da história | Aprovadas em 2026-10-15. O enunciado permanece o de `02-historia-rica.md` e `03-criterios-de-aceite.md` |
| Pares escritos R$ 100,00 / R$ 20,00 × 3, saldo R$ 60,00 e saldo R$ 50,00 | Aceitos como exemplo da regra. O seed não tem produto a R$ 20,00. Café a R$ 8,50 e adesivo a R$ 4,00 fecham o mesmo critério |
