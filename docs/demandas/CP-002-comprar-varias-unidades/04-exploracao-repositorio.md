# CP-002 — Exploração do repositório

Base do código **depois** da CP-001. Descreve o que a compra de uma unidade já faz e o que não existe para várias unidades. Não define a implementação da CP-002. Os testes foram localizados; não foram executados.

O arquivo `04-exploracao-repositorio.md` da CP-001 descreve o repositório de antes da compra. Não serve como mapa atual.

Legenda: **Fato** · **Lacuna**.

Fontes lidas: `01-entendimento.md`, `02-historia-rica.md` e `03-criterios-de-aceite.md` desta pasta; `Cartao.cs`, `MovimentacaoCartao.cs`, `CompraServico.cs`, `CompraRequisicao.cs`, `MovimentacaoResposta.cs`, `CartoesController.cs`, `MovimentacaoCartaoConfiguracao.cs`, `cartaoApi.ts`, `CartaoContexto.tsx`, `TelaCatalogo.tsx`, `TelaMovimentacoes.tsx`.

---

## 1. O que a compra já percorre

**Fato.** O caminho continua o da CP-001:

```text
Catálogo → POST /api/cartoes/{id}/compras { produtoId }
        → CompraServico.ComprarAsync
        → Cartao.Comprar(Produto)
        → um SaveChanges
        → saldo e um registro
```

`CardPlay.Services` não participa. Não há carrinho.

---

## 2. Quantidade

**Fato.** `MovimentacaoCartao.CriarCompra` grava `Quantidade = 1`. O método não recebe quantidade.

**Fato.** `Cartao.Comprar` compara `Saldo` com `produto.Preco` e subtrai só `produto.Preco`. Não multiplica.

**Fato.** A coluna `Quantidade` já existe, inteira e anulável, na migration `ProdutoEQuantidadeNaMovimentacao`. A recarga deixa esse campo nulo. Os testes de domínio afirmam quantidade 1 na compra e nulo na recarga. Não foram executados nesta leitura.

**Lacuna.** Não há entrada de quantidade na API, no domínio nem no card do catálogo.

---

## 3. Preço e total

**Fato.** `Produto.Preco` é `decimal`, precisão 18 e escala 2, coluna `TEXT`. O catálogo devolve esse preço. O seed tem 8,50, 32,90, 79,00, 4,00, 18,75 e 59,90.

**Fato.** Na compra, `Valor` recebe `produto.Preco`. Não há propriedade de preço unitário em `MovimentacaoCartao`, no DTO `MovimentacaoResposta` nem no tipo `Movimentacao` do aplicativo.

**Fato.** `CompraRequisicao` tem só `ProdutoId`. `cartaoApi.comprar` envia só `{ produtoId }`.

**Lacuna.** Nada envia o preço que o cliente viu. Nada compara esse preço com o vigente. `TratadorExcecoes` não tem status 409. A mudança de preço entre ver e confirmar não existe no código.

**Lacuna.** Não há cálculo de total nem recusa por estouro de `decimal`.

---

## 4. Confirmação e histórico

**Fato.** `CardProduto` abre o aviso com o nome e `formatarMoeda(produto.preco)`. Não há campo de quantidade. Cancelar não chama `onConfirmar`. Sem cartão, o botão fica desabilitado.

**Fato.** `TelaCatalogo` chama `comprar(produto.id)` e permanece no catálogo. A frase de sucesso mostra o saldo devolvido.

**Fato.** `TelaMovimentacoes` mostra, na compra, `nomeProduto`, `Quantidade {n}` e o valor sem “+”. Na recarga, a descrição e o valor com “+”. A data usa `formatarDataHora`. O valor exibido é `movimentacao.valor`, hoje igual ao preço da unidade.

**Lacuna.** A tela não mostra preço unitário separado do total, porque o dado não existe.

---

## 5. Recusas que já existem

**Fato.** Saldo menor que `produto.Preco` lança `SaldoInsuficienteException` antes de debitar. A API traduz para HTTP 400, detalhe “Saldo insuficiente para concluir a compra.”

**Fato.** Cartão ou produto ausente lança antes de `SalvarAlteracoesAsync` e vira HTTP 404, título “Recurso não encontrado”.

**Fato.** `SalvarAlteracoesAsync` é um `SaveChangesAsync`. Não há falha provocada no teste nem no produto. A CP-001 deixou o ACE-08 sem mecanismo.

**Lacuna.** Quantidade 0, negativa ou fracionária não é validada, porque a quantidade não entra no contrato.

---

## 6. O que isso significa para a CP-002

| Pedido já decidido | Onde o código está |
| --- | --- |
| Quantidade inteira maior que 1 | A coluna existe. O domínio força 1 |
| `Valor` = total e preço unitário à parte | `Valor` é o preço da unidade. Não há coluna de preço unitário |
| Confirmação com quantidade, unitário e total | O aviso só tem nome e preço |
| Enviar o preço visto e responder 409 se mudou | O corpo só tem `produtoId`. Não há 409 |
| Quantidade 1 no mesmo botão e na mesma lista | O botão e a lista já são esses |
| Falha de gravação só no teste | Não há ponto de falha no salvar |

Nenhum desses buracos foi implementado nesta exploração.
