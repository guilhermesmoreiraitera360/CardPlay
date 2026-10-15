# CP-002 — Plano de implementação: Comprar várias unidades

## Estado deste documento

| Campo | Valor |
| --- | --- |
| Demanda | CP-002 — Comprar várias unidades de um produto |
| Artefato | Plano para avaliação humana, antes de autorizar código |
| Fontes | `01-entendimento.md` a `05-analise-de-impacto.md` |
| Arquitetura | A do artefato 04: a compra de uma unidade já existe. Este plano não cria outra camada |
| Data | 2026-10-15 |
| Decisões de caminho | Fechadas em `01-entendimento.md`, seção de 2026-10-15 |
| Efeito deste arquivo | Ordena a mudança e os pontos de inspeção. **Não autoriza código** até a leitura deste plano |

As etapas não se juntam. Cada uma para para inspeção antes da próxima.

---

## Veredito

O caminho de negócio está decidido. Dá para escrever a alteração de cada etapa sem reabrir quantidade, total, preço visto, 409 ou falha de teste.

Nenhuma etapa deve ser executada só porque este arquivo existe. A primeira autorização, se a revisão concordar, é a etapa 1.

---

## Escopo

Dentro:

- Quantidade inteira maior ou igual a 1 de um único produto.
- Total = preço vigente × quantidade, em `decimal`, sem estouro e sem teto comercial.
- Confirmação mostra produto, quantidade, preço unitário e total, e envia a quantidade e o preço visto.
- Preço diferente do vigente: 409, sem débito.
- Saldo menor que o total: 400, frase já usada, sem débito.
- Quantidade inválida e estouro: 400, frases já decididas, sem débito.
- Um débito e um registro. `Valor` é o total. O preço unitário aplicado é campo próprio.
- Quantidade 1 no mesmo botão e na mesma lista.
- Falha de gravação só no teste, sem rastro e sem interruptor no produto.

Fora: carrinho, desconto, frete, parcelamento, banco, autenticação, estoque, concorrência e documento de produto antes da inspeção do comportamento.

---

## Ordem

```text
Etapa 1  quantidade e total no domínio
   ↓
Etapa 2  preço unitário no SQLite
   ↓
Etapa 3  orquestração
   ↓
Etapa 4  HTTP
   ↓
Etapa 5  confirmação no catálogo
   ↓
Etapa 6  histórico
   ↓
Etapa 7  documentos que ainda dizem quantidade 1
   ↓
Etapa 8  falha de gravação no teste
```

---

## Etapas

### Etapa 1 — Quantidade e total no domínio

| Campo | Conteúdo |
| --- | --- |
| Objetivo | A compra passa a receber quantidade, debita o total ou não altera nada |
| Critério | ACE-01 na forma geral, ACE-04, ACE-05, ACE-06 e ACE-07 no domínio. ACE-08 no que a quantidade 1 continua debitando o preço |
| Alteração | `Cartao.Comprar` deixa de usar só `produto.Preco`. `CriarCompra` deixa de fixar quantidade 1. Quantidade inválida e estouro lançam antes de mudar saldo e lista. Saldo menor que o total lança a exceção já existente, também antes |
| Componentes | `Cartao.cs`, `MovimentacaoCartao.cs`, `CartaoTestes.cs` |
| Proposto | O preço unitário no registro nasce aqui, no objeto, ainda sem coluna. O nome proposto do campo é `PrecoUnitario`. A inspeção pode recusar o nome antes da etapa 2 |
| Evidência | Testes de domínio, sem banco: quantidade 3 com saldo maior; saldo igual ao total; saldo menor que o total sem compra; 0, −1 e fração; quantidade 1 com total igual ao preço; estouro sem alterar o cartão. Os testes atuais que exigem quantidade 1 e `Valor` = preço são atualizados para o que a quantidade 1 passa a significar |
| Parada | Inspeção do domínio e dos testes, antes de migration |

### Etapa 2 — Preço unitário no SQLite

| Campo | Conteúdo |
| --- | --- |
| Objetivo | O preço unitário aplicado persiste na mesma movimentação |
| Critério | ACE-03 no que o registro gravado tem o unitário. A quantidade já tem coluna |
| Alteração | Configuração e migration da coluna nova, anulável, para a recarga continuar sem esse dado. Sem tabela nova e sem chave estrangeira, como `ProdutoId` já é |
| Componentes | `MovimentacaoCartaoConfiguracao.cs`, pasta `Migrations` |
| Evidência | A migration adiciona só essa coluna. A solution compila. `cardplay.db` não é prova |
| Parada | Inspeção do modelo, antes do serviço |

### Etapa 3 — Orquestração

