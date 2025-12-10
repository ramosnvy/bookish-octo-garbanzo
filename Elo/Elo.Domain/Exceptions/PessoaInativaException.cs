using Elo.Domain.Enums;

namespace Elo.Domain.Exceptions;

public class PessoaInativaException : DomainException
{
    public PessoaInativaException(int pessoaId, PessoaTipo tipo)
        : base($"O {GetTipoLabel(tipo)} com ID {pessoaId} está inativo.")
    {
    }

    public PessoaInativaException(string message)
        : base(message)
    {
    }

    private static string GetTipoLabel(PessoaTipo tipo)
    {
        return tipo switch
        {
            PessoaTipo.Cliente => "cliente",
            PessoaTipo.Fornecedor => "fornecedor",
            _ => "pessoa"
        };
    }
}
