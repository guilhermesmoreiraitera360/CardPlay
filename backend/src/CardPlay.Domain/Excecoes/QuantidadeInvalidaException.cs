namespace CardPlay.Domain.Excecoes;

public class QuantidadeInvalidaException : Exception
{
    public QuantidadeInvalidaException()
        : base("Informe uma quantidade inteira maior que zero.")
    {
    }
}
