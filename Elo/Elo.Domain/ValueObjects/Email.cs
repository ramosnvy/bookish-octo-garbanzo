using System.Text.RegularExpressions;

namespace Elo.Domain.ValueObjects;

public class Email
{
    public string Address { get; }

    public Email(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Email address cannot be empty.");

        if (!IsValid(address))
            throw new ArgumentException("Invalid email format.");

        Address = address;
    }

    public static bool IsValid(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        try
        {
            return Regex.IsMatch(email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    public override string ToString() => Address;

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj is Email other) return Address == other.Address;
        return false;
    }

    public override int GetHashCode() => Address.GetHashCode();

    public static implicit operator string(Email email) => email.Address;
    public static implicit operator Email(string address) => new Email(address);
}
