using CardPlay.Application.Dtos;

namespace CardPlay.Application.Contratos.Servicos;

public interface ICompraServico
{
    Task<CompraResposta> ComprarAsync(
        Guid cartaoId,
        Guid produtoId,
        CancellationToken cancellationToken = default);
}
