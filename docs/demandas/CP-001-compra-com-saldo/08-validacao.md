# CP-001 — Levantamento de validação

Este arquivo separa código presente, compilação com saída gravada, teste que existe no arquivo e comportamento observado. Não executa build, teste nem percurso novo. Não trata o texto de `07-execucao.md` como resultado de comando quando a saída não está em outro arquivo.

Data da leitura: 2026-10-08. Os critérios são os de `03-criterios-de-aceite.md`. O plano é `06-plan.md`.

## Como ler o estado

| Estado | Significado neste arquivo |
| --- | --- |
| Código presente | O diff ou o arquivo fonte contém a regra. Isso não executa a regra |
| Compilação gravada | Há arquivo com o comando e o código de saída. Compilar não prova saldo nem registro |
| Teste no arquivo, execução não confirmada | O método de teste existe. Não há saída de `dotnet test` posterior a esses métodos |
| Comportamento observado | Captura, log ou corpo HTTP localizável mostra o resultado daquele cenário |
| Não verificado | Não há artefato que mostre o resultado |

## Evidências localizáveis

| Artefato | O que contém | O que não contém |
| --- | --- | --- |
| `percurso-etapa5/compras.json` | `POST /compras` 200 do cartão `76066d9e-9442-462b-87da-eceb478d3be7`: saldo 1,5, café, valor 8,5, quantidade 1. Outro `POST` 400, `detail` “Saldo insuficiente para concluir a compra.” | Saldo anterior no JSON. Corpo de 404 |
| `percurso-etapa5/log.txt` | Percurso do cartão CP-QBDG8R: recarga R$ 10,00, preço do café visível, cancelar sem `POST /compras`, confirmar, saldo R$ 1,50, recusa do adesivo | Fuso da hora exibida |
| `percurso-etapa5/04-catalogo-saldo-10.png`, `05-alerta-cafe.png`, `06-cancelou.png`, `07-compra-cafe.png`, `08-cartao-apos-compra.png`, `09-alerta-adesivo.png`, `10-saldo-insuficiente.png` | Catálogo, aviso, saldo depois do café e mensagem de saldo insuficiente | O id da movimentação na tela |
| `percurso-etapa5/11-historico.png` | Histórico anterior à etapa 6: “Compra” com “+ R$ 8,50” e “Recarga” com “+ R$ 10,00” | O desenho atual do histórico |
| `percurso-etapa5/tsc.txt` | `node node_modules/typescript/bin/tsc --noEmit` em `mobile/`, `exit_code: 0`, 2026-10-07T19:45:08Z | Cenário de compra |
| `percurso-etapa6/historico.png` e `texto.txt` | Aba Histórico: “Café especial”, “R$ 8,50”, “Quantidade 1”, “07/10/2026, 19:45”; “Recarga”, “+ R$ 10,00” | Número do cartão na imagem. `texto.txt` inclui também o texto da aba Cartão, CP-QBDG8R |
| `percurso-etapa6/tsc.txt` | O mesmo `tsc`, `exit_code: 0`, 2026-10-07T19:50:12Z | Cenário de compra |
| `percurso-ace04/compras.json` | `POST /compras` 200 do cartão `518814c5-f699-4db0-af64-a52965500247`: saldo 0,0, “Adesivo CardPlay”, valor 4,0, quantidade 1 | Saldo anterior no JSON |
| `percurso-ace04/log.txt` e `03-catalogo-saldo-igual.png`, `04-alerta-adesivo.png`, `05-saldo-zero.png`, `06-cartao-zero.png`, `07-historico.png` | CP-NYL59G, saldo R$ 4,00 igual ao adesivo, confirmação, saldo R$ 0,00 no catálogo e no cartão, histórico com a compra sem “+” e a recarga com “+ R$ 4,00” | Outro produto no limite de saldo zero |
| `percurso-etapa5` não cita `npx tsc` com sucesso | O terminal da tentativa `npx tsc --noEmit` termina com `exit_code: 1` e `FETCH_ERROR` antes de compilar | Falha do código TypeScript |
| Log da API em execução, terminal `1.txt`, 2026-10-08 | “No migrations were applied. The database is already up to date.” Um `UPDATE "Cartoes" SET "Saldo"` seguido de `INSERT INTO "MovimentacoesCartao"` com `NomeProduto`, `ProdutoId` e `Quantidade`. Os valores dos parâmetros estão como `?`. `Descricao` aparece com tamanho 6 e `NomeProduto` com tamanho 16 | O texto dos parâmetros, o status HTTP e o id do cartão |
| `07-execucao.md`, resolução da etapa 2 | Relato de `Build succeeded` e `Passed! 16`. Não há outro arquivo com essa saída | Prova independente do comando. 16 testes é anterior a `CompraServicoTestes`, segundo a própria revisão da etapa 3 |
| `/tmp/compra-cafe.json` e o log da API em `127.0.0.1:5099` | Citados na revisão da etapa 4 | Não estão neste repositório nem em `/tmp` nesta leitura |

