using CardPlay.Application.Contratos.Repositorios;
using CardPlay.Application.Servicos;
using CardPlay.Domain.Entidades;
using CardPlay.Domain.Excecoes;

namespace CardPlay.Tests;

public class CompraServicoTestes
{
    [Fact]
    public async Task ComprarAsync_SaldoMaiorQuePreco_DebitaEGravaUmaVez()
    {
        var cartoes = new CartaoRepositorioEmMemoria();
        var produtos = new ProdutoRepositorioEmMemoria();
        var produto = ProdutoDeTeste(8.50m);
        produtos.Adicionar(produto);
        var cartao = CartaoComSaldo(50m);
        await cartoes.AdicionarAsync(cartao);
        var servico = new CompraServico(cartoes, produtos);

        var resposta = await servico.ComprarAsync(cartao.Id, produto.Id);

        Assert.Equal(41.50m, resposta.Saldo);
        Assert.True(resposta.Saldo >= 0m);
        Assert.Equal(cartao.Id, resposta.CartaoId);
        Assert.Equal(produto.Id, resposta.Registro.ProdutoId);
        Assert.Equal(produto.Nome, resposta.Registro.NomeProduto);
        Assert.Equal(1, resposta.Registro.Quantidade);
        Assert.Equal(produto.Preco, resposta.Registro.Valor);
        Assert.Equal(1, cartoes.VezesSalvo);
        var compra = Assert.Single(cartao.Movimentacoes, movimentacao => movimentacao.ProdutoId == produto.Id);
        Assert.Equal(produto.Nome, compra.NomeProduto);
        Assert.Equal(1, compra.Quantidade);
        Assert.Equal(produto.Preco, compra.Valor);
    }

    [Fact]
    public async Task ComprarAsync_SaldoIgualAoPreco_GravaComSaldoZero()
    {
        var cartoes = new CartaoRepositorioEmMemoria();
        var produtos = new ProdutoRepositorioEmMemoria();
        var produto = ProdutoDeTeste(4m);
        produtos.Adicionar(produto);
        var cartao = CartaoComSaldo(4m);
        await cartoes.AdicionarAsync(cartao);
        var servico = new CompraServico(cartoes, produtos);

        var resposta = await servico.ComprarAsync(cartao.Id, produto.Id);

        Assert.Equal(0m, resposta.Saldo);
        Assert.Equal(1, cartoes.VezesSalvo);
        Assert.Single(cartao.Movimentacoes, movimentacao => movimentacao.ProdutoId == produto.Id);
    }

    [Fact]
    public async Task ComprarAsync_SaldoMenorQuePreco_NaoGrava()
    {
        var cartoes = new CartaoRepositorioEmMemoria();
        var produtos = new ProdutoRepositorioEmMemoria();
        var produto = ProdutoDeTeste(18.75m);
        produtos.Adicionar(produto);
        var cartao = CartaoComSaldo(10m);
        await cartoes.AdicionarAsync(cartao);
        var servico = new CompraServico(cartoes, produtos);

        await Assert.ThrowsAsync<SaldoInsuficienteException>(() => servico.ComprarAsync(cartao.Id, produto.Id));

        Assert.Equal(10m, cartao.Saldo);
        Assert.Equal(0, cartoes.VezesSalvo);
        Assert.DoesNotContain(cartao.Movimentacoes, movimentacao => movimentacao.ProdutoId != null);
    }

    [Fact]
    public async Task ComprarAsync_CartaoAusente_NaoGrava()
    {
        var cartoes = new CartaoRepositorioEmMemoria();
        var produtos = new ProdutoRepositorioEmMemoria();
        var produto = ProdutoDeTeste(8.50m);
        produtos.Adicionar(produto);
        var servico = new CompraServico(cartoes, produtos);

        await Assert.ThrowsAsync<CartaoNaoEncontradoException>(() =>
            servico.ComprarAsync(Guid.NewGuid(), produto.Id));

        Assert.Equal(0, cartoes.VezesSalvo);
    }

    [Fact]
    public async Task ComprarAsync_ProdutoAusente_NaoGravaEMantemSaldo()
    {
        var cartoes = new CartaoRepositorioEmMemoria();
        var produtos = new ProdutoRepositorioEmMemoria();
        var cartao = CartaoComSaldo(50m);
        await cartoes.AdicionarAsync(cartao);
        var servico = new CompraServico(cartoes, produtos);

        await Assert.ThrowsAsync<ProdutoNaoEncontradoException>(() =>
            servico.ComprarAsync(cartao.Id, Guid.NewGuid()));

        Assert.Equal(50m, cartao.Saldo);
        Assert.Equal(0, cartoes.VezesSalvo);
        Assert.DoesNotContain(cartao.Movimentacoes, movimentacao => movimentacao.ProdutoId != null);
    }

    private static Cartao CartaoComSaldo(decimal saldo)
    {
        var cartao = Cartao.Solicitar("Helena");
        cartao.Recarregar(saldo, "Recarga");
        return cartao;
    }

    private static Produto ProdutoDeTeste(decimal preco)
    {
        return new Produto(
            Guid.NewGuid(),
            "Café especial",
            "Um espresso.",
            preco,
            "☕",
            true);
    }

    private sealed class CartaoRepositorioEmMemoria : ICartaoRepositorio
    {
        private readonly Dictionary<Guid, Cartao> _cartoes = [];

        public int VezesSalvo { get; private set; }

        public Task<Cartao?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _cartoes.TryGetValue(id, out var cartao);
            return Task.FromResult(cartao);
        }

        public Task AdicionarAsync(Cartao cartao, CancellationToken cancellationToken = default)
        {
            _cartoes[cartao.Id] = cartao;
            return Task.CompletedTask;
        }

        public void AdicionarMovimentacao(MovimentacaoCartao movimentacao)
        {
        }

        public Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
        {
            VezesSalvo++;
            return Task.CompletedTask;
        }
    }

    private sealed class ProdutoRepositorioEmMemoria : IProdutoRepositorio
    {
        private readonly Dictionary<Guid, Produto> _produtos = [];

        public void Adicionar(Produto produto)
        {
            _produtos[produto.Id] = produto;
        }

        public Task<IReadOnlyCollection<Produto>> ListarAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<Produto>>(_produtos.Values.ToArray());
        }

        public Task<Produto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _produtos.TryGetValue(id, out var produto);
            return Task.FromResult(produto);
        }
    }
}
