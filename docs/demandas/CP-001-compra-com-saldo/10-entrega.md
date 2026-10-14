# CP-001 — Entrega: Compra com saldo

## Estado deste documento

| Campo | Valor |
| --- | --- |
| Demanda | CP-001 — Compra de produto com saldo CardPlay |
| Artefato | Síntese para revisão humana |
| Fontes | `02-historia-rica.md`, `03-criterios-de-aceite.md`, `06-plan.md`, `07-decisoes.md`, `07-execucao.md`, `08-validacao.md`, `09-review-e-testes.md`, `evidencias/navegacao/relatorio.md` |
| Código comparado | Branch `docs/cp-001-compra-com-saldo`, commit `4a199f2`, mais a alteração não commitada em `mobile/src/telas/TelaCatalogo.tsx` |
| Data da leitura | 2026-10-08 |
| Situação | **Não concluída.** Há critérios sem evidência suficiente |

Este arquivo não executa validação nova e não altera o produto. “Observado” abaixo significa artefato já gravado. Teste escrito sem saída de `dotnet test` não conta como execução.

---

## Problema

O cliente recarrega o cartão, consulta saldo e histórico e vê o catálogo, e não conseguia comprar um produto com esse saldo.

A história recebida pede comprar um produto do catálogo com o saldo do cartão, para usar os créditos e acompanhar o valor gasto (`02-historia-rica.md`). As regras de origem, ainda como proposta nesse artefato, são:

- uma unidade de um único produto, pelo preço cadastrado no catálogo (RN01);
- identificar o cartão, selecionar o produto, ver o produto e o valor, e só então confirmar (RN02);
- cartão e produto existentes, saldo maior ou igual ao preço, saldo final não negativo (RN03);
- débito e registro juntos, com produto, quantidade, valor, data e hora (RN04).

Carrinho, múltiplas unidades, cancelamento e estorno ficam fora da proposta. O par R$ 50,00 / R$ 20,00 / R$ 30,00 é o exemplo de CA01, não uma segunda regra. O plano não cria produto a R$ 20,00: o seed não tem esse preço (`06-plan.md`).

`07-decisoes.md` registra que RN01–RN04 e ACE-01–ACE-08 valem como escritos e que a CP-001 refina o item “Utilizar saldo em produtos”. Várias dessas respostas foram escritas no mesmo diff do código, não numa autorização anterior (`07-execucao.md`). O plano, na publicação, dizia que o conjunto não estava pronto para autorizar código.

---

## Solução

A compra adotada percorre o mesmo caminho da recarga: regra no domínio, colunas na movimentação, orquestração na Application, HTTP na API, confirmação no catálogo e leitura na aba Histórico. `CardPlay.Services` não entra.

1. `Cartao.Comprar` recusa produto nulo e, se `Saldo < produto.Preco`, lança `SaldoInsuficienteException` antes de mudar saldo e antes de criar movimentação. No outro caminho, subtrai o preço e acrescenta a movimentação no mesmo método (`backend/src/CardPlay.Domain/Entidades/Cartao.cs`).
2. `MovimentacaoCartao.CriarCompra` grava descrição “Compra”, `ProdutoId`, `NomeProduto`, quantidade 1, o valor e `DateTime.UtcNow`. A recarga deixa produto, nome e quantidade vazios (`MovimentacaoCartao.cs`).
3. `CompraServico.ComprarAsync` busca cartão e produto. Cartão ausente ou produto ausente lança antes de gravar. No sucesso, chama `Comprar`, `AdicionarMovimentacao` e um `SalvarAlteracoesAsync`, e devolve `CompraResposta` com id do cartão, saldo e registro (`CompraServico.cs`).
4. `POST /api/cartoes/{id}/compras` encaminha o corpo `ProdutoId` e devolve 200. Saldo insuficiente vira HTTP 400, `detail` “Saldo insuficiente para concluir a compra.”. Cartão ou produto ausente vira HTTP 404, título “Recurso não encontrado”, com o `detail` de cada exceção (`CartoesController.cs`, `TratadorExcecoes.cs`).
5. No aplicativo, o cartão já carregado no contexto identifica a compra. O card mostra nome e preço. “Usar cartão” só habilita com cartão. Confirmar chama a API; cancelar não chama. Na web a confirmação é um aviso na própria tela. Depois do sucesso o cliente permanece no catálogo e o contexto atualiza o saldo (`07-decisoes.md`, seção “Interface fechada na etapa 5”).
6. A aba Histórico continua a mesma lista. Compra com `nomeProduto` mostra o nome, “Quantidade” e o valor sem “+”. Recarga continua com a descrição e com “+ ”. Data e hora usam o `formatarDataHora` já usado na recarga.

