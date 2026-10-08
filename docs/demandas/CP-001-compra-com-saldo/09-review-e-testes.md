# CP-001 — Revisão e testes

Revisão de leitura do código e de `08-validacao.md`. Não altera produto, não gera teste e não executa a suíte. Nenhum achado abaixo está corrigido por este arquivo.

Não há problema de comportamento confirmado nos percursos já capturados (café com saldo maior, adesivo com saldo igual e adesivo com saldo menor). Isso não garante ausência de problemas fora dessas capturas.

## Achados

### 1. A suíte de compra não tem execução gravada

| Campo | Conteúdo |
| --- | --- |
| Classificação | Lacuna de evidência, não falha observada |
| Prioridade | Média para a entrega, porque o relatório de testes não pode citar a suíte |
| Localização | `backend/tests/CardPlay.Tests/CartaoTestes.cs`, `backend/tests/CardPlay.Tests/CompraServicoTestes.cs` |
| Evidência | Os métodos existem: três `Comprar_*` no domínio e cinco `ComprarAsync_*` na orquestração, em memória. `08-validacao.md` registra que não há saída de `dotnet test` posterior a esses métodos. O “Passed! 16” em `07-execucao.md` não tem arquivo de comando e é anterior a `CompraServicoTestes` |
| Condição | Qualquer leitura que trate esses testes como verdes |
| Comportamento | Não se sabe, por execução gravada, se a suíte atual passa |
| Impacto | A entrega fica sem prova de regressão da recarga junto com a compra |
| Confirmação | Guardar a saída de `dotnet test backend/CardPlay.sln`. Este arquivo não a executa |

Os testes em arquivo, se um dia forem executados, cobrem saldo maior, saldo igual, saldo insuficiente sem gravação, cartão ausente e produto ausente. Não abrem SQLite. Não há teste de controller, de tela nem de `SaveChanges` que falha.

### 2. Hora do histórico pode estar no relógio UTC

| Campo | Conteúdo |
| --- | --- |
| Classificação | Hipótese a investigar |
| Prioridade | Baixa. Não muda saldo nem o registro |
| Localização | `MovimentacaoCartao` grava `DateTime.UtcNow`. `TelaMovimentacoes` mostra `formatarDataHora`. A lista vem de `GET /movimentacoes`, não do corpo do `POST` |
| Evidência | `percurso-etapa5/compras.json` tem `dataHora` `2026-10-07T19:45:39.7661731Z`. `percurso-etapa6/historico.png` mostra `07/10/2026, 19:45`. `08-validacao.md` diz que o fuso da captura não foi gravado |
| Condição | O instante UTC seria mostrado com os dígitos UTC se o JSON da listagem perder o fuso e o navegador interpretar a hora como local, ou se o navegador estiver em UTC |
| Comportamento | A hora vista pode não ser a hora local de quem usa o aplicativo em UTC−3 |
| Impacto | A data continua no dia certo nesse exemplo. A hora pode divergir três horas sem alterar o débito |
| Confirmação | Gravar o fuso do navegador e o JSON de `GET /movimentacoes` do mesmo cartão. Não está feito |

### 3. Saldo do contexto fica para trás se a lista falha depois da compra

| Campo | Conteúdo |
| --- | --- |
| Classificação | Hipótese a investigar. O mesmo encadeamento já existe em `recarregar` |
| Prioridade | Baixa no fluxo capturado. Média só se a listagem falhar com a compra já gravada |
| Localização | `mobile/src/estado/CartaoContexto.tsx`, `comprar`: `cartaoApi.comprar` e, só depois, `listarMovimentacoes`. `setCartao` fica após as duas chamadas |
| Evidência | O código está nessa ordem. As capturas de sucesso mostram saldo novo, então a listagem respondeu nesses percursos |
| Condição | `POST /compras` grava e `GET /movimentacoes` falha em seguida |
| Comportamento | O catálogo mostra erro e o saldo do contexto não troca. O servidor já debitou. Reabrir o cartão relê o saldo |
| Impacto | A pessoa pode achar que a compra não ocorreu |
| Confirmação | Interromper a listagem depois de um 200. Não há captura disso |

