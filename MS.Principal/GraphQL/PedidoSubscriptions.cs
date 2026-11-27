using MS.Principal.Services;

namespace MS.Principal.GraphQL;

public class PedidoSubscriptions
{
    [Subscribe]
    [Topic($"{{{nameof(clienteId)}}}")]
    public PedidoStatusEvento PedidoStatusAlterado(string clienteId, [EventMessage] PedidoStatusEvento evento)
    {
        return evento;
    }
}