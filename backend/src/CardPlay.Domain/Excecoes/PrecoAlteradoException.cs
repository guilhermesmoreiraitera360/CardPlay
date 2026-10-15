namespace CardPlay.Domain.Excecoes;

public class PrecoAlteradoException : Exception
{
    public PrecoAlteradoException()
        : base("O preço deste produto mudou. Confirme novamente o total atualizado.")
    {
    }
}
