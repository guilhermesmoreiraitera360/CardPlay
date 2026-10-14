using CardPlay.Domain.Entidades;

namespace CardPlay.Application.Contratos.Repositorios;

public interface IProdutoRepositorio
{
    Task<IReadOnlyCollection<Produto>> ListarAsync(CancellationToken cancellationToken = default);
    Task<Produto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
}
