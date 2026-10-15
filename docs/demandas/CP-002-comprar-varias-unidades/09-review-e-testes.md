# CP-002 — Revisão e testes

Revisão de leitura do código e de `08-validacao.md`. Não altera produto, não gera teste e não executa a suíte. Nenhum achado abaixo está corrigido por este arquivo.

Não há problema de saldo confirmado nos percursos já capturados: café com quantidade 3 e saldo maior, adesivo com quantidade 1, fone com saldo menor que o total, e, na API, saldo igual ao total, quantidade inválida e preço diferente. Isso não garante ausência de problemas fora dessas capturas.

## Achados

### 1. A suíte não tem execução gravada

| Campo | Conteúdo |
| --- | --- |
| Classificação | Lacuna de evidência, não falha observada |
| Prioridade | Média para a entrega, porque o relatório de testes não pode citar a suíte |
| Localização | `CartaoTestes.cs`, `CompraServicoTestes.cs` |
| Evidência | Os métodos existem, inclusive quantidade 3, quantidade inválida, estouro, preço diferente e falha ao salvar. `08-validacao.md` registra que não há arquivo com a saída de `dotnet test`. Os totais em `07-execucao.md` não estão em outro arquivo |
| Condição | Qualquer leitura que trate esses testes como verdes |
| Comportamento | Não se sabe, por execução gravada, se a suíte atual passa |
| Impacto | A entrega fica sem prova de regressão da recarga e da compra de uma unidade junto com a quantidade maior |
| Confirmação | Guardar a saída de `dotnet test backend/CardPlay.sln`. Este arquivo não a executa |

### 2. O estouro numérico não foi observado

| Campo | Conteúdo |
| --- | --- |
| Classificação | Lacuna de evidência da ACE-07 |
| Prioridade | Média. A frase e o 400 estão no código; nenhuma captura os mostra |
| Localização | `Cartao.Comprar` lança `EstouroDeTotalException`. `TratadorExcecoes` mapeia essa exceção ao HTTP 400. O teste é `Comprar_TotalEstouraDecimal_NaoAlteraSaldoNemRegistraCompra` |
| Evidência | `percurso-etapa4/chamadas.txt` cobre 0, −1 e 1,5. Não há corpo do estouro. A saída do teste não está em arquivo |
| Condição | Preço vezes quantidade não cabe em `decimal`, com o preço visto igual ao vigente |
| Comportamento | Não observado |
| Impacto | Não se sabe, por captura, se a API devolve a frase decidida e se o saldo permanece |
| Confirmação | Uma chamada HTTP, ou a saída gravada desse teste. Não foi feita aqui |

### 3. O 409 e o saldo zero não foram vistos na tela

| Campo | Conteúdo |
| --- | --- |
| Classificação | Lacuna de evidência de interface. A API desses dois casos está gravada |
| Prioridade | Baixa para o contrato HTTP. Média se a entrega exigir o gesto na tela |
| Localização | `percurso-etapa4/chamadas.txt` tem saldo 0 no adesivo, quantidade 3, e o 409 do preço 3 contra 4. `percurso-etapa5/` não repete esses dois |
| Evidência | `08-validacao.md`, ACE-04 e ACE-09 |
| Condição | Saldo igual ao total no catálogo, ou o preço do card diferente do vigente na confirmação |
| Comportamento | Na API, os dois acontecem como decidido. Na tela, não há captura |
| Impacto | O aviso e o saldo do catálogo desses casos não foram vistos |
| Confirmação | Dois percursos no aplicativo. Não foram feitos aqui |

### 4. O backlog ainda descreve a compra como quantidade 1

| Campo | Conteúdo |
| --- | --- |
| Classificação | Problema confirmado de texto, não de saldo |
| Prioridade | Baixa para o comportamento já visto. Média para quem for alterar o repositório guiado só pelo backlog |
| Localização | `docs/backlog.md` |
| Evidência | O título continua “Demandas futuras” e “Não implementar até serem pedidas com critérios claros”. O item “Utilizar saldo em produtos” diz “Atendida” e “na quantidade 1”. A CP-002 já está no diretório de trabalho |
| Condição | Leitura só do backlog |
| Comportamento | A quantidade maior pode ser tratada como proibida, ou a quantidade 1 como a única compra existente |
| Impacto | Não muda a API nem a tela. Confunde a próxima alteração |
| Confirmação | Já está no arquivo. A correção não foi feita |

### 5. Saldo do contexto fica para trás se a lista falha depois da compra

| Campo | Conteúdo |
| --- | --- |
| Classificação | Hipótese a investigar. O mesmo encadeamento já existia na CP-001 e em `recarregar` |
| Prioridade | Baixa no fluxo capturado. Média só se a listagem falhar com a compra já gravada |
| Localização | `CartaoContexto.tsx`, `comprar`: `cartaoApi.comprar` e, só depois, `listarMovimentacoes`. `setCartao` fica após as duas chamadas |
| Evidência | O código está nessa ordem. As capturas de sucesso mostram saldo novo, então a listagem respondeu nesses percursos |
| Condição | `POST /compras` grava e `GET /movimentacoes` falha em seguida |
| Comportamento | O catálogo mostra erro e o saldo do contexto não troca. O servidor já debitou. Reabrir o cartão relê o saldo |
| Impacto | A pessoa pode achar que a compra não ocorreu |
| Confirmação | Interromper a listagem depois de um 200. Não há captura disso |

### 6. A hora do histórico pode estar no relógio UTC

