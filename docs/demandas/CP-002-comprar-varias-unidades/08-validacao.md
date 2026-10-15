# CP-002 — Levantamento de validação

Este arquivo separa código presente, teste que existe no arquivo e comportamento observado. Não executa build, teste nem percurso novo. Não trata o texto de `07-execucao.md` como resultado de comando quando a saída não está em outro arquivo.

Data da leitura: 2026-10-15. Os critérios são os de `03-criterios-de-aceite.md`. O plano é `06-plan.md`.

## Como ler o estado

| Estado | Significado neste arquivo |
| --- | --- |
| Código presente | O arquivo fonte contém a regra. Isso não executa a regra |
| Teste no arquivo, execução não confirmada | O método de teste existe. Não há arquivo com a saída de `dotnet test` |
| Comportamento observado | Captura, log ou corpo HTTP localizável mostra o resultado daquele cenário |
| Não verificado | Não há artefato que mostre o resultado |

## Evidências localizáveis

| Artefato | O que contém | O que não contém |
| --- | --- | --- |
| `percurso-etapa4/chamadas.txt` | Café, quantidade 3, saldo 100 para 74,5, unitário 8,5, valor 25,5, HTTP 200. Adesivo, quantidade 3, saldo 12 para 0, HTTP 200. Adesivo, quantidade 3, saldo 10, HTTP 400 “Saldo insuficiente para concluir a compra.”, saldo permanece 10. Quantidades 0, −1 e 1,5, HTTP 400 “Informe uma quantidade inteira maior que zero.”, saldo permanece 20. Preço visto 3 contra adesivo de 4, HTTP 409, saldo permanece 20; confirmação seguinte com 4 conclui e o saldo vai a 16 | O par escrito R$ 20,00 × 3. Estouro de `decimal`. Saída de `dotnet test` |
| `percurso-etapa5/log.txt` | CP-NRBG9K, recarga R$ 100,00. Café, quantidade 3: cancelar sem `POST /compras`; confirmar, saldo R$ 74,50. Fone, quantidade 3, total R$ 237,00, “Saldo insuficiente para concluir a compra.”, saldo R$ 74,50. Adesivo, quantidade 1, unitário e total R$ 4,00, saldo R$ 70,50. `GET /movimentacoes`: recarga 100, café quantidade 3 unitário 8,5 valor 25,5, adesivo quantidade 1 unitário 4 valor 4. Sem compra do fone | Compilação do aplicativo em arquivo. Preço 409 na tela |
| `percurso-etapa5/01-alerta-cafe.png` | Aviso “Confirmar a compra de Café especial, quantidade 3, preço unitário R$ 8,50, total R$ 25,50?”. Saldo atual R$ 100,00. Botões Cancelar e Confirmar | O celular nativo |
| `percurso-etapa6/log.txt` e `historico.png` | Café R$ 25,50 sem “+”, quantidade 3, preço unitário R$ 8,50. Adesivo R$ 4,00 sem “+”, quantidade 1, preço unitário R$ 4,00. Recarga “+ R$ 100,00”, sem quantidade nem preço unitário | Outro cartão |
| `07-execucao.md` | Relato dos totais 20, 28, 35 e 36 e de `tsc` com código 0 | Arquivo próprio com a saída desses comandos |

## Afirmações

### ACE-01 — Várias unidades debitam o total e geram um registro

| Campo | Conteúdo |
| --- | --- |
| Critério | Forma geral. O par escrito R$ 100,00 / R$ 20,00 / quantidade 3 / saldo R$ 40,00 não é o único par válido |
| Estado | Comportamento observado na API e no catálogo web, com o café do seed. Teste no arquivo, execução não confirmada |
| Evidências | `percurso-etapa4/chamadas.txt`, chamada `03-compra-3-cafe`. `percurso-etapa5/log.txt`. Código: `Cartao.Comprar` e `CartaoTestes.Comprar_Quantidade3_SaldoMaiorQueTotal_DebitaTotalERegistraUmaCompra` |
| Resultado observado | Saldo 100, café R$ 8,50, quantidade 3, total 25,5, saldo final 74,5. Um registro com quantidade 3, preço unitário 8,5 e valor 25,5. Na tela, o mesmo cartão CP-NRBG9K foi de R$ 100,00 para R$ 74,50 |
| Limite | O par R$ 20,00 × 3 não foi percorrido. O teste de domínio usa R$ 20,00 e espera saldo 40, mas a saída não está em arquivo |
| Pendência | Saída gravada de `dotnet test`, se a suíte precisar ser citada como execução |

