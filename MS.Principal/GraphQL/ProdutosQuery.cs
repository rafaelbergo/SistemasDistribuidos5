using MS.Principal.Services;

namespace MS.Principal.GraphQL;

public class ProdutosQuery
{
    public async Task<List<ProdutoGraphQL>> GetProdutosDisponiveis(EstoqueGraphQLClient estoqueClient, CancellationToken ct)
    {
        var produtos = await estoqueClient.GetProdutosDisponiveisAsync(ct);

        return produtos.Select(produto => new ProdutoGraphQL
        {
            Id = produto.Id,
            Descricao = produto.Descricao,
            QuantidadeDisponivel = produto.QuantidadeDisponivel,
            Preco = produto.Preco
        }).ToList();
    }
}