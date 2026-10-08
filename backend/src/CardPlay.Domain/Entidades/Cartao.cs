using CardPlay.Domain.Excecoes;

namespace CardPlay.Domain.Entidades;

public class Cartao
{
    private readonly List<MovimentacaoCartao> _movimentacoes = [];

    private Cartao()
    {
        NomeTitular = string.Empty;
        CodigoAmigavel = string.Empty;
    }

    public Guid Id { get; private set; }
    public string NomeTitular { get; private set; }
    public string CodigoAmigavel { get; private set; }
    public decimal Saldo { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public IReadOnlyCollection<MovimentacaoCartao> Movimentacoes => _movimentacoes;

    public static Cartao Solicitar(string nomeTitular)
    {
        var nome = nomeTitular?.Trim() ?? string.Empty;
        if (nome.Length is < 2 or > 80)
        {
            throw new NomeTitularInvalidoException();
        }

        return new Cartao
        {
            Id = Guid.NewGuid(),
            NomeTitular = nome,
            CodigoAmigavel = GerarCodigoAmigavel(),
            Saldo = 0m,
            DataCriacao = DateTime.UtcNow
        };
    }

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

    public MovimentacaoCartao Comprar(Produto produto)
    {
        ArgumentNullException.ThrowIfNull(produto);

        if (Saldo < produto.Preco)
        {
            throw new SaldoInsuficienteException();
        }

        Saldo -= produto.Preco;
        var movimentacao = MovimentacaoCartao.CriarCompra(Id, produto.Id, produto.Nome, produto.Preco);
        _movimentacoes.Add(movimentacao);
        return movimentacao;
    }

    private static string GerarCodigoAmigavel()
    {
        const string alfabeto = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        Span<char> caracteres = stackalloc char[6];
        Random.Shared.GetItems(alfabeto.AsSpan(), caracteres);
        return $"CP-{new string(caracteres)}";
    }
}
