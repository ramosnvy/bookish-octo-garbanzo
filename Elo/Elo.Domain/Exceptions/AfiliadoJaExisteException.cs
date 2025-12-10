namespace Elo.Domain.Exceptions;

public class AfiliadoJaExisteException : DomainException
{
    public AfiliadoJaExisteException(string message) : base(message)
    {
    }
}
