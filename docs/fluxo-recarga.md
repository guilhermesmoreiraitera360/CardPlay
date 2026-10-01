# Fluxo da recarga simulada

A recarga simulada soma um valor positivo ao saldo do cartão e grava uma movimentação no mesmo `SaveChanges`. Não há cobrança nem chamada a serviço externo.

## 1. Resumo do fluxo

O caminho principal, lido no código, é este:

1. Na aba **Recarga**, `TelaRecarga` só segue se já existir um cartão no contexto. O valor inicial do campo é `"25"` e a descrição é `"Recarga"`. Os chips 10, 25 e 50 apenas preenchem o campo.
2. Ao tocar em **Recarregar**, a tela converte o texto com `Number(valor.replace(',', '.'))` e chama `recarregar(numero, descricao)`. Não há validação de valor na tela antes do envio. O botão fica desabilitado só enquanto a chamada está em andamento.
3. `CartaoContexto.recarregar` exige um cartão já carregado, faz `POST /api/cartoes/{id}/recargas` com `{ valor, descricao }` e, em seguida, `GET /api/cartoes/{id}/movimentacoes`. Atualiza `cartao` e `movimentacoes` no contexto. O id do cartão vem do estado já carregado (persistido no AsyncStorage na solicitação); a recarga em si não grava o id de novo.
4. `CartoesController.Recarregar` recebe o corpo como `RecargaRequisicao` e delega a `CartaoServico.RecarregarAsync`. O controller não altera saldo nem movimentação.
5. `CartaoServico` busca o cartão. Se não existir, lança `CartaoNaoEncontradoException`. Se existir, chama `cartao.Recarregar(valor, descricao)`, pede ao repositório para registrar a movimentação e chama `SalvarAlteracoesAsync` uma vez. A resposta HTTP 200 é o cartão (`CartaoResposta`), com o saldo já alterado em memória. A movimentação não vai nesse corpo.
6. `Cartao.Recarregar` rejeita `valor <= 0` com `RecargaInvalidaException` antes de mudar qualquer coisa. Com valor positivo, faz `Saldo += valor`, cria a movimentação e a inclui na coleção do cartão. Descrição vazia ou só espaços vira `"Recarga"`; caso contrário, usa o texto com `Trim`.
7. `MovimentacaoCartao.Criar` gera um `Guid`, guarda o id do cartão, o valor, a descrição e `DateTime.UtcNow`.
8. `CartaoRepositorio.SalvarAlteracoesAsync` chama `SaveChangesAsync` do EF Core. O arquivo configurado é `cardplay.db` (`Data Source=cardplay.db`). A tabela do saldo é `Cartoes.Saldo`; a da movimentação é `MovimentacoesCartao`.
9. Se a recarga inválida ou o cartão ausente escapam até a API, `TratadorExcecoes` devolve ProblemDetails: 400 para `RecargaInvalidaException` (detalhe: `"A recarga deve ter valor maior que zero."`) e 404 para `CartaoNaoEncontradoException`. O cliente lê `detail` ou `title` e a tela mostra essa mensagem. No sucesso, a mensagem `"Recarga registrada com sucesso."` é texto local da tela, não vem da API.
10. A aba **Histórico** lê `movimentacoes` do mesmo contexto. A listagem na API ordena por `DataHora` decrescente. A aba **Cartão** lê o mesmo `cartao`, então o saldo exibido depende desse estado atualizado depois da resposta.

`CardPlay.Services` não participa. A arquitetura do repositório descreve a recarga como Application, domínio, repositório e SQLite, sem API externa.

## 2. Partes e responsabilidades

| Parte | Responsabilidade observada |
| --- | --- |
| `TelaRecarga` | Entrada do valor e da descrição, envio e mensagem local de sucesso ou erro. |
| `CartaoContexto` | Chama a API de recarga, busca o histórico e substitui cartão e movimentações em memória. |
| `cartaoApi` / `clienteHttp` | `POST` e `GET` em `{URL_API}/api/cartoes/...`. A URL vem de `EXPO_PUBLIC_API_URL` ou, se ausente, `http://10.0.2.2:5080` no Android e `http://localhost:5080` nos demais. |
| `CartoesController` | Recebe o HTTP e devolve 200 com `CartaoResposta`. |
| `CartaoServico` | Orquestra busca, regra de domínio, registro da movimentação e um único `SalvarAlteracoesAsync`. |
| `Cartao` | Regra: valor `<= 0` é inválido; senão, saldo e movimentação nascem juntos. |
| `MovimentacaoCartao` | Cria o registro da recarga (id, cartão, valor, descrição, data UTC). |
| `CartaoRepositorio` | Carrega o cartão com as movimentações, marca a nova movimentação para inclusão e persiste com `SaveChangesAsync`. |
| `TratadorExcecoes` | Converte as exceções de domínio em 400 ou 404. |
| SQLite / EF | Tabelas `Cartoes` e `MovimentacoesCartao`, com `Saldo` e `Valor` em `decimal` (`TEXT`, precisão 18,2). |

