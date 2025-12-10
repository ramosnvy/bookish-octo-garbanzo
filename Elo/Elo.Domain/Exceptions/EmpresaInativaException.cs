namespace Elo.Domain.Exceptions;

public class EmpresaInativaException : DomainException
{
    public EmpresaInativaException(int empresaId)
        : base($"A empresa com ID {empresaId} está inativa.")
    {
    }

    public EmpresaInativaException(string message)
        : base(message)
    {
    }
}