| Campo | Conteúdo |
| --- | --- |
| Objetivo | O caso de uso recebe quantidade e preço visto, compara com o vigente e grava uma vez |
| Critério | ACE-01, ACE-06, ACE-07, ACE-09 e o par débito-registro da ACE-03, ainda sem HTTP |
| Alteração | `CompraServico.ComprarAsync` deixa de receber só o produto. Preços diferentes recusam sem salvar. Preços iguais chamam o domínio e um `SalvarAlteracoesAsync` |
| Componentes | `CompraServico.cs`, `CompraServicoTestes.cs` |
| Proposto | A recusa de preço diferente é uma exceção própria, para a etapa 4 mapeá-la ao 409. O nome fica na inspeção desta etapa |
| Evidência | Testes em memória: uma gravação no sucesso; nenhuma gravação em saldo insuficiente, quantidade inválida e preço diferente. Quantidade 1 grava unitário e total iguais ao preço |
| Parada | Inspeção do caso de uso, antes da rota |

### Etapa 4 — HTTP

| Campo | Conteúdo |
| --- | --- |
| Objetivo | A confirmação entra com quantidade e preço visto e sai com saldo, total e unitário, ou com a recusa |
| Critério | ACE-06, ACE-07 e ACE-09 observáveis na API. ACE-01 no corpo de sucesso |
| Alteração | `CompraRequisicao` ganha quantidade e preço unitário visto. O controlador só encaminha. `TratadorExcecoes` ganha o 409. `MovimentacaoResposta` passa a levar o preço unitário |
| Componentes | `CompraRequisicao.cs`, `CartoesController.cs`, `TratadorExcecoes.cs`, `MovimentacaoResposta.cs`, mapeador |
| Evidência | Chamadas gravadas: sucesso com quantidade 3; saldo igual ao total; 400 de saldo insuficiente; 400 de quantidade 0, −1 e 1,5; 409 de preço diferente. O aplicativo ainda não muda |
| Parada | Inspeção do contrato, antes da tela |

### Etapa 5 — Confirmação no catálogo

| Campo | Conteúdo |
| --- | --- |
| Objetivo | O cliente escolhe a quantidade no card e só confirma depois de ver produto, quantidade, unitário e total |
| Critério | ACE-02. ACE-01, ACE-04 e ACE-06 no gesto. ACE-09 na tela, se o 409 aparecer |
| Alteração | `CardProduto`, `TelaCatalogo`, `cartaoApi.comprar`, `CartaoContexto.comprar`. O cliente HTTP continua só em `mobile/src/api/`. Sem tela nova. Sem mudar a barra nem `cores.ts` |
| Evidência | Percurso: saldo maior que o total, aviso com os quatro dados, cancelar sem POST, confirmar e saldo novo no catálogo. Repetir com saldo menor que o total. Quantidade 1 continua possível |
| Parada | Inspeção do fluxo, antes do histórico |

### Etapa 6 — Histórico

| Campo | Conteúdo |
| --- | --- |
| Objetivo | A mesma lista mostra quantidade, preço unitário e total da compra |
| Critério | ACE-01 e ACE-03 na consulta. A recarga conserva descrição e “+” |
| Alteração | `TelaMovimentacoes.tsx` |
| Evidência | Um cartão com uma recarga e uma compra de quantidade 3: a compra sem “+”, com quantidade, unitário e total; a recarga com “+” |
| Parada | Inspeção da lista, antes dos documentos |

### Etapa 7 — Documentos que ainda dizem quantidade 1

| Campo | Conteúdo |
| --- | --- |
| Objetivo | `docs/product.md` e `AGENTS.md` deixarem de dizer que toda compra é uma unidade e que o débito é sempre o preço |
| Critério | Nenhuma regra nova. As frases passam a descrever quantidade escolhida, total e preço unitário |
| Alteração | Só esses dois arquivos, mais `docs/ui.md` se a frase da confirmação ainda citar só nome e preço |
| Evidência | Leitura cruzada das frases, depois do comportamento inspecionado |
| Parada | Inspeção do texto |

### Etapa 8 — Falha de gravação no teste

| Campo | Conteúdo |
| --- | --- |
| Objetivo | ACE-10: o salvar falha e não fica débito nem compra |
| Critério | ACE-10 |
| Alteração | Um teste faz o salvar falhar. Não há interruptor no produto nem rastro de tentativa |
| Evidência | O teste mostra saldo anterior e nenhuma compra concluída. Não se observa ACE-10 só com o `SaveChanges` que funciona |
| Parada | Fim do plano |

---

## O que nenhuma etapa faz

- Carrinho ou mais de um produto na mesma compra.
- Estoque e `Produto.Disponivel`.
- Teto comercial de quantidade.
- Token contra duas confirmações simultâneas.
- Documento de produto nas etapas 1 a 6.
