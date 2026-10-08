namespace CardPlay.Domain.Entidades;

public class MovimentacaoCartao
{
    private MovimentacaoCartao()
    {
        Descricao = string.Empty;
    }

    private MovimentacaoCartao(Guid cartaoId, decimal valor, string descricao)
    {
        Id = Guid.NewGuid();
        CartaoId = cartaoId;
        Valor = valor;
        Descricao = descricao;
        DataHora = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid CartaoId { get; private set; }
    public decimal Valor { get; private set; }
    public string Descricao { get; private set; }
    public DateTime DataHora { get; private set; }
    public Guid? ProdutoId { get; private set; }
    public string? NomeProduto { get; private set; }
    public int? Quantidade { get; private set; }

    internal static MovimentacaoCartao Criar(Guid cartaoId, decimal valor, string descricao)
    {
        return new MovimentacaoCartao(cartaoId, valor, descricao);
    }

    internal static MovimentacaoCartao CriarCompra(
        Guid cartaoId,
        Guid produtoId,
        string nomeProduto,
        decimal valor)
    {
        var compra = new MovimentacaoCartao(cartaoId, valor, "Compra")
        {
            ProdutoId = produtoId,
            NomeProduto = nomeProduto,
            Quantidade = 1
        };
        return compra;
    }
}
