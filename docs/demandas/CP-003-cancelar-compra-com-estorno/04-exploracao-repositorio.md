# CP-003 — Exploração do repositório

Base do código **depois** da CP-002. Descreve o que a compra concluída já guarda e o que não existe para cancelá-la. Não define a implementação da CP-003. Os testes foram localizados; não foram executados.

O arquivo `04-exploracao-repositorio.md` da CP-002 descreve o repositório de antes da quantidade escolhida. Não serve como mapa atual.

Legenda: **Fato** · **Lacuna**.

Fontes lidas: `01-entendimento.md`, `02-historia-rica.md` e `03-criterios-de-aceite.md` desta pasta; `Cartao.cs`, `MovimentacaoCartao.cs`, `CompraServico.cs`, `CartaoServico.cs`, `CartaoRepositorio.cs`, `MovimentacaoCartaoConfiguracao.cs`, `MovimentacaoResposta.cs`, `MapeadorCartao.cs`, `CartoesController.cs`, `TratadorExcecoes.cs`, `ProdutoConfiguracao.cs`, `cartaoApi.ts`, `tipos/index.ts`, `TelaMovimentacoes.tsx`.

---

## 1. O que a compra já percorre

**Fato.** O caminho da compra concluída é:

```text
Catálogo → POST /api/cartoes/{id}/compras { produtoId, quantidade, precoUnitarioVisto }
        → CompraServico.ComprarAsync
        → Cartao.Comprar(Produto, quantidade)
        → um SaveChanges
        → saldo debitado e um registro
```

`CardPlay.Services` não participa. Não há carrinho nem estoque.

**Fato.** `CartoesController` expõe solicitar, obter, recarregar, comprar e listar movimentações. Não há ação de cancelar compra nem de estornar.

---

## 2. O registro da compra

**Fato.** `MovimentacaoCartao` guarda `Id`, `CartaoId`, `Valor`, `Descricao`, `DataHora`, `ProdutoId`, `NomeProduto`, `Quantidade` e `PrecoUnitario`. `CriarCompra` grava a descrição “Compra”, `Valor` igual ao total e o preço unitário à parte. A recarga usa `Criar` e deixa produto, quantidade e preço unitário vazios.

**Fato.** Não há propriedade de data de cancelamento nem de id de outra movimentação. O snapshot `CardPlayDbContextModelSnapshot` também não tem essas colunas.

**Fato.** Os setters são privados. Só `Criar` e `CriarCompra` montam o registro. Não há método para marcar uma compra já criada.

**Fato.** `MovimentacaoResposta` e o tipo `Movimentacao` do aplicativo repetem esses campos. `MapeadorCartao.ParaResposta` não envia outro dado.

**Lacuna.** Nada no registro diz se a compra foi cancelada, nem aponta um estorno para a compra.

---

## 3. Saldo e dono da compra

**Fato.** `Cartao.Saldo` sobe em `Recarregar` e desce em `Comprar`. Não há método que some o `Valor` de uma compra já gravada.

**Fato.** `CartaoRepositorio.ObterPorIdAsync` carrega o cartão com `Include` das movimentações daquele cartão. Não há consulta de uma movimentação pelo id, fora do cartão.

**Fato.** A listagem ordena por `DataHora` decrescente em `CartaoServico.ListarMovimentacoesAsync`.

**Lacuna.** Não há como pedir o cancelamento de uma compra do cartão A usando o cartão B, porque essa operação não existe. O código também não recusa esse cruzamento.

---

## 4. Histórico

**Fato.** `TelaMovimentacoes` trata como compra toda movimentação com `nomeProduto` preenchido. A compra mostra o nome, o valor sem “+”, a quantidade, o preço unitário e a data. O restante mostra `descricao` e o valor com “+”.

**Fato.** A tela não tem botão no item. “Cancelar”, no produto de hoje, está no aviso de “Usar cartão” e só impede a compra que ainda não ocorreu.

**Fato.** `cartaoApi` não tem chamada além de solicitar, obter, recarregar, comprar e listar movimentações.

**Lacuna.** O histórico não oferece “Cancelar compra”, não mostra “Cancelada” e não tem de onde ler o nome do produto de um estorno por vínculo, porque o vínculo não existe.

---

## 5. Recusas e falha ao gravar

**Fato.** Cartão ou produto ausente na compra vira HTTP 404, título “Recurso não encontrado”. Preço visto diferente do vigente vira HTTP 409, título “Conflito”. Saldo insuficiente, quantidade inválida e estouro viram HTTP 400, título “Requisição inválida”. O `detail` é `exception.Message`.

**Fato.** Exceção não mapeada vira HTTP 500, título “Erro interno”, com o mesmo `detail`.

**Fato.** `CompraServico` envolve falha de `SalvarAlteracoesAsync` em `CompraNaoConcluidaException`, mensagem “A compra não foi concluída.”, e deixa passar `OperationCanceledException`. O teste `ComprarAsync_FalhaAoSalvar_RelendoSqlite_MantemSaldoENaoGravaCompra` faz o salvar lançar e relê outro contexto SQLite. Não foi executado nesta leitura.

**Fato.** `SalvarAlteracoesAsync` é um `SaveChangesAsync`. Não há interruptor de falha no produto.

**Lacuna.** Não há exceção, frase nem teste de cancelamento não concluído.

---

## 6. Preço do catálogo e o valor já pago

**Fato.** `Produto.Preco` é `decimal`, precisão 18 e escala 2. O seed em `ProdutoConfiguracao` tem 8,50, 32,90, 79,00, 4,00, 18,75 e 59,90. Não há produto a R$ 20,00.

**Fato.** O total debitado fica em `MovimentacaoCartao.Valor` no momento da compra. O preço vigente continua em `Produto.Preco`. Mudar o preço do catálogo não altera o `Valor` já gravado.

**Lacuna.** Nenhuma operação lê esse `Valor` para creditar o cartão.

---

## 7. O que isso significa para a CP-003

| Pedido já decidido | Onde o código está |
| --- | --- |
| Cancelar uma compra concluída pelo id da movimentação | O id existe. Não há rota nem método |
| Ação no Histórico, com aviso e desistência sem API | O histórico só lista. O aviso de desistência existe na compra nova, não na concluída |
| Crédito do `Valor`, uma vez, também com quantidade maior que 1 | `Valor` já é o total. O saldo só sobe na recarga |
| Data de cancelamento na compra original | A coluna e o método não existem |
| Estorno na mesma lista, com “+” e o nome pelo vínculo | A lista é a mesma. A tela usa `nomeProduto` para decidir que a linha é compra e não tem vínculo para ler o nome |
| Repetição sem outro crédito | Não há marca para consultar |
| Compra de outro cartão ou inexistente recusada | A compra já tem `CartaoId`. Não há operação que faça essa recusa |
| Falha ao salvar só no teste, sem rastro | O meio existe na compra. Não existe no cancelamento |
| CP-004 fora | Não há quantidade em estoque no produto |

Nenhum desses buracos foi implementado nesta exploração.