`Produto.Disponivel` não participa. Não há comprovante além do saldo e do registro, nem troca de cartão na interface. ACE-08 não ganhou modo de falha: a etapa 8 não alterou código (`07-decisoes.md`, “Etapa 8 sem diff”).

Decisões registradas em `07-decisoes.md` e usadas pelo código: mesma lista das recargas; campos próprios de produto, quantidade, valor, data e hora; quantidade sempre 1; colunas em `MovimentacaoCartao`, sem entidade nova e sem chave estrangeira de produto; exceção própria de saldo insuficiente; rota, 400 e 404 acima; botão existente; permanência no catálogo. P2.5 e P3.1 foram aceitos em 2026-10-07 como escritos na seção “Histórico fechado na etapa 6”.

Continuam abertos no mesmo arquivo: P1.4 (rastro de tentativa), ACE-08 (como provocar a falha), P2.3 (preço que muda entre ver e confirmar), P2.4 (identificação inválida que não seja cartão ou produto inexistente) e K4 (autenticação, estoque, checkout e banco).

---

## Alterações

Intervalo de produto: `ad91f0a..4a199f2`, mais um arquivo local não commitado. O commit `e0dff4e` só acrescenta `06-plan.zip`; a mensagem fala em CP-002 e o arquivo é da CP-001.

| Fluxo | Arquivos | O que mudou |
| --- | --- | --- |
| Domínio | `Cartao.cs`, `MovimentacaoCartao.cs`, `SaldoInsuficienteException.cs` | `Comprar` distinto de `Recarregar`; registro com produto, nome e quantidade 1 |
| SQLite | `MovimentacaoCartaoConfiguracao.cs`, migration `20261007183202_ProdutoEQuantidadeNaMovimentacao`, snapshot | Três colunas anuláveis em `MovimentacoesCartao`. O `Up` não cria chave estrangeira nem altera `Id` |
| Orquestração | `ICompraServico.cs`, `CompraServico.cs`, `IProdutoRepositorio.cs`, `ProdutoRepositorio.cs`, `ProdutoNaoEncontradoException.cs`, `Program.cs` | Leitura de produto por id e um `SaveChanges` só no sucesso |
| HTTP | `CompraRequisicao.cs`, `CompraResposta.cs`, `MovimentacaoResposta.cs`, `MapeadorCartao.cs`, `CartoesController.cs`, `TratadorExcecoes.cs` | `POST /api/cartoes/{id}/compras`; a listagem de movimentações passa a enviar os três campos |
| Aplicativo | `cartaoApi.ts`, `tipos/index.ts`, `CartaoContexto.tsx`, `CardProduto.tsx`, `TelaCatalogo.tsx` (commit), `TelaMovimentacoes.tsx`, `AlertaWeb.tsx`, `App.tsx` | Cliente da rota, confirmação, saldo no contexto e histórico sem “+” na compra |
| Testes no arquivo | `CartaoTestes.cs`, `CompraServicoTestes.cs` | Três `Comprar_*` e cinco `ComprarAsync_*`, em memória, sem SQLite |
| Documentos | `docs/product.md`, `docs/ui.md`, `docs/architecture.md`, `docs/backlog.md`, `README.md`, `AGENTS.md` | Passam a descrever a compra. O item do backlog fica “Atendida” |

