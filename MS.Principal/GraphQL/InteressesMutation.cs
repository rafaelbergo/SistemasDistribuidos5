using Foundation.Models;
using Foundation.Services;

namespace MS.Principal.GraphQL;

public partial class PedidosMutation
{
    public async Task<InteresseGraphQLResponse> RegistrarInteresse(
        InteressePromocaoRequest input,
        RabbitMqPublisher rabbitPublisher)
    {
        InteressePromocao interesse;

        try
        {
            interesse = new InteressePromocao
            {
                Email = input.Email,
                Categorias = input.Categorias
            }.Normalizar();
        }
        catch (ArgumentException ex)
        {
            throw new GraphQLException(ErrorBuilder.New()
                .SetMessage(ex.Message)
                .SetCode("INTERESSE_INVALIDO")
                .Build());
        }

        try
        {
            await rabbitPublisher.PublishEventAsync(
                interesse,
                "interesse.promocao");

            return new InteresseGraphQLResponse
            {
                Mensagem = "Solicitação de interesse enviada para processamento.",
                Email = interesse.Email,
                Categorias = interesse.Categorias
            };
        }
        catch (Exception)
        {
            throw new GraphQLException(ErrorBuilder.New()
                .SetMessage("Não foi possível enviar o interesse para processamento.")
                .SetCode("ERRO_PUBLICACAO_INTERESSE")
                .Build());
        }
    }

    public async Task<InteresseGraphQLResponse> CancelarInteresse(
        string email,
        RabbitMqPublisher rabbitPublisher)
    {
        InteressePromocao interesse;

        try
        {
            interesse = new InteressePromocao
            {
                Email = email,
                Cancelado = true
            }.Normalizar();
        }
        catch (ArgumentException ex)
        {
            throw new GraphQLException(ErrorBuilder.New()
                .SetMessage(ex.Message)
                .SetCode("INTERESSE_INVALIDO")
                .Build());
        }

        try
        {
            await rabbitPublisher.PublishEventAsync(
                interesse,
                "interesse.cancelado");

            return new InteresseGraphQLResponse
            {
                Mensagem = "Solicitação de cancelamento enviada para processamento.",
                Email = interesse.Email,
                Categorias = interesse.Categorias
            };
        }
        catch (Exception)
        {
            throw new GraphQLException(ErrorBuilder.New()
                .SetMessage("Não foi possível enviar o cancelamento para processamento.")
                .SetCode("ERRO_PUBLICACAO_CANCELAMENTO")
                .Build());
        }
    }
}

public class InteresseGraphQLResponse
{
    public string Mensagem { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Categorias { get; set; } = [];
}