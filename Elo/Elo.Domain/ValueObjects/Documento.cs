namespace Elo.Domain.ValueObjects;

public enum TipoDocumento
{
    CPF,
    CNPJ
}

public class Documento
{
    public string Numero { get; }
    public TipoDocumento Tipo { get; }

    public Documento(string numero)
    {
        if (string.IsNullOrWhiteSpace(numero))
            throw new ArgumentException("Numeration cannot be empty.");

        var digits = new string(numero.Where(char.IsDigit).ToArray());

        if (digits.Length == 11)
        {
            if (!IsCpf(digits)) throw new ArgumentException("Invalid CPF.");
            Tipo = TipoDocumento.CPF;
        }
        else if (digits.Length == 14)
        {
            if (!IsCnpj(digits)) throw new ArgumentException("Invalid CNPJ.");
            Tipo = TipoDocumento.CNPJ;
        }
        else
        {
            throw new ArgumentException("Document must be CPF (11 digits) or CNPJ (14 digits).");
        }

        Numero = digits;
    }

    public string Formatado()
    {
        if (Tipo == TipoDocumento.CPF)
            return Convert.ToUInt64(Numero).ToString(@"000\.000\.000\-00");
        return Convert.ToUInt64(Numero).ToString(@"00\.000\.000\/0000\-00");
    }

    public override string ToString() => Numero;

    public static implicit operator string(Documento documento) => documento.Numero;
    public static implicit operator Documento(string numero) => new Documento(numero);

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj is Documento other) return Numero == other.Numero;
        return false;
    }

    public override int GetHashCode() => Numero.GetHashCode();

    private static bool IsCpf(string cpf)
    {
        int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        string tempCpf;
        string digito;
        int soma;
        int resto;

        cpf = cpf.Trim();
        cpf = cpf.Replace(".", "").Replace("-", "");

        if (cpf.Length != 11)
            return false;

        tempCpf = cpf.Substring(0, 9);
        soma = 0;

        for (int i = 0; i < 9; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

        resto = soma % 11;
        if (resto < 2)
            resto = 0;
        else
            resto = 11 - resto;

        digito = resto.ToString();
        tempCpf = tempCpf + digito;
        soma = 0;

        for (int i = 0; i < 10; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

        resto = soma % 11;
        if (resto < 2)
            resto = 0;
        else
            resto = 11 - resto;

        digito = digito + resto.ToString();
        return cpf.EndsWith(digito);
    }

    private static bool IsCnpj(string cnpj)
    {
        int[] multiplicador1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multiplicador2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int soma;
        int resto;
        string digito;
        string tempCnpj;

        cnpj = cnpj.Trim();
        cnpj = cnpj.Replace(".", "").Replace("-", "").Replace("/", "");

        if (cnpj.Length != 14)
            return false;

        tempCnpj = cnpj.Substring(0, 12);
        soma = 0;

        for (int i = 0; i < 12; i++)
            soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];

        resto = (soma % 11);
        if (resto < 2)
            resto = 0;
        else
            resto = 11 - resto;

        digito = resto.ToString();
        tempCnpj = tempCnpj + digito;
        soma = 0;
        for (int i = 0; i < 13; i++)
            soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];

        resto = (soma % 11);
        if (resto < 2)
            resto = 0;
        else
            resto = 11 - resto;

        digito = digito + resto.ToString();
        return cnpj.EndsWith(digito);
    }
}
