namespace Elo.Domain.Exceptions;

public class ClienteNaoEncontradoException : DomainException
{
    public ClienteNaoEncontradoException(int id) : base($"Cliente com ID {id} não foi encontrado")
    {
    }
}
