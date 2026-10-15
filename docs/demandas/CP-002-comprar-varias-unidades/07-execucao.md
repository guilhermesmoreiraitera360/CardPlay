# CP-002 — Acompanhamento de execução

Escrito em 2026-10-15, depois das oito etapas. Não substitui `06-plan.md` nem `07-decisoes.md`. Cada etapa foi aceita na conversa antes da seguinte.

A saída de `dotnet test` de cada etapa foi vista no terminal e não foi copiada para um arquivo na hora. Os percursos de HTTP e de tela estão em arquivo. Uma execução da suíte com o código já das oito etapas, neste dia, informou: **36 aprovados, 0 falhas**.

| Data | Etapa | O que ficou |
| --- | --- | --- |
| 2026-10-15 | 1 — quantidade e total no domínio | Aceita. Suíte informou 28 aprovados. Sem migration |
| 2026-10-15 | 2 — preço unitário no SQLite | Aceita. Solution compilou. Suíte informou 28 aprovados |
| 2026-10-15 | 3 — orquestração | Aceita. Suíte informou 35 aprovados |
| 2026-10-15 | 4 — HTTP | Aceita. Chamadas em `percurso-etapa4/chamadas.txt` |
| 2026-10-15 | 5 — confirmação no catálogo | Aceita. Percurso em `percurso-etapa5/` |
| 2026-10-15 | 6 — histórico | Aceita. Percurso em `percurso-etapa6/` |
| 2026-10-15 | 7 — documentos | Aceita. `product.md`, `AGENTS.md` e `ui.md` |
| 2026-10-15 | 8 — falha de gravação no teste | Aceita. Suíte informou 36 aprovados |

Depois do plano, `README.md` e `docs/architecture.md` foram alinhados à mesma descrição. `07-decisoes.md` foi escrito em seguida e aceito antes deste arquivo.

## Etapa 1 — Domínio

`Cartao.Comprar` passa a receber quantidade. O total é o preço vezes a quantidade. O registro guarda `PrecoUnitario`, quantidade e `Valor` como total. Quantidade inválida, estouro e saldo menor que o total lançam antes de alterar o cartão. `Comprar(produto)` continua comprando uma unidade.

Arquivos: `Cartao.cs`, `MovimentacaoCartao.cs`, `QuantidadeInvalidaException.cs`, `EstouroDeTotalException.cs`, `CartaoTestes.cs`.

`dotnet test` com filtro `CartaoTestes` informou 20 aprovados. A suíte inteira informou 28 aprovados, 0 falhas. Não houve migration nem SQLite.

## Etapa 2 — SQLite

`PrecoUnitario` entra em `MovimentacoesCartao`, anulável, `TEXT`, precisão 18 e escala 2. Migration `20261015133903_PrecoUnitarioNaMovimentacao`. Sem tabela nova e sem chave estrangeira.

A solution compilou com 0 avisos e 0 erros. A suíte informou 28 aprovados. `cardplay.db` não foi usado como prova nesta etapa. A coluna foi aplicada quando a API subiu na etapa 4.

## Etapa 3 — Orquestração

`CompraServico.ComprarAsync` recebe quantidade e preço unitário visto. Preço diferente lança `PrecoAlteradoException` e não grava. Preço igual chama o domínio e um `SalvarAlteracoesAsync`. A rota antiga, só com o produto, continuou comprando uma unidade pelo preço vigente até a etapa 4.

Arquivos: `PrecoAlteradoException.cs`, `ICompraServico.cs`, `CompraServico.cs`, `CompraServicoTestes.cs`.

O filtro `CompraServicoTestes` informou 12 aprovados. A suíte informou 35 aprovados. A solution compilou sem avisos.

## Etapa 4 — HTTP

O corpo passa a ser `produtoId`, `quantidade` e `precoUnitarioVisto`. O controlador só encaminha. A resposta leva `precoUnitario`; `valor` é o total. Preço diferente responde 409, título “Conflito”. Quantidade inválida e saldo insuficiente continuam 400. A assinatura antiga do serviço foi removida.

Evidência gravada em `percurso-etapa4/chamadas.txt`, contra `http://localhost:5080`:

- Café, quantidade 3, preço visto 8,50, saldo 100: HTTP 200, saldo 74,5, quantidade 3, unitário 8,5, total 25,5.
- Adesivo, quantidade 3, saldo 12: HTTP 200, saldo 0, total 12.
- Adesivo, quantidade 3, saldo 10: HTTP 400, “Saldo insuficiente para concluir a compra.”, saldo permanece 10.
- Quantidades 0, −1 e 1,5: HTTP 400, “Informe uma quantidade inteira maior que zero.”, saldo permanece 20.
- Preço visto 3 contra adesivo de 4: HTTP 409, saldo permanece 20. A confirmação seguinte, com 4, concluiu e o saldo foi para 16.

A suíte, rodada antes dessas chamadas, continuava em 35 aprovados. O aplicativo ainda não enviava o corpo novo.

## Etapa 5 — Catálogo

A quantidade é escolhida no card. O aviso mostra produto, quantidade, preço unitário e total. Cancelar não chama a API. Confirmar envia a quantidade e o preço mostrado. O cliente permanece no catálogo.

`node node_modules/typescript/bin/tsc --noEmit`, a partir de `mobile`, terminou com código 0.

Percurso em `percurso-etapa5/log.txt` e `percurso-etapa5/01-alerta-cafe.png`. Cartão CP-NRBG9K, id `c214fd08-081c-4678-9eec-990439f25b14`:

- Café, quantidade 3. Cancelar manteve R$ 100,00 e não houve `POST /compras`. Confirmar deixou R$ 74,50 no catálogo.
- Fone, quantidade 3, total R$ 237,00. A tela mostrou “Saldo insuficiente para concluir a compra.” O saldo continuou R$ 74,50.
- Adesivo, quantidade 1. Unitário e total R$ 4,00. O saldo foi para R$ 70,50.

O histórico na tela ainda não mostrava o preço unitário.

## Etapa 6 — Histórico

A compra mostra nome, total sem “+”, quantidade e preço unitário. A recarga continua com a descrição e com “+”. O tipo da movimentação no aplicativo passou a incluir `precoUnitario`.

`tsc --noEmit` terminou com código 0. Percurso em `percurso-etapa6/log.txt` e `percurso-etapa6/historico.png`, no mesmo cartão CP-NRBG9K: café R$ 25,50, quantidade 3, unitário R$ 8,50; adesivo R$ 4,00, quantidade 1, unitário R$ 4,00; recarga “+ R$ 100,00”.

A captura foi gravada primeiro em `mobile/docs/...` por causa do diretório do comando. Foi movida para `percurso-etapa6/historico.png` ao escrever este arquivo.

## Etapa 7 — Documentos

`docs/product.md`, `AGENTS.md` e `docs/ui.md` deixaram de dizer que toda compra é uma unidade e que o débito é sempre o preço. Passam a descrever quantidade escolhida, preço unitário e total. Nenhuma regra nova.

`README.md` e `docs/architecture.md` ainda diziam uma unidade. Foram alinhados depois do fim do plano, na mesma conversa, fora da lista da etapa 7.

## Etapa 8 — Falha ao gravar

`ComprarAsync_FalhaAoSalvar_InformaQueNaoConcluiuENaoGrava` faz o salvar lançar. A exceção é `CompraNaoConcluidaException`, mensagem “A compra não foi concluída.” O saldo gravado no teste permanece 100 e a compra não entra nas movimentações gravadas. Não há interruptor no produto.

A suíte informou 36 aprovados, 0 falhas. A execução posterior citada no início deste arquivo repetiu esse total.
