# CP-003 — Levantamento de validação

Este arquivo separa código presente, teste que existe no arquivo e comportamento observado. Não executa build, teste nem percurso novo. Não trata o texto de `07-execucao.md` como resultado de comando quando a saída não está em outro arquivo.

Data da leitura: 2026-10-16. Os critérios são os de `03-criterios-de-aceite.md`. O plano é `06-plan.md`. As RN e os CA continuam proposta. Este arquivo não os aprova.

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
| `percurso-etapa4/chamadas.txt` | Cartão A `CP-T6P8MK`: recarga 50, café 8,50, saldo 41,5, cancelamento HTTP 200, saldo 50, estorno 8,5 com `compraId`, compra com data de cancelamento. Repetição HTTP 409 “Esta compra já foi cancelada.”, saldo permanece 50. Cartão B `CP-8GKTTY` pede a compra de A: HTTP 404 “Não há compra deste cartão para cancelar.”, saldos 50 e 10. Id da recarga e id inexistente: o mesmo 404. Saldo final de A: 50 | O par escrito R$ 20,00 / saldo R$ 30,00 / saldo R$ 50,00. Lista de movimentações relida depois do 409. Saída de `dotnet test` |
| `percurso-etapa5/percurso.txt` | `CP-2QZ4PQ`: recarga R$ 20,00, três adesivos a R$ 4,00, total R$ 12,00, saldo R$ 8,00. Aviso com nome, quantidade 3, unitário R$ 4,00, total pago R$ 12,00 e estorno R$ 12,00. Desistir: saldo 8 e duas movimentações. Catálogo da API em 5,0. Confirmar: saldo R$ 20,00, estorno com “+”, compra “Cancelada” sem botão. Preço restaurado; `GET /api/produtos` devolveu 4,0. Fuso `America/Sao_Paulo` | Alerta nativo. Segundo toque depois do sucesso. Saída de `tsc` em arquivo |
| `percurso-etapa5/02-confirmacao-web.png` | Aviso “Confirmar o cancelamento de Adesivo CardPlay, quantidade 3, preço unitário R$ 4,00, total pago R$ 12,00, estorno R$ 12,00?”. Botões Cancelar e Confirmar | O celular nativo |
| `percurso-etapa5/03-historico-apos-estorno.png` | Estorno “Adesivo CardPlay”, linha “Estorno”, “+ R$ 12,00”. Compra “Adesivo CardPlay”, R$ 12,00 sem “+”, quantidade 3, unitário R$ 4,00, linha “Cancelada”, sem botão. Recarga “+ R$ 20,00” | Outro cartão |
| `percurso-etapa5/04-saldo-cartao.png` | `CP-2QZ4PQ`, saldo R$ 20,00 depois do estorno | O saldo de antes do cancelamento |
| `07-execucao.md` | Relato dos filtros 25, 26, 45 e 22, da suíte em 50 e de `tsc` com código 0 | Arquivo próprio com a saída desses comandos |

## Afirmações

### ACE-01 — Cancelar devolve o total e marca a compra

| Campo | Conteúdo |
| --- | --- |
| Critério | Forma geral. O par escrito R$ 20,00 / saldo R$ 30,00 / saldo R$ 50,00 não é o único par válido |
| Estado | Comportamento observado na API e no Histórico web, com totais do seed. Teste no arquivo, execução não confirmada |
| Evidências | `percurso-etapa4/chamadas.txt`, chamadas `05-cancelamento` e `06-movimentacoes-depois`. `percurso-etapa5/percurso.txt`, `03-historico-apos-estorno.png` e `04-saldo-cartao.png`. Código: `Cartao.CancelarCompra` e `CartaoTestes.CancelarCompra_Saldo30AposCompraDe20_Credita20MarcaECriaUmEstorno` |
| Resultado observado | API: saldo 41,5 após café de 8,50, cancelamento devolve 8,50, saldo 50, compra conservada com data de cancelamento e um estorno de 8,5 vinculado. Tela: saldo R$ 8,00 após total R$ 12,00, confirmação devolve R$ 20,00, compra conservada com “Cancelada” e um estorno de R$ 12,00 |
| Limite | O par escrito não foi percorrido. O teste de domínio usa esse par, mas a saída não está em arquivo. O corpo HTTP mostra saldo e estorno juntos; não há log SQL do mesmo `SaveChanges` |
| Pendência | Saída gravada de `dotnet test`, se a suíte precisar ser citada como execução |

