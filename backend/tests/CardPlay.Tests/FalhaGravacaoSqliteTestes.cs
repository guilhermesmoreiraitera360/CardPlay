using CardPlay.Application.Contratos.Repositorios;
using CardPlay.Application.Servicos;
using CardPlay.Domain.Entidades;
using CardPlay.Domain.Excecoes;
using CardPlay.Repository.Contexto;
using CardPlay.Repository.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace CardPlay.Tests;

public class FalhaGravacaoSqliteTestes
{
    private static readonly Guid AdesivoId = Guid.Parse("9c4f5d0b-3f7d-4e4a-ab5c-4d5e6f708192");

    [Fact]
    public async Task ComprarAsync_FalhaAoSalvar_RelendoSqlite_MantemSaldoENaoGravaCompra()
    {
        var caminho = Path.Combine(Path.GetTempPath(), $"cardplay-ace10-{Guid.NewGuid():N}.db");
        var opcoes = new DbContextOptionsBuilder<CardPlayDbContext>()
            .UseSqlite($"Data Source={caminho}")
            .Options;

        Guid cartaoId;
        try
        {
            await using (var preparo = new CardPlayDbContext(opcoes))
            {
                await preparo.Database.MigrateAsync();
                var cartao = Cartao.Solicitar("Falha Gravacao");
                cartao.Recarregar(100m, "Recarga");
                preparo.Cartoes.Add(cartao);
                await preparo.SaveChangesAsync();
                cartaoId = cartao.Id;
            }

            await using (var compra = new CardPlayDbContext(opcoes))
            {
                var servico = new CompraServico(
                    new RepositorioQueFalhaAoSalvar(new CartaoRepositorio(compra)),
                    new ProdutoRepositorio(compra));

                var excecao = await Assert.ThrowsAsync<CompraNaoConcluidaException>(() =>
                    servico.ComprarAsync(cartaoId, AdesivoId, 1m, 4m));

                Assert.Equal("A compra não foi concluída.", excecao.Message);
            }

            await using var leitura = new CardPlayDbContext(opcoes);
            var gravado = await leitura.Cartoes
                .Include(cartao => cartao.Movimentacoes)
                .SingleAsync(cartao => cartao.Id == cartaoId);

            Assert.Equal(100m, gravado.Saldo);
            Assert.DoesNotContain(gravado.Movimentacoes, movimentacao => movimentacao.ProdutoId != null);
        }
        finally
        {
            foreach (var sufixo in new[] { "", "-wal", "-shm" })
            {
                var arquivo = caminho + sufixo;
                if (File.Exists(arquivo))
                {
                    File.Delete(arquivo);
                }
            }
        }
    }

    private sealed class RepositorioQueFalhaAoSalvar : ICartaoRepositorio
    {
        private readonly ICartaoRepositorio _interno;

        public RepositorioQueFalhaAoSalvar(ICartaoRepositorio interno)
        {
            _interno = interno;
        }

        public Task<Cartao?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _interno.ObterPorIdAsync(id, cancellationToken);
        }

        public Task AdicionarAsync(Cartao cartao, CancellationToken cancellationToken = default)
        {
            return _interno.AdicionarAsync(cartao, cancellationToken);
        }

        public void AdicionarMovimentacao(MovimentacaoCartao movimentacao)
        {
            _interno.AdicionarMovimentacao(movimentacao);
        }

        public Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
        {
            throw new IOException("falha de teste antes de gravar");
        }
    }
}
