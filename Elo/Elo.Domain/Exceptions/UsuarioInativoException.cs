namespace Elo.Domain.Exceptions;

public class UsuarioInativoException : DomainException
{
    public UsuarioInativoException(int usuarioId)
        : base($"O usuário com ID {usuarioId} está inativo e não pode realizar esta operação.")
    {
    }

    public UsuarioInativoException(string message)
        : base(message)
    {
    }
}