### ACE-02 — A confirmação mostra a compra e o valor do estorno

| Campo | Conteúdo |
| --- | --- |
| Critério | ACE-02 |
| Estado | Comportamento observado no aplicativo web |
| Evidências | `percurso-etapa5/02-confirmacao-web.png` e `percurso-etapa5/percurso.txt` |
| Resultado observado | O aviso mostrou Adesivo CardPlay, quantidade 3, preço unitário R$ 4,00, total pago R$ 12,00 e estorno R$ 12,00. Cancelar não alterou o saldo 8 nem criou estorno. A recarga na lista não tem “Cancelar compra”. Depois do sucesso o botão da compra some |
| Limite | Web, com o aviso da própria tela. Não há captura do alerta nativo. Histórico sem cartão no contexto não foi percorrido |
| Pendência | Nenhuma para o gesto web já capturado |

### ACE-03 — Crédito, marca e estorno juntos, com a compra original conservada

| Campo | Conteúdo |
| --- | --- |
| Critério | ACE-03, inclusive a lista do Histórico |
| Estado | Comportamento observado no corpo HTTP e no Histórico web. O `SaveChanges` único está no código. Não há log SQL desta demanda na pasta |
| Evidências | `percurso-etapa4/chamadas.txt`, chamadas `05-cancelamento` e `06-movimentacoes-depois`. `percurso-etapa5/03-historico-apos-estorno.png`. Código: `CompraServico.CancelarAsync` chama `SalvarAlteracoesAsync` uma vez depois de `CancelarCompra` |
| Resultado observado | O estorno tem descrição “Estorno”, o valor da compra e `compraId`. `nomeProduto` do estorno é nulo. A compra conserva nome, quantidade, preço unitário e valor, e ganha data de cancelamento. Na tela a compra fica sem “+” e com “Cancelada”; o estorno fica na mesma lista, com “+” e o nome da compra |
| Limite | A data da API está em UTC. A tela mostra o relógio em `America/Sao_Paulo`. Não há arquivo com `BEGIN` nem com o `UPDATE` e o `INSERT` desse cancelamento |
| Pendência | Nenhuma para o corpo e para o Histórico já capturados |

### ACE-04 — Repetir não gera outro crédito

| Campo | Conteúdo |
| --- | --- |
| Critério | Forma geral. O par escrito saldo R$ 50,00 e estorno de R$ 20,00 não é o único par válido |
| Estado | Recusa e saldo observados na API. A lista não foi relida depois do 409. Na tela o segundo toque não existe porque o botão some. Teste no arquivo, execução não confirmada |
| Evidências | `percurso-etapa4/chamadas.txt`, chamadas `06-movimentacoes-depois`, `07-repeticao` e `08-cartao-a-depois-da-repeticao`. Código: `CancelarCompra_Repetida_NaoCreditaDeNovoNemCriaOutroEstorno` e `CancelarAsync_CompraJaCancelada_NaoGravaDeNovo` |
| Resultado observado | Depois do primeiro cancelamento há um estorno. O segundo POST responde HTTP 409, detalhe “Esta compra já foi cancelada.” O saldo permanece 50. `03-criterios-de-aceite.md` ainda diz que a frase de P2.3 está em aberto; o corpo gravado usa essa frase |
| Limite | Não há `GET /movimentacoes` depois do 409. O par escrito não foi percorrido |
| Pendência | Lista relida depois da repetição, se o “único estorno” precisar ser citado a partir do HTTP. Saída gravada da suíte, se o teste precisar |

### ACE-05 — Quantidade maior que 1 estorna o total uma vez

| Campo | Conteúdo |
| --- | --- |
| Critério | ACE-05 |
| Estado | Comportamento observado na tela e no `GET` do mesmo percurso. Teste no arquivo, execução não confirmada |
| Evidências | `percurso-etapa5/percurso.txt` e `03-historico-apos-estorno.png`. Código: `CancelarCompra_QuantidadeMaiorQueUm_CreditaOTotalUmaVez` e `CancelarAsync_Quantidade3_CreditaOTotalEGravaUmaVez` |
| Resultado observado | Quantidade 3, unitário R$ 4,00, total e estorno R$ 12,00. Um estorno. O crédito não é R$ 4,00 |
| Limite | Um produto do seed. O preço de catálogo diferente entra no ACE-06 |
| Pendência | Nenhuma para esse gesto |

