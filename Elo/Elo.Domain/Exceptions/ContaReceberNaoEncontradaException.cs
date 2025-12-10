namespace Elo.Domain.Exceptions;

public class ContaReceberNaoEncontradaException : DomainException
{
    public ContaReceberNaoEncontradaException(int id)
        : base($"Conta a receber com ID {id} não foi encontrada.")
    {
    }
}
