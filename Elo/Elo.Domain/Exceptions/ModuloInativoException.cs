namespace Elo.Domain.Exceptions;

public class ModuloInativoException : DomainException
{
    public ModuloInativoException(int moduloId)
        : base($"O módulo com ID {moduloId} está inativo.")
    {
    }

    public ModuloInativoException(string moduloNome)
        : base($"O módulo '{moduloNome}' está inativo.")
    {
    }
}
