using CardPlay.Domain.Entidades;
using CardPlay.Domain.Excecoes;

namespace CardPlay.Tests;

public class CartaoTestes
{
    [Fact]
    public void Solicitar_ComNomeValido_CriaCartaoComSaldoZero()
    {
        var cartao = Cartao.Solicitar("Ana Silva");

        Assert.NotEqual(Guid.Empty, cartao.Id);
        Assert.Equal("Ana Silva", cartao.NomeTitular);
        Assert.StartsWith("CP-", cartao.CodigoAmigavel);
        Assert.Equal(9, cartao.CodigoAmigavel.Length);
        Assert.Equal(0m, cartao.Saldo);
        Assert.Empty(cartao.Movimentacoes);
        Assert.True(cartao.DataCriacao <= DateTime.UtcNow);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("A")]
    public void Solicitar_ComNomeInvalido_Rejeita(string nome)
    {
        Assert.Throws<NomeTitularInvalidoException>(() => Cartao.Solicitar(nome));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-15.5)]
    public void Recarregar_ValorIgualOuInferiorAZero_Rejeita(decimal valor)
    {
        var cartao = Cartao.Solicitar("Bruno");

        Assert.Throws<RecargaInvalidaException>(() => cartao.Recarregar(valor, "Tentativa"));
        Assert.Equal(0m, cartao.Saldo);
        Assert.Empty(cartao.Movimentacoes);
    }

    [Fact]
    public void Recarregar_ValorValido_AtualizaSaldo()
    {
        var cartao = Cartao.Solicitar("Carla");

        cartao.Recarregar(50.25m, "Recarga inicial");

        Assert.Equal(50.25m, cartao.Saldo);
    }

    [Fact]
    public void Recarregar_ValorValido_RegistraMovimentacao()
    {
        var cartao = Cartao.Solicitar("Diego");

        cartao.Recarregar(20m, "Recarga da semana");

        var movimentacao = Assert.Single(cartao.Movimentacoes);
        Assert.Equal(20m, movimentacao.Valor);
        Assert.Equal("Recarga da semana", movimentacao.Descricao);
        Assert.Equal(cartao.Id, movimentacao.CartaoId);
        Assert.True(movimentacao.DataHora <= DateTime.UtcNow);
        Assert.Null(movimentacao.ProdutoId);
        Assert.Null(movimentacao.NomeProduto);
        Assert.Null(movimentacao.Quantidade);
        Assert.Null(movimentacao.PrecoUnitario);
    }

    [Fact]
    public void Recarregar_SemDescricao_UsaDescricaoPadrao()
    {
        var cartao = Cartao.Solicitar("Elena");

        cartao.Recarregar(10m, "   ");

        Assert.Equal("Recarga", Assert.Single(cartao.Movimentacoes).Descricao);
    }

    [Fact]
    public void Comprar_SaldoMaiorQuePreco_DebitaPrecoERegistraCompra()
    {
        var cartao = Cartao.Solicitar("Helena");
        cartao.Recarregar(50m, "Recarga");
        var produto = ProdutoDeTeste(8.50m);
        var antes = DateTime.UtcNow;

        var compra = cartao.Comprar(produto);

        Assert.Equal(41.50m, cartao.Saldo);
        Assert.True(cartao.Saldo >= 0m);
        Assert.Equal(2, cartao.Movimentacoes.Count);
        Assert.Equal(produto.Id, compra.ProdutoId);
        Assert.Equal(produto.Nome, compra.NomeProduto);
        Assert.Equal(1, compra.Quantidade);
        Assert.Equal(produto.Preco, compra.PrecoUnitario);
        Assert.Equal(produto.Preco, compra.Valor);
        Assert.Equal(cartao.Id, compra.CartaoId);
        Assert.Equal("Compra", compra.Descricao);
        Assert.InRange(compra.DataHora, antes, DateTime.UtcNow);
    }

    [Fact]
    public void Comprar_SaldoIgualAoPreco_ConcluiComSaldoZero()
    {
        var cartao = Cartao.Solicitar("Igor");
        cartao.Recarregar(4m, "Recarga");
        var produto = ProdutoDeTeste(4m);

        var compra = cartao.Comprar(produto);

        Assert.Equal(0m, cartao.Saldo);
        Assert.Equal(produto.Preco, compra.PrecoUnitario);
        Assert.Equal(produto.Preco, compra.Valor);
        Assert.Equal(1, compra.Quantidade);
        Assert.Equal(2, cartao.Movimentacoes.Count);
    }

    [Fact]
    public void Comprar_SaldoMenorQuePreco_NaoAlteraSaldoNemRegistraCompra()
    {
        var cartao = Cartao.Solicitar("Julia");
        cartao.Recarregar(10m, "Recarga");
        var produto = ProdutoDeTeste(18.75m);

        Assert.Throws<SaldoInsuficienteException>(() => cartao.Comprar(produto));

        Assert.Equal(10m, cartao.Saldo);
        var recarga = Assert.Single(cartao.Movimentacoes);
        Assert.Null(recarga.ProdutoId);
        Assert.Null(recarga.Quantidade);
        Assert.Null(recarga.PrecoUnitario);
    }

    [Fact]
    public void Comprar_Quantidade3_SaldoMaiorQueTotal_DebitaTotalERegistraUmaCompra()
    {
        var cartao = Cartao.Solicitar("Helena");
        cartao.Recarregar(100m, "Recarga");
        var produto = ProdutoDeTeste(20m);

        var compra = cartao.Comprar(produto, 3m);

        Assert.Equal(40m, cartao.Saldo);
        Assert.Equal(2, cartao.Movimentacoes.Count);
        Assert.Equal(3, compra.Quantidade);
        Assert.Equal(20m, compra.PrecoUnitario);
        Assert.Equal(60m, compra.Valor);
        Assert.Equal(produto.Nome, compra.NomeProduto);
    }

    [Fact]
    public void Comprar_Quantidade3_SaldoIgualAoTotal_ConcluiComSaldoZero()
    {
        var cartao = Cartao.Solicitar("Igor");
        cartao.Recarregar(60m, "Recarga");
        var produto = ProdutoDeTeste(20m);

        var compra = cartao.Comprar(produto, 3m);

        Assert.Equal(0m, cartao.Saldo);
        Assert.Equal(3, compra.Quantidade);
        Assert.Equal(60m, compra.Valor);
    }

    [Fact]
    public void Comprar_Quantidade3_SaldoMenorQueTotal_NaoAlteraSaldoNemRegistraCompra()
    {
        var cartao = Cartao.Solicitar("Julia");
        cartao.Recarregar(50m, "Recarga");
        var produto = ProdutoDeTeste(20m);

        Assert.Throws<SaldoInsuficienteException>(() => cartao.Comprar(produto, 3m));

        Assert.Equal(50m, cartao.Saldo);
        Assert.Null(Assert.Single(cartao.Movimentacoes).ProdutoId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1.5)]
    public void Comprar_QuantidadeInvalida_NaoAlteraSaldoNemRegistraCompra(decimal quantidade)
    {
        var cartao = Cartao.Solicitar("Lia");
        cartao.Recarregar(100m, "Recarga");
        var produto = ProdutoDeTeste(20m);

        var excecao = Assert.Throws<QuantidadeInvalidaException>(() => cartao.Comprar(produto, quantidade));

        Assert.Equal("Informe uma quantidade inteira maior que zero.", excecao.Message);
        Assert.Equal(100m, cartao.Saldo);
        Assert.Null(Assert.Single(cartao.Movimentacoes).ProdutoId);
    }

    [Fact]
    public void Comprar_TotalEstouraDecimal_NaoAlteraSaldoNemRegistraCompra()
    {
        var cartao = Cartao.Solicitar("Miguel");
        cartao.Recarregar(100m, "Recarga");
        var produto = ProdutoDeTeste(decimal.MaxValue);

        var excecao = Assert.Throws<EstouroDeTotalException>(() => cartao.Comprar(produto, 2m));

        Assert.Equal("Não foi possível calcular o total sem estouro numérico.", excecao.Message);
        Assert.Equal(100m, cartao.Saldo);
        Assert.Null(Assert.Single(cartao.Movimentacoes).ProdutoId);
    }

    [Fact]
    public void CancelarCompra_Saldo30AposCompraDe20_Credita20MarcaECriaUmEstorno()
    {
        var cartao = Cartao.Solicitar("Nara");
        cartao.Recarregar(50m, "Recarga");
        var compra = cartao.Comprar(ProdutoDeTeste(20m));
        var antes = DateTime.UtcNow;

        var estorno = cartao.CancelarCompra(compra.Id);

        Assert.Equal(50m, cartao.Saldo);
        Assert.Equal(20m, compra.Valor);
        Assert.Equal(1, compra.Quantidade);
        Assert.Equal(20m, compra.PrecoUnitario);
        Assert.Equal("Compra", compra.Descricao);
        Assert.NotNull(compra.DataCancelamento);
        Assert.InRange(compra.DataCancelamento.Value, antes, DateTime.UtcNow);
        Assert.Equal(20m, estorno.Valor);
        Assert.Equal("Estorno", estorno.Descricao);
        Assert.Equal(compra.Id, estorno.CompraId);
        Assert.Equal(cartao.Id, estorno.CartaoId);
        Assert.Null(estorno.NomeProduto);
        Assert.Null(estorno.ProdutoId);
        Assert.Null(estorno.Quantidade);
        Assert.Null(estorno.PrecoUnitario);
        Assert.Equal(3, cartao.Movimentacoes.Count);
    }

    [Fact]
    public void CancelarCompra_QuantidadeMaiorQueUm_CreditaOTotalUmaVez()
    {
        var cartao = Cartao.Solicitar("Otto");
        cartao.Recarregar(100m, "Recarga");
        var compra = cartao.Comprar(ProdutoDeTeste(20m), 3m);

        var estorno = cartao.CancelarCompra(compra.Id);

        Assert.Equal(100m, cartao.Saldo);
        Assert.Equal(60m, estorno.Valor);
        Assert.Equal(3, compra.Quantidade);
        Assert.Equal(20m, compra.PrecoUnitario);
        Assert.Equal(60m, compra.Valor);
    }

    [Fact]
    public void CancelarCompra_Repetida_NaoCreditaDeNovoNemCriaOutroEstorno()
    {
        var cartao = Cartao.Solicitar("Paula");
        cartao.Recarregar(50m, "Recarga");
        var compra = cartao.Comprar(ProdutoDeTeste(20m));
        cartao.CancelarCompra(compra.Id);

        var excecao = Assert.Throws<CompraJaCanceladaException>(() => cartao.CancelarCompra(compra.Id));

        Assert.Equal("Esta compra já foi cancelada.", excecao.Message);
        Assert.Equal(50m, cartao.Saldo);
        Assert.Equal(3, cartao.Movimentacoes.Count);
        Assert.Single(cartao.Movimentacoes, movimentacao => movimentacao.Descricao == "Estorno");
    }

    [Fact]
    public void CancelarCompra_IdAusenteOuRecarga_NaoAlteraOCartao()
    {
        var cartao = Cartao.Solicitar("Rita");
        cartao.Recarregar(50m, "Recarga");
        var recarga = Assert.Single(cartao.Movimentacoes);

        var idAusente = Assert.Throws<CompraNaoCancelavelException>(() => cartao.CancelarCompra(Guid.NewGuid()));
        var recargaRecusada = Assert.Throws<CompraNaoCancelavelException>(() => cartao.CancelarCompra(recarga.Id));

        Assert.Equal("Não há compra deste cartão para cancelar.", idAusente.Message);
        Assert.Equal(idAusente.Message, recargaRecusada.Message);
        Assert.Equal(50m, cartao.Saldo);
        Assert.Null(Assert.Single(cartao.Movimentacoes).ProdutoId);
    }

    [Fact]
    public void CancelarCompra_NaoUsaPrecoDeOutroProduto()
    {
        var cartao = Cartao.Solicitar("Sara");
        cartao.Recarregar(50m, "Recarga");
        var compra = cartao.Comprar(ProdutoDeTeste(20m));
        _ = ProdutoDeTeste(25m);

        var estorno = cartao.CancelarCompra(compra.Id);

        Assert.Equal(20m, estorno.Valor);
        Assert.Equal(50m, cartao.Saldo);
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
}
