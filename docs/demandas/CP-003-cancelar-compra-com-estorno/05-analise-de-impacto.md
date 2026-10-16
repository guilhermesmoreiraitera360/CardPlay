# CP-003 — Análise de impacto

## Estado deste documento

| Campo | Valor |
| --- | --- |
| Demanda | CP-003 — Cancelar uma compra com estorno integral |
| Artefato | Impacto da história sobre o código depois da CP-002 |
| Fontes | `02-historia-rica.md`, `03-criterios-de-aceite.md`, `04-exploracao-repositorio.md`, decisões de 2026-10-16 em `01-entendimento.md` |
| Data | 2026-10-16 |
| O que este arquivo faz | Liga cada efeito observável ao ponto do código que hoje não o faz |
| O que este arquivo não faz | Não escolhe o nome da migration, não escolhe status HTTP, não escreve código e não executa testes |

Legenda: **Confirmado** (o código citado não faz o que o critério pede) · **Decidido** (o caminho já está em `01-entendimento.md`) · **Aberto** (a frase ou o destino ainda não foram escolhidos) · **Fora** (a história exclui).

---

## O que a compra atual atravessa

A compra concluída já existe. A CP-003 não cria outra camada.

```text
Catálogo → POST /api/cartoes/{id}/compras { produtoId, quantidade, precoUnitarioVisto }
        → CompraServico.ComprarAsync
        → Cartao.Comprar
        → MovimentacaoCartao.CriarCompra  (Valor = total)
        → um SaveChanges
```

O histórico só lê `GET /api/cartoes/{id}/movimentacoes`. `CardPlay.Services` continua sem caso de uso.

---

## Impactos

### I-01 — Creditar o total de uma compra já gravada

| Campo | Conteúdo |
| --- | --- |
| Grau | **Confirmado** |
| Onde | `Cartao` |
| Hoje | `Saldo` sobe em `Recarregar` e desce em `Comprar`. Não há método que some o `Valor` de uma compra existente |
| Efeito | ACE-01 e ACE-05 exigem o saldo anterior mais o total debitado, uma vez, também quando a quantidade é maior que 1. Sem um método novo, o crédito não acontece |
| Critério | ACE-01, ACE-05 |

### I-02 — Marca na compra e estorno vinculado

| Campo | Conteúdo |
| --- | --- |
| Grau | **Confirmado** |
| Onde | `MovimentacaoCartao`, `MovimentacaoCartaoConfiguracao`, snapshot, `MovimentacaoResposta`, tipo `Movimentacao` |
| Hoje | Não há data de cancelamento nem id de outra movimentação. Os setters são privados. `CriarCompra` e `Criar` são os únicos construtores |
| Efeito | A decisão P1.3 pede a data de cancelamento, em UTC, no registro original, e outra movimentação com descrição “Estorno”, o mesmo valor e o id da compra. Isso pede colunas novas. A compra original conserva produto, quantidade, preço unitário, total e data |
| Critério | ACE-01, ACE-03, ACE-08 |

### I-03 — Caso de uso e rota

| Campo | Conteúdo |
| --- | --- |
| Grau | **Confirmado** |
| Onde | `CartoesController`, `CompraServico`, `cartaoApi`, `CartaoContexto` |
| Hoje | A única escrita de compra é `POST /api/cartoes/{id}/compras`. O aplicativo não envia o id de uma movimentação |
| Efeito | A decisão P1.2 manda confirmar com o id do cartão do contexto e o id da compra. Sem essa entrada, o Histórico não tem a quem pedir o cancelamento |
| Critério | ACE-01, ACE-02 |

### I-04 — Compra de outro cartão ou inexistente

| Campo | Conteúdo |
| --- | --- |
| Grau | **Confirmado** no efeito; **Aberto** na frase |
| Onde | `CartaoRepositorio.ObterPorIdAsync` |
| Hoje | A carga traz só as movimentações daquele cartão. Não há busca de uma movimentação pelo id. Não há recusa de cruzar cartões, porque a operação não existe |
| Efeito | ACE-07 exige recusa, saldos intactos e nenhum estorno. A frase dessa recusa continua em P2.1. Este arquivo não escolhe o status HTTP |
| Critério | ACE-07 |

### I-05 — Repetição

| Campo | Conteúdo |
| --- | --- |
| Grau | **Confirmado** no efeito; **Aberto** na frase |
| Onde | Não há marca para consultar |
| Hoje | Uma compra gravada pode ser lida de novo. Nada impede um segundo crédito, porque o primeiro não existe |
| Efeito | ACE-04 e ACE-08 exigem o saldo anterior e um único estorno quando a compra já tem data de cancelamento. A mensagem da repetição continua em P2.3 |
| Critério | ACE-04, ACE-08 |

### I-06 — Histórico

