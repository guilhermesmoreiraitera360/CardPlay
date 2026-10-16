using CardPlay.Application.Dtos;
using CardPlay.Domain.Entidades;

namespace CardPlay.Application.Mapeamentos;

public static class MapeadorCartao
{
    public static CartaoResposta ParaResposta(Cartao cartao)
    {
        return new CartaoResposta
        {
            Id = cartao.Id,
            NomeTitular = cartao.NomeTitular,
            CodigoAmigavel = cartao.CodigoAmigavel,
            Saldo = cartao.Saldo,
            DataCriacao = ComoUtc(cartao.DataCriacao)
        };
    }

    public static MovimentacaoResposta ParaResposta(MovimentacaoCartao movimentacao)
    {
        return new MovimentacaoResposta
        {
            Id = movimentacao.Id,
            Valor = movimentacao.Valor,
            Descricao = movimentacao.Descricao,
            DataHora = ComoUtc(movimentacao.DataHora),
            ProdutoId = movimentacao.ProdutoId,
            NomeProduto = movimentacao.NomeProduto,
            Quantidade = movimentacao.Quantidade,
            PrecoUnitario = movimentacao.PrecoUnitario,
            DataCancelamento = ComoUtc(movimentacao.DataCancelamento),
            CompraId = movimentacao.CompraId
        };
    }

    private static DateTime ComoUtc(DateTime data)
    {
        return data.Kind == DateTimeKind.Utc ? data : DateTime.SpecifyKind(data, DateTimeKind.Utc);
    }

    private static DateTime? ComoUtc(DateTime? data)
    {
        return data is null ? null : ComoUtc(data.Value);
    }
}
