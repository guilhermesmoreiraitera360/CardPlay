namespace CardPlay.Domain.Excecoes;

public class CompraNaoConcluidaException : Exception
{
    public CompraNaoConcluidaException(Exception causa)
        : base("A compra não foi concluída.", causa)
    {
    }
}