| Campo | Conteúdo |
| --- | --- |
| Grau | **Confirmado** |
| Onde | `TelaMovimentacoes` |
| Hoje | Compra é toda linha com `nomeProduto`. Ela mostra nome, quantidade, preço unitário e valor sem “+”. Não há botão. Não há linha “Cancelada” |
| Efeito | ACE-02 pede “Cancelar compra” só na compra ainda não cancelada, com aviso de nome, quantidade, preço unitário, total pago e valor do estorno. Desistir não chama a API. ACE-03 pede a compra sem “+” com “Cancelada”, e o estorno na mesma lista com “+”. O nome do estorno é lido pelo id da compra, não gravando `nomeProduto` no estorno: se esse campo viesse preenchido, a tela atual trataria o estorno como compra |
| Critério | ACE-02, ACE-03 |

### I-07 — Preço atual do catálogo

| Campo | Conteúdo |
| --- | --- |
| Grau | **Decidido** |
| Onde | `Produto.Preco` e `MovimentacaoCartao.Valor` |
| Hoje | Os dois já estão separados. O seed não tem produto a R$ 20,00 |
| Efeito | ACE-06 usa o `Valor` gravado. Mudar `Produto.Preco` não pode mudar o crédito. Não pede coluna de preço |
| Critério | ACE-06 |

### I-08 — Falha ao gravar

| Campo | Conteúdo |
| --- | --- |
| Grau | **Confirmado** |
| Onde | `CartaoRepositorio.SalvarAlteracoesAsync`; `CompraServico`; `FalhaGravacaoSqliteTestes` |
| Hoje | A compra já envolve a falha do salvar em “A compra não foi concluída.” e o teste relê outro contexto SQLite. Não há o mesmo teste para cancelamento. Não há interruptor no produto |
| Efeito | A decisão P1.5 pede o mesmo meio: falha no teste, no salvar, sem crédito, sem data de cancelamento, sem estorno e sem rastro. A frase é “o cancelamento não foi concluído.” Exceção não mapeada hoje vira HTTP 500 com `exception.Message`. Este arquivo não cria status novo |
| Critério | ACE-09 |

### I-09 — Testes da compra que já existem

| Campo | Conteúdo |
| --- | --- |
| Grau | **Confirmado** |
| Onde | `CartaoTestes`, `CompraServicoTestes`, `FalhaGravacaoSqliteTestes` |
| Hoje | Cobrem compra, quantidade, preço alterado e falha ao salvar a compra. Não foram executados nesta leitura. Não afirmam cancelamento |
| Efeito | O cancelamento pede testes novos. Os testes da compra não quebram só porque o método novo ainda não existe |
| Critério | ACE-01, ACE-04, ACE-05, ACE-07, ACE-09 |

### I-10 — Documentos que não falam em desfazer a compra

| Campo | Conteúdo |
| --- | --- |
| Grau | **Confirmado** |
| Onde | `docs/product.md`, `docs/ui.md`, `AGENTS.md` |
| Hoje | “Cancelar” é desistir da confirmação de “Usar cartão”. O histórico não mostra compra cancelada nem estorno |
| Efeito | Depois do comportamento novo, essa frase continua valendo para a compra que ainda não ocorreu e deixa de ser o único cancelamento. A atualização dos documentos fica para o fim, para não documentar antes da inspeção |
| Critério | Nenhum ACE novo. É alinhamento posterior |

---

## O que não muda

| Item | Por quê |
| --- | --- |
| Camadas e `CardPlay.Services` | A orquestração continua na Application |
| Comprar, recarregar e “Usar cartão” | A CP-003 não muda a confirmação da compra nova |
| Recarga | Não é compra. Não ganha “Cancelar compra” |
| Lista de histórico | Decisão P1.3. O estorno entra nela |
| `Produto.Disponivel` e estoque | Fora. A CP-004 não entra |
| Estorno parcial, taxa, prazo, chargeback, autenticação | Fora da história |
| Concorrência de dois cancelamentos ao mesmo tempo | A história não descreve dois pedidos em voo antes do primeiro gravar. Não se cria trava nova neste arquivo |
| Aviso de autorização de produção | P3.2 ainda não escolhe o lugar. RN04 já proíbe declarar segurança de produção |

---

## Dados

| Dado | Hoje | Depois da decisão |
| --- | --- | --- |
| Total debitado | `Valor` da compra | Continua. O crédito usa esse valor |
| Data de cancelamento | Não existe | Campo no registro original. Vazio significa não cancelada |
| Vínculo do estorno | Não existe | O estorno guarda o id da compra |
| Nome no estorno | A tela usa `nomeProduto` para reconhecer compra | O estorno não preenche `nomeProduto`. A tela acha o nome na compra vinculada |
| Saldo | `decimal` | Continua `decimal`. Sobe o `Valor` só quando o gravar conclui |

---

## Ordem em que o impacto aparece

A marca sem coluna não volta na listagem. O crédito sem método no cartão não altera o saldo. A rota sem o id da compra não tem o que cancelar. O Histórico que preenche `nomeProduto` no estorno mostra um estorno como se fosse compra. A falha de gravação só se observa depois de existir um cancelamento que grave saldo, marca e estorno no mesmo `SaveChanges`.

Por isso a análise separa: regra e registro, depois contrato, depois Histórico, e a falha de gravação por último.

O plano ainda não está escrito. Este arquivo não abre etapa de código.
