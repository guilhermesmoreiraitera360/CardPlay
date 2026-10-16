namespace CardPlay.Domain.Excecoes;

public class CancelamentoNaoConcluidoException : Exception
{
    public CancelamentoNaoConcluidoException(Exception causa)
        : base("O cancelamento não foi concluído.", causa)
    {
    }
}
