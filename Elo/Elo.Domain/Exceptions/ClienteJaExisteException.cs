namespace Elo.Domain.Exceptions;

public class ClienteJaExisteException : DomainException
{
    public ClienteJaExisteException(string message) : base(message)
    {
    }
}
