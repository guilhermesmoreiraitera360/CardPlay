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