## Afirmações

### ACE-01 — Saldo maior que o preço debita o preço do catálogo e gera um registro

| Campo | Conteúdo |
| --- | --- |
| Critério | ACE-01, forma geral. O par R$ 50 / R$ 20 / R$ 30 não é meta |
| Estado | Comportamento observado num cartão e num produto. Código presente para a regra geral. Teste no arquivo, execução não confirmada |
| Evidências | `percurso-etapa5/compras.json` corpo 200; `07-compra-cafe.png`; `08-cartao-apos-compra.png`. Código: `Cartao.Comprar` e `CartaoTestes.Comprar_SaldoMaiorQuePreco_DebitaPrecoERegistraCompra` |
| Resultado observado | Recarga de R$ 10,00, café do seed a R$ 8,50, saldo final 1,5 no JSON e “R$ 1,50” na aba Cartão. Registro com nome “Café especial”, valor 8,5, quantidade 1, `produtoId` `6f1c2a7e-0c4a-4b1d-9e2f-1a2b3c4d5e6f` |
| Limite | Um preço do seed e um saldo inicial. O JSON não traz o saldo anterior; o saldo anterior está no log e nas capturas do mesmo percurso. Não há saída de `dotnet test` desse método |
| Pendência | Guardar a saída de `dotnet test backend/CardPlay.sln` se a suíte precisar ser citada como execução |

### ACE-02 — Confirmar só depois de ver produto e preço

| Campo | Conteúdo |
| --- | --- |
| Critério | ACE-02 |
| Estado | Comportamento observado no aplicativo web |
| Evidências | `04-catalogo-saldo-10.png`, `05-alerta-cafe.png`, `percurso-etapa5/log.txt` (“compras HTTP após cancelar: 0”). No limite de saldo igual: `percurso-ace04/03-catalogo-saldo-igual.png` e `04-alerta-adesivo.png` |
| Resultado observado | O café a R$ 8,50 e o saldo R$ 10,00 estão na tela antes do aviso “Confirmar a compra de Café especial por R$ 8,50?”. Cancelar não chama `/compras`. O mesmo ocorre com o adesivo a R$ 4,00 quando o saldo também é R$ 4,00 |
| Limite | Web, com o aviso da própria tela. Não há captura do alerta nativo do celular |
| Pendência | Nenhuma para o gesto web já capturado. P2.3, mudança de preço entre ver e confirmar, continua sem cenário |

### ACE-03 — Débito e registro juntos

| Campo | Conteúdo |
| --- | --- |
| Critério | ACE-03 |
| Estado | Código presente. Comportamento observado na resposta HTTP e, em parte, no log SQL. A transação única não está impressa |
| Evidências | `CompraServico.ComprarAsync` chama `SalvarAlteracoesAsync` uma vez depois de `Comprar` e `AdicionarMovimentacao`. `percurso-etapa5/compras.json` e `percurso-ace04/compras.json` devolvem saldo e registro no mesmo corpo. Terminal `1.txt` mostra `UPDATE "Cartoes" SET "Saldo"` e, em seguida, `INSERT` com as colunas de produto |
| Resultado observado | Os dois corpos 200 trazem saldo e registro juntos. O log SQL grava saldo e movimentação na mesma sequência, com parâmetros ocultos |
| Limite | O log não mostra `BEGIN` nem o valor debitado. Não prova, sozinho, que os dois comandos são o café de R$ 8,50 ou o adesivo de R$ 4,00. Tamanho 6 em `Descricao` e tamanho 16 em `NomeProduto` são compatíveis com “Compra” e “Adesivo CardPlay”, sem o texto no log |
| Pendência | Não há teste de repositório com SQLite cuja saída esteja gravada |

### ACE-04 — Saldo igual ao preço termina em zero

