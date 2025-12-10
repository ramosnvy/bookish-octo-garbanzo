namespace Elo.Domain.Exceptions;

public class TicketNaoEncontradoException : DomainException
{
    public TicketNaoEncontradoException(int id) 
        : base($"Ticket com ID {id} não foi encontrado.")
    {
    }
}