### ACE-06 — Desistir, e preço atual diferente do valor pago

| Campo | Conteúdo |
| --- | --- |
| Critério | Forma geral. O par escrito compra de R$ 20,00 com catálogo a R$ 25,00 não é o único par válido |
| Estado | Comportamento observado no Histórico web |
| Evidências | `percurso-etapa5/percurso.txt` e `02-confirmacao-web.png` |
| Resultado observado | Desistir manteve saldo 8, compra sem data de cancelamento e nenhuma movimentação de estorno. Com o adesivo a 5,0 na API, o aviso e o crédito usaram o total gravado de R$ 12,00. O catálogo já aberto na tela não buscou a lista de novo e continuou mostrando R$ 4,00 |
| Limite | O par escrito não foi percorrido. A diferença de preço foi lida em `GET /api/produtos`, não num card atualizado |
| Pendência | Nenhuma para a desistência e para o total gravado já capturados |

### ACE-07 — Compra de outro cartão ou inexistente

| Campo | Conteúdo |
| --- | --- |
| Critério | ACE-07 |
| Estado | Recusa e saldos observados na API. A compra de A já estava cancelada quando B pediu o cancelamento. Teste no arquivo, execução não confirmada |
| Evidências | `percurso-etapa4/chamadas.txt`, chamadas `11-compra-de-a-pedida-por-b` a `16-cartao-a-final`. Código: `CancelarAsync_CompraDeOutroCartao_NaoGravaNenhumDosDois`, `CancelarAsync_IdAusente_NaoGrava`, `CancelarAsync_Recarga_NaoGrava` e `CancelarCompra_IdAusenteOuRecarga_NaoAlteraOCartao` |
| Resultado observado | Pedido de B, id da recarga e id inexistente responderam HTTP 404, detalhe “Não há compra deste cartão para cancelar.” Saldos permaneceram 50 e 10. `03-criterios-de-aceite.md` ainda diz que a frase de P2.1 está em aberto; o corpo gravado usa essa frase |
| Limite | O 404 de B não isola “a compra não fica marcada”: a marca já existia desde o cancelamento de A. Não há estorno novo nesse trecho, e também não há lista relida |
| Pendência | Percurso em que a compra ainda não foi cancelada quando o outro cartão pede, se a marca intacta precisar ser citada a partir do HTTP. Saída gravada da suíte, se o teste precisar |

### ACE-08 — Compra já cancelada

| Campo | Conteúdo |
| --- | --- |
| Critério | ACE-08. O efeito de saldo é o do ACE-04 |
| Estado | O mesmo HTTP 409 e o mesmo saldo do ACE-04. A lista com um estorno é a de antes da repetição |
| Evidências | As do ACE-04 |
| Resultado observado | A compra já tinha data de cancelamento e um estorno. O segundo pedido não creditou: o saldo permaneceu 50 |
| Limite | O mesmo limite do ACE-04 sobre a lista depois do 409 |
| Pendência | A mesma pendência do ACE-04 |

### ACE-09 — Falha ao gravar

| Campo | Conteúdo |
| --- | --- |
| Critério | ACE-09 |
| Estado | Código presente. Teste no arquivo, execução não confirmada. Não há interruptor no produto |
| Evidências | `CompraServico.CancelarAsync` transforma falha de `SalvarAlteracoesAsync` em `CancelamentoNaoConcluidoException`, mensagem “O cancelamento não foi concluído.” `FalhaGravacaoSqliteTestes.CancelarAsync_FalhaAoSalvar_RelendoSqlite_MantemSaldoSemMarcaESemEstorno` afirma saldo gravado 96, compra sem data de cancelamento e nenhum `CompraId`. O tratador não mapeia essa exceção |
| Resultado observado | Nenhum. `07-execucao.md` relata 22 aprovados nesse filtro e 50 na suíte; essa saída não está em outro arquivo. Não há chamada HTTP desta falha |
| Limite | O teste usa outro contexto SQLite. Não há captura da frase na tela nem do HTTP 500 |
| Pendência | Saída gravada de `dotnet test` desse método, se a execução precisar ser citada |

## Fora deste levantamento

Estorno parcial, chargeback, taxa, prazo, autenticação, autorização de produção e a CP-004 continuam fora dos critérios. `README.md` e `docs/architecture.md` não descrevem o cancelamento e não são critério deste arquivo. Comprar, recarregar e “Cancelar” no aviso de “Usar cartão” não foram recapturados aqui.
