namespace CardPlay.Application.Dtos;

public class CompraResposta
{
    public Guid CartaoId { get; init; }
    public decimal Saldo { get; init; }
    public MovimentacaoResposta Registro { get; init; } = new();
}
