namespace Elo.Domain.Exceptions;

public class ContaPagarNaoEncontradaException : DomainException
{
    public ContaPagarNaoEncontradaException(int id)
        : base($"Conta a pagar com ID {id} não foi encontrada.")
    {
    }
}
