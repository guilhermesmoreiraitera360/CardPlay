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
            DataCriacao = cartao.DataCriacao
        };
    }

    public static MovimentacaoResposta ParaResposta(MovimentacaoCartao movimentacao)
    {
        return new MovimentacaoResposta
        {
            Id = movimentacao.Id,
            Valor = movimentacao.Valor,
            Descricao = movimentacao.Descricao,
            DataHora = movimentacao.DataHora,
            ProdutoId = movimentacao.ProdutoId,
            NomeProduto = movimentacao.NomeProduto,
            Quantidade = movimentacao.Quantidade
        };
    }
}
