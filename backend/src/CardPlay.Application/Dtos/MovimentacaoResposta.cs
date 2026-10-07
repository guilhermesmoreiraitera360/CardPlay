namespace CardPlay.Application.Dtos;

public class MovimentacaoResposta
{
    public Guid Id { get; init; }
    public decimal Valor { get; init; }
    public string Descricao { get; init; } = string.Empty;
    public DateTime DataHora { get; init; }
    public Guid? ProdutoId { get; init; }
    public string? NomeProduto { get; init; }
    public int? Quantidade { get; init; }
}
