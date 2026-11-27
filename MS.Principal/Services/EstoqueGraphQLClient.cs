using Foundation.Models;
using System.Text.Json;

namespace MS.Principal.Services;

public class EstoqueGraphQLClient
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public EstoqueGraphQLClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ProdutoCatalogoDto>> GetProdutosDisponiveisAsync(CancellationToken ct = default)
    {
        const string query = """
            query {
                produtosDisponiveis {
                    id
                    descricao
                    quantidadeDisponivel
                    preco
                }
            }
            """;

        var response = await ExecutarQueryAsync<ProdutosData>(query, ct);

        return response?.ProdutosDisponiveis ?? [];
    }

    public async Task<ProdutoCatalogoDto?> GetProdutoAsync(string id, CancellationToken ct = default)
    {
        const string query = """
            query ($id: String!) {
                produto(id: $id) {
                    id
                    descricao
                    quantidadeDisponivel
                    preco
                }
            }
            """;

        var response = await ExecutarQueryAsync<ProdutoData>(query, ct, new { id });

        return response?.Produto;
    }

    private async Task<TData?> ExecutarQueryAsync<TData>(string query, CancellationToken ct, object? variables = null)
    {
        var request = new
        {
            query,
            variables
        };

        using var response = await _httpClient.PostAsJsonAsync("/graphql", request, ct);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GraphQLResponse<TData>>(JsonOptions, ct);

        if (result?.Errors is { Count: > 0 })
        {
            var mensagem = string.Join("; ", result.Errors.Select(error => error.Message));

            throw new InvalidOperationException($"Erro GraphQL no MS Estoque: {mensagem}");
        }

        if (result.Data == null)
        {
            throw new InvalidOperationException("O MS Estoque retornou uma resposta GraphQL sem dados.");
        }

        return result.Data;
    }

    private sealed class GraphQLResponse<TData>
    {
        public TData? Data { get; set; }
        public List<GraphQLError>? Errors { get; set; }
    }

    private sealed class GraphQLError
    {
        public string Message { get; set; } = string.Empty;
    }

    private sealed class ProdutosData
    {
        public List<ProdutoCatalogoDto> ProdutosDisponiveis { get; set; } = [];
    }

    private sealed class ProdutoData
    {
        public ProdutoCatalogoDto? Produto { get; set; }
    }
}