### ACE-02 — A confirmação mostra quantidade, unitário e total

| Campo | Conteúdo |
| --- | --- |
| Critério | ACE-02 |
| Estado | Comportamento observado no aplicativo web |
| Evidências | `percurso-etapa5/01-alerta-cafe.png` e `percurso-etapa5/log.txt` |
| Resultado observado | O aviso do café mostrou produto, quantidade 3, preço unitário R$ 8,50 e total R$ 25,50. Cancelar não chamou `POST /compras` e o saldo continuou R$ 100,00. O fone mostrou quantidade 3, unitário R$ 79,00 e total R$ 237,00. O adesivo, quantidade 1, mostrou unitário e total R$ 4,00 |
| Limite | Web, com o aviso da própria tela. Não há captura do alerta nativo do celular. O botão sem cartão não foi percorrido de novo nesta demanda |
| Pendência | Nenhuma para o gesto web já capturado |

### ACE-03 — Débito e um registro juntos

| Campo | Conteúdo |
| --- | --- |
| Critério | ACE-03, inclusive permanecer no catálogo |
| Estado | Comportamento observado no corpo HTTP e no catálogo. O `SaveChanges` único está no código. Não há log SQL desta demanda na pasta |
| Evidências | `percurso-etapa4/chamadas.txt`, corpos 200. `percurso-etapa5/log.txt`: “Compra registrada. Saldo: R$ 74,50” na mesma tela do catálogo. Código: `CompraServico.ComprarAsync` chama `SalvarAlteracoesAsync` uma vez depois de `Comprar` |
| Resultado observado | Os corpos 200 trazem saldo e registro no mesmo JSON, com quantidade, preço unitário e valor total. Depois do café, o catálogo continuou aberto com o saldo novo |
| Limite | Não há arquivo com `BEGIN` nem com o `UPDATE` e o `INSERT` dessa compra |
| Pendência | Nenhuma para o corpo e para a permanência no catálogo já capturados |

### ACE-04 — Saldo exatamente igual ao total

| Campo | Conteúdo |
| --- | --- |
| Critério | Forma geral. O par escrito R$ 60,00 / R$ 20,00 / quantidade 3 não é o único par válido |
| Estado | Comportamento observado na API. Não percorrido na tela. Teste no arquivo, execução não confirmada |
| Evidências | `percurso-etapa4/chamadas.txt`, chamada `06-compra-3-adesivo-zera`. Código: `Comprar_Quantidade3_SaldoIgualAoTotal_ConcluiComSaldoZero` |
| Resultado observado | Recarga 12, adesivo R$ 4,00, quantidade 3, HTTP 200, saldo 0, quantidade 3, preço unitário 4, valor 12 |
| Limite | Um produto do seed. A tela não mostrou saldo zero |
| Pendência | Percurso no catálogo com saldo igual ao total, se a tela precisar ser citada. Saída gravada da suíte, se o teste precisar |

### ACE-05 — O saldo não fica negativo

| Campo | Conteúdo |
| --- | --- |
| Critério | ACE-05, nas tentativas já capturadas |
| Estado | Comportamento observado nos percursos gravados. Não há captura de saldo negativo |
| Evidências | Saldos finais em `percurso-etapa4/chamadas.txt` e `percurso-etapa5/log.txt`: 74,5, 0, 10, 20, 16, R$ 74,50 e R$ 70,50 |
| Resultado observado | Nas compras concluídas e nas recusas gravadas, o saldo lido depois é maior ou igual a zero |
| Limite | Só esses percursos. ACE-10 não tem execução gravada, então o saldo dessa falha não entra aqui |
| Pendência | Nenhuma para os percursos já gravados |

### ACE-06 — Saldo menor que o total

| Campo | Conteúdo |
| --- | --- |
| Critério | Forma geral. O par escrito R$ 50,00 contra total R$ 60,00 não é o único par válido |
| Estado | Comportamento observado na API e na tela |
| Evidências | `percurso-etapa4/chamadas.txt`, chamadas `09-compra-saldo-insuficiente` e `10-saldo-apos-insuficiente`. `percurso-etapa5/log.txt` |
| Resultado observado | API: saldo 10, adesivo quantidade 3, total 12, HTTP 400, detalhe “Saldo insuficiente para concluir a compra.”, saldo permanece 10. Tela: fone quantidade 3, total R$ 237,00, a mesma frase, saldo permanece R$ 74,50 e o `GET` posterior não tem compra do fone |
| Limite | O par R$ 50,00 / R$ 60,00 não foi percorrido |
| Pendência | Nenhuma para os dois pares já gravados |

