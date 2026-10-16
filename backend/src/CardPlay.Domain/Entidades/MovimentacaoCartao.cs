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
    public decimal? PrecoUnitario { get; private set; }
    public DateTime? DataCancelamento { get; private set; }
    public Guid? CompraId { get; private set; }

    internal static MovimentacaoCartao Criar(Guid cartaoId, decimal valor, string descricao)
    {
        return new MovimentacaoCartao(cartaoId, valor, descricao);
    }

    internal static MovimentacaoCartao CriarCompra(
        Guid cartaoId,
        Guid produtoId,
        string nomeProduto,
        decimal precoUnitario,
        int quantidade,
        decimal total)
    {
        var compra = new MovimentacaoCartao(cartaoId, total, "Compra")
        {
            ProdutoId = produtoId,
            NomeProduto = nomeProduto,
            Quantidade = quantidade,
            PrecoUnitario = precoUnitario
        };
        return compra;
    }

    internal static MovimentacaoCartao CriarEstorno(Guid cartaoId, Guid compraId, decimal valor)
    {
        return new MovimentacaoCartao(cartaoId, valor, "Estorno")
        {
            CompraId = compraId
        };
    }

    internal void MarcarCancelada(DateTime dataCancelamento)
    {
        DataCancelamento = dataCancelamento;
    }
}
