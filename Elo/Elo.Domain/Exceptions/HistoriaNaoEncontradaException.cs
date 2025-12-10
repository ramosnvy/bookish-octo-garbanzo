namespace Elo.Domain.Exceptions;

public class HistoriaNaoEncontradaException : DomainException
{
    public HistoriaNaoEncontradaException(int id) 
        : base($"Historia com ID {id} não foi encontrada.")
    {
    }
}
