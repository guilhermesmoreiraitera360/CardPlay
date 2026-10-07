using CardPlay.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardPlay.Repository.Configuracoes;

public class MovimentacaoCartaoConfiguracao : IEntityTypeConfiguration<MovimentacaoCartao>
{
    public void Configure(EntityTypeBuilder<MovimentacaoCartao> builder)
    {
        builder.ToTable("MovimentacoesCartao");
        builder.HasKey(movimentacao => movimentacao.Id);
        builder.Property(movimentacao => movimentacao.Id)
            .ValueGeneratedNever();

        builder.Property(movimentacao => movimentacao.Valor)
            .HasColumnType("TEXT")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(movimentacao => movimentacao.Descricao)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(movimentacao => movimentacao.DataHora)
            .IsRequired();

        builder.Property(movimentacao => movimentacao.NomeProduto)
            .HasMaxLength(80);
    }
}
