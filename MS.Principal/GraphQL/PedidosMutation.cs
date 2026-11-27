using Foundation.Models;
using Foundation.Services;
using MS.Principal.Services;

namespace MS.Principal.GraphQL;

public partial class PedidosMutation
{
    public async Task<PedidoGraphQLResponse> CriarPedido(PedidoRequest input, EstoqueGraphQLClient estoqueClient, RabbitMqPublisher rabbitPublisher, CancellationToken ct)
    {
        if (input.Itens == null || !input.Itens.Any())
        {
            throw new GraphQLException(ErrorBuilder.New()
                .SetMessage("O pedido deve possuir pelo menos um item.")
                .SetCode("PEDIDO_SEM_ITENS")
                .Build());
        }

        if (string.IsNullOrWhiteSpace(input.ClienteId))
        {
            throw new GraphQLException(ErrorBuilder.New()
                .SetMessage("O cliente deve ser informado.")
                .SetCode("CLIENTE_INVALIDO")
                .Build());
        }

        if (input.Itens.Any(item => string.IsNullOrWhiteSpace(item.Id) || item.Quantidade <= 0))
        {
            throw new GraphQLException(ErrorBuilder.New()
                .SetMessage("Todos os itens devem possuir ID e quantidade positiva.")
                .SetCode("ITEM_INVALIDO")
                .Build());
        }

        decimal totalValue = 0;
        var itensPedido = new List<ItemPedido>();

        foreach (var reqItem in input.Itens)
        {
            var produto = await estoqueClient.GetProdutoAsync(reqItem.Id, ct);

            if (produto == null)
            {
                throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage($"Produto {reqItem.Id} não encontrado.")
                    .SetCode("PRODUTO_NAO_ENCONTRADO")
                    .Build());
            }

            totalValue += produto.Preco * reqItem.Quantidade;

            itensPedido.Add(new ItemPedido
            {
                Id = reqItem.Id,
                Quantidade = reqItem.Quantidade,
                Preco = produto.Preco
            });
        }

        string pedidoId = Guid.NewGuid().ToString("N");

        var pedido = new PedidoCriado
        {
            Id = pedidoId,
            ClienteId = input.ClienteId,
            Itens = itensPedido,
            ValorTotal = totalValue
        };

        try
        {
            await rabbitPublisher.PublishEventAsync(pedido, "pedido.criado");

            return new PedidoGraphQLResponse
            {
                Mensagem = "Pedido criado e enviado para processamento.",
                PedidoId = pedidoId,
                TotalValue = totalValue
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MS.Principal] Erro ao publicar pedido: {ex.Message}");

            throw new GraphQLException(ErrorBuilder.New()
                .SetMessage("Não foi possível publicar o pedido.")
                .SetCode("ERRO_PUBLICACAO_PEDIDO")
                .Build());
        }
    }
}

public class PedidoGraphQLResponse
{
    public string Mensagem { get; set; } = string.Empty;
    public string PedidoId { get; set; } = string.Empty;
    public decimal TotalValue { get; set; }
}