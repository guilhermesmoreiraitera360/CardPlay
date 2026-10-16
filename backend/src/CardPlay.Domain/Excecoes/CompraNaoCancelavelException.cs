namespace CardPlay.Domain.Excecoes;

public class CompraNaoCancelavelException : Exception
{
    public CompraNaoCancelavelException()
        : base("Não há compra deste cartão para cancelar.")
    {
    }
}
