using Elo.Domain.Enums;

namespace Elo.Domain.Exceptions;

public class FormaPagamentoInativaException : Exception
{
    public FormaPagamentoInativaException(FormaPagamento formaPagamento)
        : base($"A forma de pagamento '{formaPagamento}' não está disponível para esta empresa.")
    {
    }
}