## 3. Evidências

Regra de domínio e criação conjunta de saldo e movimentação:

```40:51:backend/src/CardPlay.Domain/Entidades/Cartao.cs
    public MovimentacaoCartao Recarregar(decimal valor, string? descricao)
    {
        if (valor <= 0)
        {
            throw new RecargaInvalidaException();
        }

        var texto = string.IsNullOrWhiteSpace(descricao) ? "Recarga" : descricao.Trim();
        Saldo += valor;
        var movimentacao = MovimentacaoCartao.Criar(Id, valor, texto);
        _movimentacoes.Add(movimentacao);
        return movimentacao;
    }
```

Orquestração e um único save:

```35:44:backend/src/CardPlay.Application/Servicos/CartaoServico.cs
    public async Task<CartaoResposta> RecarregarAsync(
        Guid id,
        RecargaRequisicao requisicao,
        CancellationToken cancellationToken = default)
    {
        var cartao = await ObterCartaoAsync(id, cancellationToken);
        var movimentacao = cartao.Recarregar(requisicao.Valor, requisicao.Descricao);
        _cartaoRepositorio.AdicionarMovimentacao(movimentacao);
        await _cartaoRepositorio.SalvarAlteracoesAsync(cancellationToken);
        return MapeadorCartao.ParaResposta(cartao);
    }
```

Persistência: `ObterPorIdAsync` inclui `Movimentacoes` e não usa `AsNoTracking`. `AdicionarMovimentacao` inclui a entidade se estiver `Detached`; se já estiver rastreada e o estado não for `Added`, força `Added`. `SalvarAlteracoesAsync` é só `SaveChangesAsync`.

Testes existentes, sem EF e sem SQLite:

- `CartaoTestes`: valor 0, -1 e -15,5 rejeitados, saldo permanece 0 e a coleção fica vazia; 50,25 atualiza o saldo; 20 registra valor, descrição e `CartaoId`; descrição em branco vira `"Recarga"`.
- `CartaoServicoTestes`: recarga de 40 com descrição persiste no repositório em memória e conta duas chamadas a `SalvarAlteracoesAsync` (criação do cartão e recarga). Valor 0 lança `RecargaInvalidaException` e o saldo consultado continua 0, sem movimentação. O falso repositório deixa `AdicionarMovimentacao` vazio; o histórico do teste enxerga a movimentação porque ela já está na coleção do mesmo objeto `Cartao`.

O aplicativo e a API não foram executados nesta investigação.

## 4. Dúvidas ou pontos não confirmados

- Não há teste de integração com EF Core ou SQLite. O código chama `SaveChangesAsync` uma vez depois de alterar o saldo e de chamar `AdicionarMovimentacao`. Não está confirmado, em execução, que as duas gravações chegam juntas ao `cardplay.db`, nem o que o rastreador do EF faz com a coleção `_movimentacoes` além do `Add` explícito.
- Não há transação explícita no serviço. O efeito transacional fica no `SaveChanges` do EF. Isolamento e rollback no SQLite não estão cobertos por teste neste repositório.
- `Program.cs` não define política de nomes JSON. O mobile envia e lê camelCase (`valor`, `saldo`). A correspondência com as propriedades PascalCase depende do padrão do ASP.NET Core para controllers. Não há teste de contrato HTTP.
- A tela não rejeita valor não numérico nem `<= 0`. `Number` de texto inválido produz `NaN`. O que a API faz com esse corpo não está testado aqui.
- A descrição tem `HasMaxLength(120)` na configuração do EF e na migration. O domínio não limita o tamanho. O que acontece com texto maior que 120 caracteres não está testado.
- Não há token de concorrência em `Cartao`. O comportamento de duas recargas simultâneas no mesmo cartão não está no código nem nos testes.
- A mensagem de sucesso da tela não prova que o saldo na interface mudou; isso depende do `setCartao` com o corpo do `POST`. Isso não foi confirmado na interface.