Fora do commit `4a199f2`: `mobile/src/telas/TelaCatalogo.tsx` mede a largura do próprio catálogo para a grade. O relatório de navegação diz que, antes disso, 640 px e 390 px continuavam em duas colunas (`evidencias/navegacao/relatorio.md`, NAV-08). Essa correção não está no commit que o relatório cita.

### Divergências entre registro e código

- `07-execucao.md` localiza o aviso web em `CardProduto.tsx`. O diff também monta `AlertaWeb.tsx` em `App.tsx`, que substitui `Alert.alert` na web. O caminho da compra na web não chama `Alert.alert`: abre o `Modal` de `CardProduto.tsx`. As capturas web correspondem a esse aviso na tela, não a um segundo diálogo.
- O plano pedia parar cada etapa antes da seguinte e não tratar etapa sem evidência como validada. Os pareceres de `07-execucao.md` repetem “não avançar como etapa validada”. O código das etapas 1 a 7 está no Git mesmo assim.
- A etapa 7 reescreve as frases que negavam a compra. O título de `docs/backlog.md` continua “Demandas futuras” e “Não implementar até serem pedidas com critérios claros”, enquanto o item diz “Atendida”. O motivo dessa permanência não está escrito (`07-execucao.md`).
- ACE-07 pede informar dado inválido nos dois cenários. O código usa duas frases de `detail` e o mesmo título 404. `09-review-e-testes.md` não trata a diferença de frase como defeito. Não há corpo HTTP 404 gravado.
- `06-plan.md` deixava ACE-08 sem alteração especificável. O diff da etapa 8 é só texto em `07-decisoes.md`. O critério não foi observado.

---

## Validação

Nenhuma suíte foi executada para este documento. A saída “Passed! 16” em `07-execucao.md` não tem arquivo de comando e, pelo próprio acompanhamento, é anterior a `CompraServicoTestes`. `08-validacao.md` e `09-review-e-testes.md` não confirmam `dotnet test` depois dos oito métodos de compra.

Compilação do aplicativo com saída gravada: `percurso-etapa5/tsc.txt` e `percurso-etapa6/tsc.txt`, `exit_code: 0`. O `npx tsc --noEmit` citado na etapa 5 falhou com `FETCH_ERROR` antes de compilar. Não há log de `dotnet build` fora do parágrafo da resolução da etapa 2.

### Critérios

