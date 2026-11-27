namespace MS.Estoque.GraphQL;

using MS.Estoque.Services;
using HotChocolate;

public class ProdutosQuery
{
    public List<ProdutoGraphQL> GetProdutosDisponiveis(EstoqueManager estoqueManager)
    {
        lock (estoqueManager.InventoryLock)
        {
            return estoqueManager.Inventory
                .Where(item => item.Value.Quantidade > 0)
                .OrderBy(item => item.Key)
                .Select(item => new ProdutoGraphQL
                {
                    Id = item.Key,
                    Descricao = $"Produto {item.Key}",
                    QuantidadeDisponivel = item.Value.Quantidade,
                    Preco = item.Value.Preco
                })
                .ToList();
        }
    }

    public ProdutoGraphQL? GetProduto(string id, EstoqueManager estoqueManager)
    {
        lock (estoqueManager.InventoryLock)
        {
            if (!estoqueManager.Inventory.TryGetValue(id, out var produto))
                return null;

            return new ProdutoGraphQL
            {
                Id = id,
                Descricao = $"Produto {id}",
                QuantidadeDisponivel = produto.Quantidade,
                Preco = produto.Preco
            };
        }
    }
}