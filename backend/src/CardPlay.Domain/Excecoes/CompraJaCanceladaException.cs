namespace CardPlay.Domain.Excecoes;

public class CompraJaCanceladaException : Exception
{
    public CompraJaCanceladaException()
        : base("Esta compra já foi cancelada.")
    {
    }
}
