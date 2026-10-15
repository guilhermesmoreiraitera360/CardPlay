# CP-002 — Análise de impacto

## Estado deste documento

| Campo | Valor |
| --- | --- |
| Demanda | CP-002 — Comprar várias unidades de um produto |
| Artefato | Impacto da história sobre o código depois da CP-001 |
| Fontes | `02-historia-rica.md`, `03-criterios-de-aceite.md`, `04-exploracao-repositorio.md`, decisões de 2026-10-15 em `01-entendimento.md` |
| Data | 2026-10-15 |
| O que este arquivo faz | Liga cada efeito observável ao ponto do código que hoje não o faz |
| O que este arquivo não faz | Não escolhe o nome da migration, não escreve código e não executa testes |

Legenda: **Confirmado** (o código citado não faz o que o critério pede) · **Decidido** (o caminho já está em `01-entendimento.md`) · **Fora** (a história exclui).

---

## O que a compra atual atravessa

O fluxo de uma unidade já existe. A CP-002 não cria outro caminho de camadas.

```text
CardProduto → TelaCatalogo → CartaoContexto.comprar(produtoId)
           → POST /api/cartoes/{id}/compras { produtoId }
           → CompraServico.ComprarAsync
           → Cartao.Comprar(Produto)
           → MovimentacaoCartao.CriarCompra  (quantidade 1, Valor = preço)
           → um SaveChanges
```

`CardPlay.Services` continua sem caso de uso.

---

## Impactos

### I-01 — Quantidade deixa de ser sempre 1

| Campo | Conteúdo |
| --- | --- |
| Grau | **Confirmado** |
| Onde | `Cartao.Comprar` e `MovimentacaoCartao.CriarCompra` |
| Hoje | A compra recebe só o produto. A quantidade gravada é a constante 1. O saldo é comparado e debitado com `produto.Preco` |
| Efeito | ACE-01, ACE-04 e ACE-08 exigem quantidade escolhida e total = preço × quantidade. Sem mudar esses dois métodos, quantidade 3 ainda debita uma unidade |
| Critério | ACE-01, ACE-04, ACE-08 |

### I-02 — Preço unitário separado do total

| Campo | Conteúdo |
| --- | --- |
| Grau | **Confirmado** |
| Onde | `MovimentacaoCartao`, `MovimentacaoCartaoConfiguracao`, `MovimentacaoResposta`, tipo `Movimentacao` do aplicativo |
| Hoje | `Valor` guarda o preço da unidade. Não há propriedade de preço unitário. A coluna `Quantidade` já existe e é anulável |
| Efeito | A decisão P1.2 manda `Valor` ser o total e o preço unitário aplicado ter campo próprio. Isso pede coluna nova. A quantidade não pede coluna nova |
| Critério | ACE-01, ACE-03, ACE-08 |

### I-03 — Corpo da confirmação

| Campo | Conteúdo |
| --- | --- |
| Grau | **Confirmado** |
| Onde | `CompraRequisicao`, `CartoesController.Comprar`, `cartaoApi.comprar`, `CartaoContexto.comprar` |
| Hoje | O corpo é `{ produtoId }`. O controlador chama `ComprarAsync(id, produtoId)` |
| Efeito | A decisão P1.3 manda a confirmação enviar a quantidade e o preço unitário visto. Sem esses dois dados, o servidor não distingue quantidade 3 nem preço antigo |
| Critério | ACE-01, ACE-07, ACE-09 |

### I-04 — Preço que mudou

| Campo | Conteúdo |
| --- | --- |
| Grau | **Confirmado** |
| Onde | `CompraServico`, `TratadorExcecoes` |
| Hoje | O serviço lê o produto e usa o preço vigente, sem comparar com o que o cliente viu. Não há HTTP 409 |
| Efeito | ACE-09 exige 409, a frase “O preço deste produto mudou. Confirme novamente o total atualizado.”, saldo intacto e nenhuma compra. Uma confirmação seguinte, com o preço novo, pode concluir |
| Critério | ACE-09 |

### I-05 — Quantidade inválida e estouro

| Campo | Conteúdo |
| --- | --- |
| Grau | **Confirmado** |
| Onde | Não há validação de quantidade. `decimal` estoura com `OverflowException` se a multiplicação não for contida |
| Hoje | 0, −1 e 1,5 não chegam ao domínio, porque a quantidade não está no contrato |
| Efeito | ACE-07 exige HTTP 400, as duas frases decididas, sem débito e sem compra. A recusa acontece antes de alterar o cartão |
| Critério | ACE-07, ACE-05 |

### I-06 — Saldo insuficiente para o total

| Campo | Conteúdo |
| --- | --- |
| Grau | **Confirmado** no mecanismo; o valor comparado muda |
| Onde | `Cartao.Comprar`, `SaldoInsuficienteException`, `TratadorExcecoes` |
| Hoje | A recusa compara o saldo com o preço de uma unidade e responde 400 com “Saldo insuficiente para concluir a compra.” |
| Efeito | ACE-06 usa a mesma frase e o mesmo 400, mas a comparação passa a ser com o total. O exemplo R$ 50 contra R$ 60 não pode debitar R$ 20 |
| Critério | ACE-06 |