### ACE-07 — Quantidade inválida ou total que não cabe

| Campo | Conteúdo |
| --- | --- |
| Critério | Quantidades 0, −1 e 1,5 na API. Estouro de `decimal` à parte |
| Estado | Quantidade inválida observada na API. Estouro: código presente e teste no arquivo, execução não confirmada. Estouro não foi chamado na API |
| Evidências | `percurso-etapa4/chamadas.txt`, chamadas `13-quantidade-0`, `14-quantidade-menos-1`, `15-quantidade-1-5` e `16-saldo-apos-quantidade`. Código: `EstouroDeTotalException` e `CartaoTestes.Comprar_TotalEstouraDecimal_NaoAlteraSaldoNemRegistraCompra` |
| Resultado observado | As três quantidades responderam HTTP 400, detalhe “Informe uma quantidade inteira maior que zero.” O saldo permaneceu 20 |
| Limite | Não há corpo HTTP do estouro. O teste de estouro não tem saída gravada |
| Pendência | Chamada HTTP, ou saída gravada do teste, para o estouro |

### ACE-08 — Quantidade 1 mantém o fluxo da CP-001

| Campo | Conteúdo |
| --- | --- |
| Critério | ACE-08, com unitário e total iguais ao preço |
| Estado | Comportamento observado na tela e no `GET` do mesmo percurso. Também há um HTTP 200 de quantidade 1 na etapa 4 |
| Evidências | `percurso-etapa5/log.txt`. `percurso-etapa4/chamadas.txt`, chamada `19-compra-preco-vigente`. `percurso-etapa6/historico.png` |
| Resultado observado | Adesivo, quantidade 1, aviso com unitário e total R$ 4,00, saldo de R$ 74,50 para R$ 70,50. O histórico mostra o adesivo sem “+”, quantidade 1 e preço unitário R$ 4,00, na mesma lista da recarga. O botão continua “Usar cartão” |
| Limite | Um produto do seed, na web |
| Pendência | Nenhuma para esse gesto |

### ACE-09 — Preço mudou entre a apresentação e a confirmação

| Campo | Conteúdo |
| --- | --- |
| Critério | ACE-09 |
| Estado | Comportamento observado na API. Não percorrido na tela |
| Evidências | `percurso-etapa4/chamadas.txt`, chamadas `17-preco-diferente`, `18-saldo-apos-409` e `19-compra-preco-vigente` |
| Resultado observado | Preço visto 3 contra adesivo de 4: HTTP 409, título “Conflito”, detalhe “O preço deste produto mudou. Confirme novamente o total atualizado.” Saldo permanece 20. A confirmação seguinte, com preço 4 e quantidade 1, conclui e o saldo vai a 16 |
| Limite | O preço diferente foi enviado no corpo. O catálogo não mostrou um preço e depois outro |
| Pendência | Percurso na tela, se o 409 precisar aparecer no aviso do aplicativo |

### ACE-10 — Falha ao gravar

| Campo | Conteúdo |
| --- | --- |
| Critério | ACE-10 |
| Estado | Código presente. Teste no arquivo, execução não confirmada. Não há interruptor no produto |
| Evidências | `CompraServico.ComprarAsync` transforma falha de `SalvarAlteracoesAsync` em `CompraNaoConcluidaException`, mensagem “A compra não foi concluída.” `CompraServicoTestes.ComprarAsync_FalhaAoSalvar_InformaQueNaoConcluiuENaoGrava` afirma saldo gravado 100, salvar não concluído e compra fora das movimentações gravadas |
| Resultado observado | Nenhum. `07-execucao.md` relata 36 aprovados; essa saída não está em outro arquivo |
| Limite | O teste é em memória. Não há chamada HTTP desta falha |
| Pendência | Saída gravada de `dotnet test` desse método, se a execução precisar ser citada |

## Fora deste levantamento

Carrinho, estoque, desconto, frete, parcelamento, banco e autenticação continuam fora dos critérios. Cartão inexistente e produto inexistente continuam o 404 da CP-001 e não foram recapturados aqui.
