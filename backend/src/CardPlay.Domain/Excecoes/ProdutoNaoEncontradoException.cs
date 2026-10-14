namespace CardPlay.Domain.Excecoes;

public class ProdutoNaoEncontradoException : Exception
{
    public ProdutoNaoEncontradoException(Guid id)
        : base($"Produto {id} não foi encontrado.")
    {
        Id = id;
    }

    public Guid Id { get; }
}
