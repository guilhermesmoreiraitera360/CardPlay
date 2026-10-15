namespace CardPlay.Domain.Excecoes;

public class EstouroDeTotalException : Exception
{
    public EstouroDeTotalException()
        : base("Não foi possível calcular o total sem estouro numérico.")
    {
    }
}
