namespace MS.Estoque.GraphQL;

public class ProdutoGraphQL
{
    public string Id { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public int QuantidadeDisponivel { get; set; }

    public decimal Preco { get; set; }
}