| Critério | Resultado sustentado | Pendência |
| --- | --- | --- |
| ACE-01, forma geral | Observado no café do seed a R$ 8,50. Recarga R$ 10,00, saldo final 1,5, quantidade 1, nome “Café especial”. `percurso-etapa5/compras.json`, `07-compra-cafe.png`, `08-cartao-apos-compra.png`. Repetido em NAV-04 (`relatorio.md`, cartão CP-GK2U8Y). O par R$ 50 / R$ 20 / R$ 30 não era meta | Testes `Comprar_SaldoMaiorQuePreco_DebitaPrecoERegistraCompra` e `ComprarAsync_SaldoMaiorQuePreco_DebitaEGravaUmaVez` existem e não têm saída gravada |
| ACE-02 | Observado na web: preço visível antes do aviso; cancelar não chama `/compras`. `percurso-etapa5/log.txt`, `04-catalogo-saldo-10.png`, `05-alerta-cafe.png`. O mesmo gesto no adesivo de R$ 4,00 em `percurso-ace04/` e em NAV-03 | Sem captura do `Alert` nativo. P2.3, preço alterado entre ver e confirmar, sem cenário |
| ACE-03 | Código: um `SalvarAlteracoesAsync` depois do débito e da movimentação. Os corpos 200 trazem saldo e registro juntos. O log da API em `1.txt` mostra `UPDATE` de saldo e `INSERT` com as colunas de produto; parâmetros como `?`, sem `BEGIN` | Não prova sozinho qual compra foi gravada nem uma transação impressa. Sem teste de SQLite com saída |
| ACE-04 | Observado no adesivo de R$ 4,00 com saldo R$ 4,00: saldo 0,0 no JSON, “R$ 0,00” no catálogo e no cartão, uma compra no histórico. `percurso-ace04/compras.json`, `05-saldo-zero.png`, `06-cartao-zero.png`, `07-historico.png`. Repetido em NAV-07 (CP-RZ3TQ2) | Um produto do seed. Teste de saldo zero sem execução gravada |
| ACE-05 | Nos percursos capturados o saldo ficou 1,50, 0,00 ou o anterior (R$ 1,50) depois da recusa. Nenhum desses artefatos mostra saldo negativo | Não cobre caminho não capturado nem ACE-08 |
| ACE-06 | Observado: adesivo R$ 4,00 com saldo R$ 1,50, HTTP 400, `detail` “Saldo insuficiente para concluir a compra.”, saldo na tela R$ 1,50, histórico sem o adesivo. `percurso-etapa5/compras.json`, `10-saldo-insuficiente.png`, `percurso-etapa6/historico.png`. Repetido em NAV-06 | Um par saldo/preço. O teste que afirma que a gravação não foi chamada não tem saída |
| ACE-07 | Código e testes em arquivo (`ComprarAsync_CartaoAusente_NaoGrava`, `ComprarAsync_ProdutoAusente_NaoGravaEMantemSaldo`). HTTP 404 no `TratadorExcecoes` | **Sem comportamento observado.** Nenhum corpo 404 em `percurso-etapa5/`, `percurso-ace04/` ou na navegação. P2.4 ficou de fora: sem cartão, o botão não informa dado inválido |
| ACE-08 | Não verificado. Recusas de saldo, cartão ausente e produto ausente acontecem antes de `SalvarAlteracoesAsync`. HTTP 500 genérico está no código e não numa resposta gravada | **Sem modo de falha e sem evidência.** P1.4 continua aberto |

### Outras evidências, e o que não comprovam

- Histórico atual da compra: `percurso-etapa6/historico.png` e `percurso-ace04/07-historico.png`, mais NAV-04 e NAV-07. `percurso-etapa5/11-historico.png` ainda mostra “+” na compra e é anterior à etapa 6. Não sustenta o desenho atual.
- Navegação de 2026-10-08, `evidencias/navegacao/relatorio.md`: 8 cenários planejados, 8 executados, 8 passaram no relato. Ambiente `http://localhost:8081` e API `http://127.0.0.1:5080`. O próprio relatório deixa ACE-07, ACE-08, alerta nativo e `dotnet test` fora da rodada.
- NAV-08, depois da medição da largura do catálogo: 640×900 com um produto por linha e 960×900 com duas colunas, no cartão de saldo R$ 0,00 e com “Usar cartão” habilitado. As imagens `nav-08-largura-390.png` e `nav-08-390-apos-reload.png` são da tentativa anterior, com duas colunas abaixo de 720 px. A correção usada nessa nova execução não está em `4a199f2`.
- A hora exibida na navegação, 13:13 a 13:18, coincide com o instante UTC dos JSON, não com o relógio local UTC−3 (`relatorio.md`). `09-review-e-testes.md` já tratava isso como hipótese; a rodada registra a coincidência e não grava, neste repositório, o JSON de `GET /movimentacoes` ao lado do fuso.
- `/tmp/compra-cafe.json` e o log da API em `127.0.0.1:5099`, citados na revisão da etapa 4, não estão no repositório. Os corpos usados são os de `percurso-etapa5` e `percurso-ace04`.
- `09-review-e-testes.md` não confirma falha de saldo nos percursos já capturados. Também não corrige achados. A decisão de aceitar a entrega continua humana, como o relatório de navegação repete.

---

## Riscos

