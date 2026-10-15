namespace CardPlay.Application.Dtos;

public class CompraRequisicao
{
    public Guid ProdutoId { get; set; }
    public decimal Quantidade { get; set; }
    public decimal PrecoUnitarioVisto { get; set; }
}