### I-07 — Confirmação na tela

| Campo | Conteúdo |
| --- | --- |
| Grau | **Confirmado** |
| Onde | `CardProduto`, `TelaCatalogo` |
| Hoje | O aviso é “Confirmar a compra de {nome} por {preço}?”. Não há quantidade escolhida nem total de várias unidades |
| Efeito | ACE-02 exige ver produto, quantidade, preço unitário e total antes de confirmar. Cancelar continua sem chamar a API. Sem cartão, o botão continua desabilitado |
| Critério | ACE-02 |

### I-08 — Histórico

| Campo | Conteúdo |
| --- | --- |
| Grau | **Confirmado** no que falta mostrar |
| Onde | `TelaMovimentacoes` |
| Hoje | A compra mostra nome, “Quantidade {n}” e `valor` sem “+”. A recarga mostra descrição e “+”. Como `valor` é o preço da unidade, a tela ainda não mostra unitário e total separados |
| Efeito | A decisão P3.2 manda mostrar nome, quantidade, preço unitário, total sem “+” e a mesma data. A recarga não muda |
| Critério | ACE-01, ACE-08 |

### I-09 — Falha ao gravar

| Campo | Conteúdo |
| --- | --- |
| Grau | **Confirmado** |
| Onde | `CartaoRepositorio.SalvarAlteracoesAsync`; testes em memória só incrementam um contador |
| Hoje | Não há falha provocada. A CP-001 deixou o ACE-08 sem mecanismo |
| Efeito | A decisão P1.4 manda a falha existir no teste, no salvar, sem interruptor no produto, sem débito gravado, sem compra concluída e sem rastro. ACE-10 não se observa copiando o `SaveChanges` que funciona |
| Critério | ACE-10 |

### I-10 — Testes que fixam quantidade 1 e valor = preço

| Campo | Conteúdo |
| --- | --- |
| Grau | **Confirmado** |
| Onde | `CartaoTestes` e `CompraServicoTestes` |
| Hoje | Os métodos de compra afirmam quantidade 1 e `Valor` igual a `produto.Preco`. Não foram executados nesta leitura |
| Efeito | Quantidade 3 com `Valor` igual ao total quebra essas afirmações se elas continuarem escritas para uma unidade. Quantidade 1 precisa continuar coberta, agora com unitário e total iguais |
| Critério | ACE-01, ACE-08 |

### I-11 — Documentos que dizem quantidade sempre 1

| Campo | Conteúdo |
| --- | --- |
| Grau | **Confirmado** |
| Onde | `docs/product.md`, `AGENTS.md` |
| Hoje | Os dois dizem que a quantidade da compra é 1 e que o saldo final é o anterior menos o preço do catálogo |
| Efeito | Depois do comportamento novo, essa frase descreve só a quantidade 1. A atualização dos documentos fica para o fim, como na CP-001, para não documentar antes da inspeção |
| Critério | Nenhum ACE novo. É alinhamento posterior |

---

## O que não muda

| Item | Por quê |
| --- | --- |
| Camadas e `CardPlay.Services` | A orquestração continua na Application |
| Mesma lista de histórico e o mesmo botão | Decisões P2.1, P2.4 e P3.2 |
| Recarga | Continua sem produto, sem quantidade e com “+” |
| Cartão ou produto ausente | Continuam 404, sem débito. A CP-002 não reescreve esse caso |
| `Produto.Disponivel` e estoque | Fora. A CP-004 não foi escolhida |
| Carrinho, desconto, frete, parcelamento, banco, autenticação | Fora da história |
| Concorrência de duas confirmações | A CP-001 deixou isso fora. A CP-002 não cria token |
| Permanecer no catálogo | Decisão P3.1. A tela já fica no catálogo |

---

## Dados

| Dado | Hoje | Depois da decisão |
| --- | --- | --- |
| Quantidade | Constante 1 na compra; nula na recarga | Inteiro enviado na confirmação; 1 continua válido |
| Preço unitário aplicado | Não existe | Campo próprio, igual ao preço vigente aceito |
| Total debitado | `Valor` = preço de uma unidade | `Valor` = preço × quantidade |
| Preço visto | Não viaja | Vai no corpo, para a comparação do ACE-09 |
| Saldo | `decimal` | Continua `decimal`. Estouro recusa antes de alterar o cartão |

---

## Ordem em que o impacto aparece

A quantidade no domínio sem o corpo HTTP não é observável na tela. O preço unitário sem coluna não volta na listagem. O 409 sem o preço visto no corpo não tem o que comparar. A tela que mostra o total antes do servidor calcular esse total mente, se os dois divergirem.

Por isso a análise separa: regra e registro, depois contrato, depois confirmação e histórico, e a falha de gravação por último, porque ela depende de existir uma compra que grave.

O plano ainda não está escrito. Este arquivo não abre etapa de código.