| Campo | Conteúdo |
| --- | --- |
| Classificação | Hipótese herdada da CP-001. Esta revisão não gravou o fuso do navegador |
| Prioridade | Baixa. Não muda saldo nem o registro |
| Localização | `MovimentacaoCartao` grava `DateTime.UtcNow`. `TelaMovimentacoes` mostra `formatarDataHora` |
| Evidência | `percurso-etapa6/log.txt` e `historico.png` mostram 15/10/2026, 14:02. Não há, na pasta, o fuso do navegador ao lado desse horário |
| Condição | O instante UTC é mostrado com os dígitos UTC |
| Comportamento | A hora vista pode não ser a hora local |
| Impacto | A data do exemplo continua 15/10/2026. A hora pode divergir sem alterar o débito |
| Confirmação | Gravar o fuso do navegador e o JSON de `GET /movimentacoes` do mesmo cartão. Não está feito |

## Falsos positivos

| Suspeita | Por que não é achado |
| --- | --- |
| Compra de quantidade 3 gravada mais de uma vez | `percurso-etapa6/historico.png` mostra um café, um adesivo e uma recarga |
| Quantidade 1 deixou de funcionar | O mesmo histórico mostra o adesivo sem “+”, quantidade 1 e preço unitário R$ 4,00. O saldo foi de R$ 74,50 para R$ 70,50 |
| Recarga ganhou “Preço unitário” ou perdeu o “+” | A recarga do mesmo percurso é “Recarga” e “+ R$ 100,00”, sem quantidade nem preço unitário |
| `Produto.Disponivel` ignorado | Continua fora da regra, como na CP-001 |
| Preço diferente debitou | `chamadas.txt` mostra 409 e saldo 20 antes da confirmação seguinte |

## Testes

### No arquivo, execução não comprovada

| Teste | Cenário que o código do teste afirma |
| --- | --- |
| `Comprar_Quantidade3_SaldoMaiorQueTotal_DebitaTotalERegistraUmaCompra` | 100 − 60, quantidade 3, unitário 20, valor 60 |
| `Comprar_Quantidade3_SaldoIgualAoTotal_ConcluiComSaldoZero` | Saldo zero |
| `Comprar_Quantidade3_SaldoMenorQueTotal_NaoAlteraSaldoNemRegistraCompra` | Saldo e lista intactos |
| `Comprar_QuantidadeInvalida_NaoAlteraSaldoNemRegistraCompra` | 0, −1 e 1,5 |
| `Comprar_TotalEstouraDecimal_NaoAlteraSaldoNemRegistraCompra` | Estouro sem alterar o cartão |
| `Comprar_SaldoMaiorQuePreco_DebitaPrecoERegistraCompra` | Quantidade 1, unitário e valor iguais ao preço |
| `ComprarAsync_PrecoVistoDiferente_NaoGrava` | Nenhuma gravação |
| `ComprarAsync_FalhaAoSalvar_InformaQueNaoConcluiuENaoGrava` | Mensagem “A compra não foi concluída.”, saldo gravado anterior, compra fora das movimentações gravadas |

O teste da falha compara um retrato do repositório em memória, atualizado só quando o salvar conclui. Não abre SQLite e não relê o banco. O objeto do domínio já foi debitado quando o salvar lança; a resposta da compra não é devolvida.

A recarga e os testes de quantidade 1 da CP-001 continuam no arquivo, também sem saída nova de suíte.

### Comprovado por captura, não por teste automatizado

| Cenário | Artefato |
| --- | --- |
| Saldo maior, café R$ 8,50, quantidade 3, saldo 74,50 | `percurso-etapa4/chamadas.txt`, `percurso-etapa5/log.txt` |
| Aviso com produto, quantidade, unitário e total; cancelar não compra | `percurso-etapa5/01-alerta-cafe.png`, `percurso-etapa5/log.txt` |
| Saldo menor na API e na tela | `chamadas.txt`, `percurso-etapa5/log.txt` |
| Quantidades 0, −1 e 1,5 | `chamadas.txt` |
| Saldo igual ao total, só na API | `chamadas.txt`, adesivo, saldo 12 para 0 |
| Preço diferente, 409, e confirmação seguinte, só na API | `chamadas.txt` |
| Quantidade 1 no catálogo e no histórico | `percurso-etapa5/log.txt`, `percurso-etapa6/historico.png` |
| Histórico com unitário, total sem “+” e recarga com “+” | `percurso-etapa6/historico.png` |

Não há `tsc.txt` nesta pasta. `07-execucao.md` relata código 0; essa saída não está em outro arquivo.

### Propostos, não escritos e não executados

- Saída gravada de `dotnet test backend/CardPlay.sln`.
- Chamada HTTP do estouro de `decimal`.
- Percurso na tela do saldo igual ao total e do 409.
- Percurso no alerta nativo do celular. As capturas são web.
- JSON de `GET /movimentacoes` ao lado do fuso do navegador.

Nenhuma dessas propostas foi implementada aqui.

## Decisões ainda abertas

Registrada em `07-decisoes.md`. Não vira correção nesta revisão.

- Aprovar RN01–RN04 e CA01–CA05 como o texto oficial da história.

## Fora do escopo

- Carrinho, estoque, desconto, frete, parcelamento, banco, autenticação e teto comercial.
- Duas confirmações ao mesmo tempo. O plano deixa isso de fora.
- `Produto.Disponivel`.

## Limitações desta revisão

- Não rodou build, teste nem o aplicativo.
- Não reabriu o banco.
- Ausência de achado confirmado de saldo não é prova de que todo preço do seed ou toda falha de rede se comporta como as capturas.

A decisão de aceitar a entrega continua humana.