- Tratar a demanda como fechada. ACE-07 e ACE-08 não têm observação. A suíte de compra não tem execução gravada, então não há prova registrada de regressão da recarga junto com a compra (`09-review-e-testes.md`).
- Ler só o cabeçalho de `docs/backlog.md` e tratar a compra já implementada como proibida.
- Conferir o histórico em `percurso-etapa5/11-historico.png` e concluir que a compra ainda aparece com “+”.
- Conferir o commit `4a199f2` e não achar a correção de colunas. Quem reproduzir NAV-08 nesse commit volta à medição que manteve duas colunas abaixo de 720 px.
- Hora do histórico no relógio UTC, já coincidente com o UTC na navegação de 2026-10-08. Não altera o débito. Em UTC−3 a hora vista pode divergir três horas (`09-review-e-testes.md`).
- `comprar` em `CartaoContexto.tsx` só chama `setCartao` depois de `listarMovimentacoes`. Se o `POST` gravar e o `GET` falhar, o catálogo mostra erro e o saldo do contexto não troca, com o débito já no servidor. As capturas de sucesso não exercitam essa falha. O mesmo encadeamento já existe em `recarregar`.
- Colunas anuláveis e sem chave estrangeira: o esquema não impede compra sem produto nem quantidade diferente de 1. A quantidade 1 está em `CriarCompra`, não no SQLite.
- `Comprar` altera o saldo e depois chama `CriarCompra`. A fábrica atual não lança. Uma falha entre as duas linhas deixaria débito sem registro. Esse é o tema de ACE-08, sem mecanismo.
- Preço negativo não é rejeitado. Os preços do seed lidos na configuração são positivos. A proposta não escreve esse caso (`07-execucao.md`, etapa 1).
- Duas confirmações no mesmo saldo continuam fora da proposta. A API não tem token de concorrência. O botão da tela bloqueia outro toque enquanto uma compra está em envio (`09-review-e-testes.md`).
- A interface não troca de cartão. A navegação criou o segundo cartão removendo a chave `@cardplay/cartaoId` no navegador. O primeiro permaneceu na API (`relatorio.md`).
- HTTP 500 de exceção não mapeada não é a mensagem “compra não concluída” de ACE-08 e não prova saldo gravado intacto.

---

## Próximos passos

Separados do que já está no código e do que as capturas já mostram. Nenhum item desta lista foi feito na preparação deste arquivo.

- Guardar a saída de `dotnet test backend/CardPlay.sln` se a suíte for citada como execução. Os oito métodos de compra e a suíte de recarga precisam aparecer nessa saída (`08-validacao.md`, `09-review-e-testes.md`).
- Gravar `POST /api/cartoes/{id}/compras` com cartão inexistente e com produto inexistente, com status e `detail` (ACE-07).
- Decidir se ACE-08 permanece critério sem evidência ou se o modo de falha será descrito. O plano não autoriza inventar falha artificial, teste de SQLite ou transação extra (`06-plan.md`, `07-decisoes.md`).
- Decidir P1.4, rastro de tentativa que não seja compra concluída.
- Aceitar o item “Atendida” debaixo do título de demandas futuras em `docs/backlog.md`, ou separar esse item. A etapa 7 ficou sem esse motivo (`07-execucao.md`).
- Incluir no Git a medição de largura de `TelaCatalogo.tsx` que a navegação de 2026-10-08 já usou. Ela não está em `4a199f2`.
- Percorrer o alerta nativo do celular. As capturas são web.
- Gravar o fuso do navegador e o JSON de `GET /movimentacoes` do mesmo cartão, se a hora UTC ainda precisar de confirmação além da coincidência já escrita no relatório de navegação.
- Interromper a listagem depois de um `POST /compras` 200, se for preciso confirmar o saldo do contexto desatualizado. Não há captura disso.

Fora do que esta entrega fecha, e já deixados sem etapa: P2.3, P2.4, K4, `Produto.Disponivel` e concorrência.
