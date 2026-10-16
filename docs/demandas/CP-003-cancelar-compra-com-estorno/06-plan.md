# CP-003 — Plano de implementação: Cancelar uma compra com estorno integral

## Estado deste documento

| Campo | Valor |
| --- | --- |
| Demanda | CP-003 — Cancelar uma compra com estorno integral |
| Artefato | Plano para avaliação humana, antes de autorizar código |
| Fontes | `01-entendimento.md` a `05-analise-de-impacto.md` |
| Arquitetura | A do artefato 04: a compra concluída já existe. Este plano não cria outra camada |
| Data | 2026-10-16 |
| Decisões de caminho | Fechadas em `01-entendimento.md`, seção de 2026-10-16 |
| Efeito deste arquivo | Ordena a mudança e os pontos de inspeção. **Não autoriza código** até a leitura deste plano |

As etapas não se juntam. Cada uma para para inspeção antes da próxima.

As frases de compra inexistente, de outro cartão e já cancelada, e os status HTTP dessas recusas, continuam em aberto. Onde uma etapa precisa deles, o texto diz **proposto**. A inspeção dessa etapa aceita ou troca antes de seguir. A frase da falha de gravação já está decidida.

---

## Veredito

O caminho de negócio está decidido. Dá para escrever cada etapa sem reabrir o total debitado, a data de cancelamento, o estorno na mesma lista, a CP-004 ou a falha só no teste.

Nenhuma etapa deve ser executada só porque este arquivo existe. A primeira autorização, se a revisão concordar, é a etapa 1.

---

## Escopo

Dentro:

- Cancelamento integral de uma compra concluída ainda não cancelada, pelo id da movimentação e pelo cartão do contexto.
- Aviso no Histórico com nome, quantidade, preço unitário, total pago e valor do estorno. Desistir não chama a API.
- Crédito do `Valor`, uma vez, também quando a quantidade é maior que 1. O preço atual do catálogo não entra.
- No mesmo gravar: saldo, data de cancelamento na compra original e um estorno com a descrição “Estorno” e o id da compra. A compra original permanece.
- Repetição sem outro crédito e sem outro estorno.
- Id que não é compra daquele cartão: recusa, sem crédito e sem estorno. Recarga entra nesse caso.
- Falha de gravação só no teste, sem rastro e sem interruptor no produto.

Fora: estorno parcial, taxa, prazo, chargeback, autenticação, estoque, CP-004, dois cancelamentos ao mesmo tempo e documento de produto antes da inspeção do comportamento.

---

## Ordem

```text
Etapa 1  crédito, marca e estorno no domínio
   ↓
Etapa 2  colunas no SQLite
   ↓
Etapa 3  orquestração
   ↓
Etapa 4  HTTP
   ↓
Etapa 5  confirmação no Histórico
   ↓
Etapa 6  documentos que ainda não falam em desfazer a compra
   ↓
Etapa 7  falha de gravação no teste
```

---

## Etapas

### Etapa 1 — Crédito, marca e estorno no domínio

| Campo | Conteúdo |
| --- | --- |
| Objetivo | O cartão cancela uma compra sua: credita o total, marca a compra e cria o estorno, ou não altera nada |
| Critério | ACE-01 na forma geral, ACE-03, ACE-04, ACE-05 e ACE-08 no domínio. ACE-06 no que o crédito é o `Valor` já gravado. ACE-07 no que id ausente ou recarga não alteram o cartão |
| Alteração | `Cartao` ganha o cancelamento da compra identificada pelo id. A recusa acontece antes de mudar saldo e lista. O sucesso soma o `Valor`, preenche a data de cancelamento em UTC e acrescenta o estorno |
| Componentes | `Cartao.cs`, `MovimentacaoCartao.cs`, `CartaoTestes.cs` |
| Proposto | O campo da marca se chama `DataCancelamento`. O estorno guarda o id da compra num campo `CompraId`. O estorno não preenche `NomeProduto`. A inspeção pode recusar esses nomes antes da etapa 2 |
| Evidência | Testes de domínio, sem banco: compra de total 20 e saldo 30 termina em 50, com a compra conservada, marcada, e um estorno de 20 vinculado; quantidade maior que 1 credita o total uma vez; repetir não muda saldo nem cria outro estorno; id ausente e recarga não mudam o cartão; o preço do produto, se mudado no objeto, não muda o crédito |
| Parada | Inspeção do domínio e dos testes, antes de migration |

### Etapa 2 — Colunas no SQLite

| Campo | Conteúdo |
| --- | --- |
| Objetivo | A data de cancelamento e o vínculo do estorno persistem na mesma tabela de movimentações |
| Critério | ACE-03 no que a marca e o vínculo sobrevivem ao gravar. A recarga continua sem esses dados |
| Alteração | Configuração e migration das duas colunas, anuláveis. Sem tabela nova e sem chave estrangeira, como `ProdutoId` já é |
| Componentes | `MovimentacaoCartaoConfiguracao.cs`, pasta `Migrations` |
| Evidência | A migration adiciona só essas colunas. A solution compila. `cardplay.db` não é prova |
| Parada | Inspeção do modelo, antes do serviço |

### Etapa 3 — Orquestração