| Campo | Conteúdo |
| --- | --- |
| Critério | ACE-04 |
| Estado | Comportamento observado para o adesivo de R$ 4,00. Teste no arquivo, execução não confirmada |
| Evidências | `percurso-ace04/compras.json`; `03-catalogo-saldo-igual.png`; `05-saldo-zero.png`; `06-cartao-zero.png`; `07-historico.png`. Código: `Comprar_SaldoIgualAoPreco_ConcluiComSaldoZero` e `ComprarAsync_SaldoIgualAoPreco_GravaComSaldoZero` |
| Resultado observado | Cartão CP-NYL59G, saldo R$ 4,00, adesivo R$ 4,00, resposta com saldo 0,0, registro do adesivo, quantidade 1, valor 4,0. Catálogo e aba Cartão mostram R$ 0,00. O histórico tem essa compra e uma recarga de R$ 4,00 |
| Limite | Um produto do seed. A revisão da etapa 4 dizia que não havia corpo com saldo zero; esse corpo passou a existir em `percurso-ace04/compras.json`. `/tmp/compra-cafe.json`, citado antes, não está acessível |
| Pendência | Saída gravada da suíte, se for preciso citar o teste como execução |

### ACE-05 — Saldo final maior ou igual a zero

| Campo | Conteúdo |
| --- | --- |
| Critério | ACE-05, nos caminhos de ACE-01, ACE-04 e ACE-06 |
| Estado | Comportamento observado nesses três percursos. Não é prova para um caminho não capturado |
| Evidências | Saldos 1,5 e 0,0 nos corpos 200. `10-saldo-insuficiente.png` permanece em R$ 1,50 depois do 400 |
| Resultado observado | Nenhum desses artefatos mostra saldo negativo |
| Limite | Três cenários capturados. Não há varredura de outros preços |
| Pendência | Nenhuma além dos cenários que ACE-06 e ACE-08 ainda não fecham por completo |

### ACE-06 — Saldo menor que o preço

| Campo | Conteúdo |
| --- | --- |
| Critério | ACE-06 |
| Estado | Comportamento observado para um adesivo de R$ 4,00 com saldo R$ 1,50. Teste no arquivo, execução não confirmada |
| Evidências | `percurso-etapa5/compras.json` corpo 400; `10-saldo-insuficiente.png`; `percurso-etapa6/historico.png`, que continua com uma compra de café e uma recarga, sem adesivo. Código: `Comprar_SaldoMenorQuePreco_NaoAlteraSaldoNemRegistraCompra` |
| Resultado observado | HTTP 400, `detail` “Saldo insuficiente para concluir a compra.” A tela mostra essa frase e “Saldo atual: R$ 1,50”. O histórico posterior não tem segunda compra |
| Limite | Um par saldo/preço. A captura não relê o banco; a ausência da segunda compra está na tela e no fato de o JSON de sucesso desse cartão ter um único corpo 200 |
| Pendência | Saída gravada da suíte para o teste que afirma que `SalvarAlteracoesAsync` não foi chamado |

### ACE-07 — Cartão ou produto ausente

| Campo | Conteúdo |
| --- | --- |
| Critério | ACE-07. P2.4, cartão ausente no aplicativo, ficou de fora |
| Estado | Código presente. Teste no arquivo, execução não confirmada. Comportamento HTTP não verificado |
| Evidências | `TratadorExcecoes` envia `CartaoNaoEncontradoException` e `ProdutoNaoEncontradoException` para HTTP 404, título “Recurso não encontrado”. `CompraServicoTestes` tem `ComprarAsync_CartaoAusente_NaoGrava` e `ComprarAsync_ProdutoAusente_NaoGravaEMantemSaldo`. Nenhum corpo 404 está em `percurso-etapa5/` nem em `percurso-ace04/` |
| Resultado observado | Nenhum |
| Limite | O código usa duas frases de `detail`, uma por exceção. O critério pede informação de dado inválido; não exige a mesma frase. Isso está no código, não numa resposta gravada |
| Pendência | Gravar `POST /compras` com cartão inexistente e com produto inexistente, com o status e o `detail` |

### ACE-08 — Falha ao efetivar débito ou registro

| Campo | Conteúdo |
| --- | --- |
| Critério | ACE-08. P1.4 não exige rastro de tentativa |
| Estado | Não verificado. A etapa 8 não tem diff de código |
| Evidências | `07-decisoes.md`, seção “Etapa 8 sem diff”. `06-plan.md` diz que não há alteração especificável. `Cartao.Comprar` lança saldo insuficiente antes de debitar. `CompraServico` lança cartão ou produto ausente antes de `SalvarAlteracoesAsync`. `TratadorExcecoes` usa HTTP 500 para exceção não nomeada |
| Resultado observado | Nenhum para falha de gravação |
| Limite | As recusas já capturadas não são um `SaveChanges` que falha. HTTP 500 está no código e não numa resposta gravada |
| Pendência | Decidir se o critério permanece sem evidência ou se um modo de falha será descrito. Sem esse modo, não há teste nem chamada que demonstre ACE-08 |

