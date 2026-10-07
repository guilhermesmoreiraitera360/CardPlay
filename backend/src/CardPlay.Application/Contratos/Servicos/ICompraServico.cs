using CardPlay.Application.Dtos;

namespace CardPlay.Application.Contratos.Servicos;

public interface ICompraServico
{
    Task<CartaoResposta> ComprarAsync(
        Guid cartaoId,
        Guid produtoId,
        CancellationToken cancellationToken = default);
}
