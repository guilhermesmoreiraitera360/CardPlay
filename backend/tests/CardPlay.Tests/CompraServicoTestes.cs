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

        var resposta = await servico.ComprarAsync(cartao.Id, produto.Id, 1m, produto.Preco);

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
        Assert.Equal(produto.Preco, compra.PrecoUnitario);
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

        var resposta = await servico.ComprarAsync(cartao.Id, produto.Id, 1m, produto.Preco);

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

        await Assert.ThrowsAsync<SaldoInsuficienteException>(() =>
            servico.ComprarAsync(cartao.Id, produto.Id, 1m, produto.Preco));

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
            servico.ComprarAsync(Guid.NewGuid(), produto.Id, 1m, produto.Preco));

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
            servico.ComprarAsync(cartao.Id, Guid.NewGuid(), 1m, 8.50m));

        Assert.Equal(50m, cartao.Saldo);
        Assert.Equal(0, cartoes.VezesSalvo);
        Assert.DoesNotContain(cartao.Movimentacoes, movimentacao => movimentacao.ProdutoId != null);
    }

    [Fact]
    public async Task ComprarAsync_Quantidade3_SaldoMaiorQueTotal_DebitaTotalEGravaUmaVez()
    {
        var cartoes = new CartaoRepositorioEmMemoria();
        var produtos = new ProdutoRepositorioEmMemoria();
        var produto = ProdutoDeTeste(20m);
        produtos.Adicionar(produto);
        var cartao = CartaoComSaldo(100m);
        await cartoes.AdicionarAsync(cartao);
        var servico = new CompraServico(cartoes, produtos);

        var resposta = await servico.ComprarAsync(cartao.Id, produto.Id, 3m, 20m);

        Assert.Equal(40m, resposta.Saldo);
        Assert.Equal(3, resposta.Registro.Quantidade);
        Assert.Equal(60m, resposta.Registro.Valor);
        Assert.Equal(1, cartoes.VezesSalvo);
        var compra = Assert.Single(cartao.Movimentacoes, movimentacao => movimentacao.ProdutoId == produto.Id);
        Assert.Equal(20m, compra.PrecoUnitario);
        Assert.Equal(60m, compra.Valor);
    }

    [Fact]
    public async Task ComprarAsync_Quantidade3_SaldoIgualAoTotal_GravaComSaldoZero()
    {
        var cartoes = new CartaoRepositorioEmMemoria();
        var produtos = new ProdutoRepositorioEmMemoria();
        var produto = ProdutoDeTeste(20m);
        produtos.Adicionar(produto);
        var cartao = CartaoComSaldo(60m);
        await cartoes.AdicionarAsync(cartao);
        var servico = new CompraServico(cartoes, produtos);

        var resposta = await servico.ComprarAsync(cartao.Id, produto.Id, 3m, 20m);

        Assert.Equal(0m, resposta.Saldo);
        Assert.Equal(1, cartoes.VezesSalvo);
    }

    [Fact]
    public async Task ComprarAsync_Quantidade3_SaldoMenorQueTotal_NaoGrava()
    {
        var cartoes = new CartaoRepositorioEmMemoria();
        var produtos = new ProdutoRepositorioEmMemoria();
        var produto = ProdutoDeTeste(20m);
        produtos.Adicionar(produto);
        var cartao = CartaoComSaldo(50m);
        await cartoes.AdicionarAsync(cartao);
        var servico = new CompraServico(cartoes, produtos);

        await Assert.ThrowsAsync<SaldoInsuficienteException>(() =>
            servico.ComprarAsync(cartao.Id, produto.Id, 3m, 20m));

        Assert.Equal(50m, cartao.Saldo);
        Assert.Equal(0, cartoes.VezesSalvo);
        Assert.DoesNotContain(cartao.Movimentacoes, movimentacao => movimentacao.ProdutoId != null);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1.5)]
    public async Task ComprarAsync_QuantidadeInvalida_NaoGrava(decimal quantidade)
    {
        var cartoes = new CartaoRepositorioEmMemoria();
        var produtos = new ProdutoRepositorioEmMemoria();
        var produto = ProdutoDeTeste(20m);
        produtos.Adicionar(produto);
        var cartao = CartaoComSaldo(100m);
        await cartoes.AdicionarAsync(cartao);
        var servico = new CompraServico(cartoes, produtos);

        await Assert.ThrowsAsync<QuantidadeInvalidaException>(() =>
            servico.ComprarAsync(cartao.Id, produto.Id, quantidade, produto.Preco));

        Assert.Equal(100m, cartao.Saldo);
        Assert.Equal(0, cartoes.VezesSalvo);
        Assert.DoesNotContain(cartao.Movimentacoes, movimentacao => movimentacao.ProdutoId != null);
    }

    [Fact]
    public async Task ComprarAsync_PrecoVistoDiferente_NaoGrava()
    {
        var cartoes = new CartaoRepositorioEmMemoria();
        var produtos = new ProdutoRepositorioEmMemoria();
        var produto = ProdutoDeTeste(20m);
        produtos.Adicionar(produto);
        var cartao = CartaoComSaldo(100m);
        await cartoes.AdicionarAsync(cartao);
        var servico = new CompraServico(cartoes, produtos);

        var excecao = await Assert.ThrowsAsync<PrecoAlteradoException>(() =>
            servico.ComprarAsync(cartao.Id, produto.Id, 3m, 18m));

        Assert.Equal("O preço deste produto mudou. Confirme novamente o total atualizado.", excecao.Message);
        Assert.Equal(100m, cartao.Saldo);
        Assert.Equal(0, cartoes.VezesSalvo);
        Assert.DoesNotContain(cartao.Movimentacoes, movimentacao => movimentacao.ProdutoId != null);
    }

    [Fact]
    public async Task ComprarAsync_FalhaAoSalvar_InformaQueNaoConcluiuENaoGrava()
    {
        var cartoes = new CartaoRepositorioEmMemoria { FalharAoSalvar = true };
        var produtos = new ProdutoRepositorioEmMemoria();
        var produto = ProdutoDeTeste(20m);
        produtos.Adicionar(produto);
        var cartao = CartaoComSaldo(100m);
        await cartoes.AdicionarAsync(cartao);
        var servico = new CompraServico(cartoes, produtos);

        var excecao = await Assert.ThrowsAsync<CompraNaoConcluidaException>(() =>
            servico.ComprarAsync(cartao.Id, produto.Id, 3m, 20m));

        Assert.Equal("A compra não foi concluída.", excecao.Message);
        Assert.Equal(100m, cartoes.ObterSaldoGravado(cartao.Id));
        Assert.Equal(0, cartoes.VezesSalvo);
        Assert.DoesNotContain(
            cartoes.ObterMovimentacoesGravadas(cartao.Id),
            id => cartao.Movimentacoes.Single(movimentacao => movimentacao.Id == id).ProdutoId != null);
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
        private readonly Dictionary<Guid, EstadoGravado> _gravado = [];

        public int VezesSalvo { get; private set; }
        public bool FalharAoSalvar { get; init; }

        public decimal ObterSaldoGravado(Guid id)
        {
            return _gravado[id].Saldo;
        }

        public IReadOnlyCollection<Guid> ObterMovimentacoesGravadas(Guid id)
        {
            return _gravado[id].MovimentacaoIds;
        }

        public Task<Cartao?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _cartoes.TryGetValue(id, out var cartao);
            return Task.FromResult(cartao);
        }

        public Task AdicionarAsync(Cartao cartao, CancellationToken cancellationToken = default)
        {
            _cartoes[cartao.Id] = cartao;
            Gravar(cartao);
            return Task.CompletedTask;
        }

        public void AdicionarMovimentacao(MovimentacaoCartao movimentacao)
        {
        }

        public Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
        {
            if (FalharAoSalvar)
            {
                throw new InvalidOperationException("falha de teste");
            }

            VezesSalvo++;
            foreach (var cartao in _cartoes.Values)
            {
                Gravar(cartao);
            }

            return Task.CompletedTask;
        }

        private void Gravar(Cartao cartao)
        {
            _gravado[cartao.Id] = new EstadoGravado(
                cartao.Saldo,
                cartao.Movimentacoes.Select(movimentacao => movimentacao.Id).ToArray());
        }

        private sealed record EstadoGravado(decimal Saldo, IReadOnlyCollection<Guid> MovimentacaoIds);
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