## Compilação e suíte

| Afirmação | Estado | Evidência | Limite |
| --- | --- | --- | --- |
| O aplicativo TypeScript compilou depois da etapa 5 | Compilação gravada | `percurso-etapa5/tsc.txt`, `exit_code: 0` | Não executa a compra. O `npx tsc --noEmit` com `exit_code: 1` falhou ao buscar o pacote, antes de compilar |
| O aplicativo compilou de novo com a tela de histórico | Compilação gravada | `percurso-etapa6/tsc.txt`, `exit_code: 0` | Não é a consulta; a consulta está na captura da mesma pasta |
| A solution compilou e 16 testes passaram | Não confirmado como arquivo de comando | Só o parágrafo em `07-execucao.md` | Não há log do comando. A contagem 16 é anterior aos testes de `CompraServico`, pelo texto da revisão da etapa 3 |
| A suíte atual, com os oito métodos `Comprar` e `ComprarAsync`, passou | Não executado neste levantamento | Os métodos estão em `CartaoTestes.cs` e `CompraServicoTestes.cs` | Existir o método não é resultado |

## O que o diff cobre e o que a evidência alcança

| Mudança | Critério | Até onde a evidência vai |
| --- | --- | --- |
| `Cartao.Comprar`, exceção de saldo insuficiente, quantidade 1 | ACE-01, ACE-03, ACE-04, ACE-05, ACE-06 | Código e testes no arquivo. Observado na API e na tela para café 8,50 e adesivo 4,00 |
| Colunas `NomeProduto`, `ProdutoId`, `Quantidade` | ACE-03 | Migration no código. Log `1.txt` grava essas colunas com parâmetros `?`. “No migrations were applied” nesse log não mostra o `Up` sendo aplicado nesta subida |
| `POST /api/cartoes/{id}/compras` | ACE-01, ACE-04, ACE-06 | Dois corpos 200 e um 400 gravados. 404 não gravado |
| Confirmação no catálogo e saldo do contexto | ACE-02, ACE-01, ACE-04, ACE-06 | Capturas web |
| Histórico com nome, quantidade e valor sem “+” | ACE-01 e ACE-03 na consulta; P2.5 e P3.1 aceitos em `07-decisoes.md` | `percurso-etapa6/historico.png` e `percurso-ace04/07-historico.png`. A captura `percurso-etapa5/11-historico.png` ainda mostra “+” e não serve para o desenho atual |
| Documentos de produto, README, AGENTS e backlog | K2, K5, I-17 | Texto alterado. Não é execução. `docs/backlog.md` marca o item como atendido e o cabeçalho ainda diz “Demandas futuras” e “Não implementar até serem pedidas” |
| Etapa 8 | ACE-08 | Sem diff de produto. Critério sem observação |

## Contradições

- A revisão da etapa 4 registra que não havia corpo de saldo zero nem de saldo insuficiente. `percurso-ace04/compras.json` e `percurso-etapa5/compras.json` passaram a ter esses corpos. O arquivo `/tmp/compra-cafe.json` citado na etapa 4 não está acessível.
- `percurso-etapa5/11-historico.png` mostra a compra com “+”. `percurso-etapa6/historico.png` mostra a mesma compra de café sem “+”. São momentos diferentes. Só a segunda sustenta o histórico da etapa 6.
- `07-execucao.md` afirma `Passed! 16` e `Build succeeded`. Não há saída do comando fora desse parágrafo. Esse número não inclui os testes de compra da orquestração.
- O log `1.txt` diz que nenhuma migration foi aplicada e, no mesmo processo, lê e grava `NomeProduto`, `ProdutoId` e `Quantidade`. Isso indica banco já migrado, não a aplicação do `Up` nessa subida, e não mostra os valores.

## Cenários sem validação

- `dotnet test` e `dotnet build` com saída gravada depois dos testes de compra.
- HTTP 404 de cartão ausente e de produto ausente.
- ACE-08, falha de gravação no meio do débito ou do registro.
- P1.4, rastro de tentativa.
- Alerta nativo no celular. As capturas são do aplicativo web.
- Fuso da hora “19:45” em relação ao instante UTC dos JSON.
- Preço do catálogo alterado entre a visualização e a confirmação (P2.3).
- Identificação inválida além de cartão ou produto inexistente (P2.4).
