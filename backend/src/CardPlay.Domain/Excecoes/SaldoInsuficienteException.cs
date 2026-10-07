namespace CardPlay.Domain.Excecoes;

public class SaldoInsuficienteException : Exception
{
    public SaldoInsuficienteException()
        : base("Saldo insuficiente para concluir a compra.")
    {
    }
}
