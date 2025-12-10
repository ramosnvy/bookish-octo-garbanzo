namespace Elo.Domain.Exceptions;

public class ProdutoInativoException : DomainException
{
    public ProdutoInativoException(int produtoId)
        : base($"O produto com ID {produtoId} está inativo.")
    {
    }

    public ProdutoInativoException(string message)
        : base(message)
    {
    }
}