### 4. Cabeçalho do backlog ainda manda não implementar

| Campo | Conteúdo |
| --- | --- |
| Classificação | Problema confirmado de texto, não de saldo |
| Prioridade | Baixa para o comportamento já visto. Média para quem for alterar o repositório guiado só pelo backlog |
| Localização | `docs/backlog.md` |
| Evidência | O item “Utilizar saldo em produtos” diz “Atendida”. O título do arquivo continua “Demandas futuras” e “Não implementar até serem pedidas com critérios claros” |
| Condição | Leitura só do cabeçalho |
| Comportamento | A compra já implementada pode ser tratada como proibida |
| Impacto | Não muda a API nem a tela. Confunde a próxima alteração |
| Confirmação | Já está no arquivo. A correção não foi feita |

## Falsos positivos

| Suspeita | Por que não é achado |
| --- | --- |
| Compra gravada duas vezes | `percurso-etapa6/historico.png` e `percurso-ace04/07-historico.png` mostram uma compra para cada confirmação |
| Cartão lido sem rastreamento e saldo não persistido | `ObterPorIdAsync` de cartão não usa `AsNoTracking`. Os corpos 200 e as telas mostram o saldo debitado |
| `Produto.Disponivel` ignorado | `07-decisoes.md` deixa esse campo fora da regra. Não é desvio do que foi aceito |
| Dois textos de 404 | ACE-07 pede informação de dado inválido, não a mesma frase. O 404 em si não tem corpo gravado; a diferença de frase não é, sozinha, um defeito |
| Histórico com “+” na compra | `percurso-etapa5/11-historico.png` é anterior à etapa 6. `percurso-etapa6/historico.png` mostra o café sem “+” |

## Testes

### No arquivo, execução não comprovada

| Teste | Cenário que o código do teste afirma |
| --- | --- |
| `Comprar_SaldoMaiorQuePreco_DebitaPrecoERegistraCompra` | 50 − 8,50, quantidade 1, nome do produto |
| `Comprar_SaldoIgualAoPreco_ConcluiComSaldoZero` | Saldo zero |
| `Comprar_SaldoMenorQuePreco_NaoAlteraSaldoNemRegistraCompra` | Saldo e lista intactos |
| `ComprarAsync_SaldoMaiorQuePreco_DebitaEGravaUmaVez` | Uma chamada a `SalvarAlteracoesAsync` e o registro na resposta |
| `ComprarAsync_SaldoIgualAoPreco_GravaComSaldoZero` | Grava com saldo zero |
| `ComprarAsync_SaldoMenorQuePreco_NaoGrava` | Não incrementa o contador de gravação |
| `ComprarAsync_CartaoAusente_NaoGrava` | Exceção e nenhuma gravação |
| `ComprarAsync_ProdutoAusente_NaoGravaEMantemSaldo` | Saldo mantido e nenhuma gravação |

A recarga continua coberta por `CartaoTestes` e `CartaoServicoTestes`, também sem saída nova de suíte.

### Comprovado por captura, não por teste automatizado

| Cenário | Artefato |
| --- | --- |
| Saldo maior, café R$ 8,50, saldo 1,50 | `percurso-etapa5/compras.json`, `07-compra-cafe.png`, `08-cartao-apos-compra.png` |
| Ver preço antes de confirmar; cancelar não compra | `percurso-etapa5/log.txt`, `04-catalogo-saldo-10.png`, `05-alerta-cafe.png` |
| Saldo menor, HTTP 400, saldo intacto, sem segunda compra | `percurso-etapa5/compras.json`, `10-saldo-insuficiente.png`, `percurso-etapa6/historico.png` |
| Saldo igual ao preço, saldo zero | `percurso-ace04/compras.json`, `05-saldo-zero.png`, `06-cartao-zero.png` |
| Histórico com produto, quantidade e recarga com “+” | `percurso-etapa6/historico.png`, `percurso-ace04/07-historico.png` |
| TypeScript do aplicativo | `percurso-etapa5/tsc.txt` e `percurso-etapa6/tsc.txt`, `exit_code: 0` |

