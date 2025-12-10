namespace Elo.Domain.Exceptions;

public class AfiliadoNaoEncontradoException : DomainException
{
    public AfiliadoNaoEncontradoException(int id) 
        : base($"Afiliado com ID {id} não foi encontrado.")
    {
    }
}
