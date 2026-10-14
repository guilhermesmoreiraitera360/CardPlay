using CardPlay.Application.Contratos.Repositorios;
using CardPlay.Application.Contratos.Servicos;
using CardPlay.Application.Dtos;
using CardPlay.Application.Mapeamentos;
using CardPlay.Domain.Excecoes;

namespace CardPlay.Application.Servicos;

public class CompraServico : ICompraServico
{
    private readonly ICartaoRepositorio _cartaoRepositorio;
    private readonly IProdutoRepositorio _produtoRepositorio;

    public CompraServico(ICartaoRepositorio cartaoRepositorio, IProdutoRepositorio produtoRepositorio)
    {
        _cartaoRepositorio = cartaoRepositorio;
        _produtoRepositorio = produtoRepositorio;
    }

    public async Task<CompraResposta> ComprarAsync(
        Guid cartaoId,
        Guid produtoId,
        CancellationToken cancellationToken = default)
    {
        var cartao = await _cartaoRepositorio.ObterPorIdAsync(cartaoId, cancellationToken);
        if (cartao is null)
        {
            throw new CartaoNaoEncontradoException(cartaoId);
        }

        var produto = await _produtoRepositorio.ObterPorIdAsync(produtoId, cancellationToken);
        if (produto is null)
        {
            throw new ProdutoNaoEncontradoException(produtoId);
        }

        var movimentacao = cartao.Comprar(produto);
        _cartaoRepositorio.AdicionarMovimentacao(movimentacao);
        await _cartaoRepositorio.SalvarAlteracoesAsync(cancellationToken);
        return new CompraResposta
        {
            CartaoId = cartao.Id,
            Saldo = cartao.Saldo,
            Registro = MapeadorCartao.ParaResposta(movimentacao)
        };
    }
}