### Propostos, não escritos e não executados

- Saída gravada de `dotnet test backend/CardPlay.sln`.
- `POST /compras` com cartão inexistente e com produto inexistente, guardando o 404.
- Falha de `SaveChanges` no meio da compra (ACE-08). O plano não descreve como provocá-la. Não há teste proposto que esta revisão autorize a inventar.
- Percurso no alerta nativo do celular. As capturas são web.
- JSON de `GET /movimentacoes` ao lado do fuso do navegador, para a hora.

Nenhuma dessas propostas foi implementada aqui.

## Decisões ainda abertas

Registradas em `07-decisoes.md` e no plano. Não viram correção nesta revisão.

- ACE-08 permanece sem modo de falha e sem evidência.
- P1.4, rastro de tentativa, permanece aberto.
- P2.3, preço que muda entre ver e confirmar.
- P2.4, identificação inválida que não seja cartão ou produto inexistente.
- K4, autenticação, estoque, checkout e banco, fora desta história.

## Fora do escopo

- Duas confirmações ao mesmo tempo no mesmo saldo. O plano deixa concorrência fora (I-19). O botão da tela bloqueia outro toque enquanto uma compra está em envio; a API não tem token.
- `Produto.Disponivel`, por decisão já escrita.
- Trocar `number` do TypeScript por `decimal`. O plano registra que o aplicativo já usava `number`. Os preços capturados, 8,50 e 4,00, não exibiram erro de arredondamento.

## Limitações desta revisão

- Não rodou build, teste nem o aplicativo.
- Não reabriu o banco. O log `1.txt` mostra `UPDATE` de saldo e `INSERT` com as colunas de produto, com parâmetros `?`.
- `/tmp/compra-cafe.json` e o log da API em `127.0.0.1:5099`, citados na etapa 4, não estão acessíveis. Os corpos usados aqui são os de `percurso-etapa5` e `percurso-ace04`.
- Ausência de achado confirmado de saldo não é prova de que todo preço do seed ou toda falha de rede se comporta como as capturas.

## Rodada de navegação em 2026-10-08

Primeira tentativa, no mesmo dia, não executou os cenários: o navegador integrado não estava ligado à sessão. A segunda tentativa, com `cursor-ide-browser` disponível, percorreu NAV-01 a NAV-08 em `http://localhost:8081`, com a API em `http://127.0.0.1:5080`.

| Campo | Conteúdo |
| --- | --- |
| Resultado | 8 planejados, 8 executados, 8 passou, 0 falhou, 0 bloqueado, 0 não executados |
| NAV-08 | A primeira medição pela janela manteve duas colunas abaixo de 720 px. Depois da correção, 640 px ficou em uma coluna e 960 px em duas. Saldo R$ 0,00 e “Usar cartão” habilitado |
| Dados | Dois cartões fictícios, titular Titular Navegação: `CP-GK2U8Y` (saldo 1,5) e `CP-RZ3TQ2` (saldo 0,0). Sem reset e sem exclusão |
| Relato | [relatorio.md](evidencias/navegacao/relatorio.md) e [relatorio.html](evidencias/navegacao/relatorio.html). Capturas em `evidencias/navegacao/` |
| Limite | Voltar e avançar do navegador não se aplicam: a URL do documento permaneceu `http://localhost:8081/` entre as abas. A hora exibida, 13:13 a 13:18, coincide com o instante UTC dos JSON, não com o relógio local UTC−3 |

A decisão de aceitar a entrega continua humana.