| Campo | Conteúdo |
| --- | --- |
| Objetivo | O caso de uso recebe o cartão e o id da compra, chama o domínio e grava uma vez |
| Critério | ACE-01, ACE-03, ACE-04, ACE-05, ACE-07 e ACE-08, ainda sem HTTP |
| Alteração | O caso de uso da compra ganha o cancelamento. Compra de outro cartão não está na lista daquele cartão e segue a mesma recusa do id ausente. Compra já cancelada não grava. O sucesso chama um `SalvarAlteracoesAsync` |
| Componentes | `CompraServico.cs`, `ICompraServico.cs`, `CompraServicoTestes.cs` |
| Proposto | Continua em `CompraServico`, sem serviço novo. A inspeção pode separar |
| Evidência | Testes em memória: uma gravação no sucesso; nenhuma gravação em id ausente, recarga e compra já cancelada; o estorno não leva `NomeProduto`; quantidade maior que 1 credita o total |
| Parada | Inspeção do caso de uso, antes da rota |

### Etapa 4 — HTTP

| Campo | Conteúdo |
| --- | --- |
| Objetivo | O cancelamento entra com o id da compra e sai com o saldo novo e os registros, ou com a recusa |
| Critério | ACE-07 e ACE-08 observáveis na API. ACE-01 no corpo de sucesso. ACE-06 no que o corpo não envia o preço do catálogo |
| Alteração | Nova ação em `CartoesController`. O controlador só encaminha. A resposta reutiliza saldo e movimentação. `MovimentacaoResposta` passa a levar a data de cancelamento e o id da compra, ambos vazios quando não couberem |
| Componentes | `CartoesController.cs`, `TratadorExcecoes.cs`, `MovimentacaoResposta.cs`, mapeador |
| Proposto | Rota `POST /api/cartoes/{id}/compras/{compraId}/cancelamento`. Id que não é compra daquele cartão: HTTP 404, título “Recurso não encontrado”, detalhe “Não há compra deste cartão para cancelar.” Compra já cancelada: HTTP 409, título “Conflito”, detalhe “Esta compra já foi cancelada.” A inspeção aceita ou troca frase e status antes de a etapa valer |
| Evidência | Chamadas gravadas: sucesso com crédito do `Valor` e um estorno; repetição sem segundo crédito; id inexistente; id de compra de outro cartão; id de recarga. O aplicativo ainda não muda |
| Parada | Inspeção do contrato, antes da tela |

### Etapa 5 — Confirmação no Histórico

| Campo | Conteúdo |
| --- | --- |
| Objetivo | O cliente vê a compra e o valor do estorno e só então confirma |
| Critério | ACE-02. ACE-01, ACE-04 e ACE-06 no gesto. ACE-07 e ACE-08 se a recusa aparecer |
| Alteração | `TelaMovimentacoes`, `cartaoApi`, `CartaoContexto`, tipo `Movimentacao`. O cliente HTTP continua só em `mobile/src/api/`. “Cancelar compra” só na compra sem data de cancelamento. O aviso reutiliza o modo já usado em “Usar cartão”: na web, aviso na própria tela; no celular, o alerta nativo. Desistir não chama a API. O estorno mostra “+” e o nome achado na compra cujo id ele guarda. Sem mudar a barra nem `cores.ts` |
| Proposto | Depois do sucesso o cliente permanece no Histórico. Isso fecha P3.1 se a inspeção aceitar |
| Evidência | Percurso: aviso com os dados da compra e o valor do estorno; desistir sem POST; confirmar e saldo creditado; a compra sem “+” com “Cancelada”; o estorno com “+”; repetir sem segundo crédito. Uma compra cujo preço de catálogo mudou devolve o total antigo |
| Parada | Inspeção do fluxo, antes dos documentos |

### Etapa 6 — Documentos que ainda não falam em desfazer a compra

| Campo | Conteúdo |
| --- | --- |
| Objetivo | `docs/product.md`, `docs/ui.md` e `AGENTS.md` passarem a descrever o cancelamento da compra concluída sem apagar o sentido atual de desistir da confirmação de “Usar cartão” |
| Critério | Nenhuma regra nova. As frases passam a descrever crédito do total, marca, estorno e desistência sem API |
| Alteração | Só esses arquivos |
| Evidência | Leitura cruzada das frases, depois do comportamento inspecionado |
| Parada | Inspeção do texto |

### Etapa 7 — Falha de gravação no teste

| Campo | Conteúdo |
| --- | --- |
| Objetivo | ACE-09: o salvar falha e não ficam crédito, marca nem estorno |
| Critério | ACE-09 |
| Alteração | Um teste faz o salvar falhar e relê o estado em outro contexto. Não há interruptor no produto nem rastro de tentativa. A informação é a de que o cancelamento não foi concluído |
| Evidência | O teste mostra saldo anterior, compra sem data de cancelamento e nenhum estorno. Não se observa ACE-09 só com o `SaveChanges` que funciona |
| Parada | Fim do plano |

---

## O que nenhuma etapa faz

- Estorno parcial ou crédito diferente do `Valor`.
- Estoque e a CP-004.
- Taxa, prazo e chargeback.
- Autenticação ou aviso novo de autorização de produção. P3.2 continua aberto.
- Trava contra dois cancelamentos simultâneos.
- Documento de produto nas etapas 1 a 5.
