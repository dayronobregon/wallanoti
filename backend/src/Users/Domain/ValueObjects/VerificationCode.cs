namespace Wallanoti.Src.Users.Domain.ValueObjects;

public sealed class VerificationCode
{
    /// <summary>
    /// Codigo de verificación de 6 dígitos.
    /// TODO deberia de ser valido por un tiempo limitado.
    /// </summary>
    public string Value { get; }

    public VerificationCode(string value)
    {
        Value = value;
    }

    private bool IsValid(string code)
    {
        //El código debe ser de 6 dígitos
        if (string.IsNullOrWhiteSpace(code) || code.Length != 6 || !code.All(char.IsDigit))
        {
            return false;
        }

        return Value == code;
    }

    public bool Verify(string code)
    {
        if (!IsValid(code))
        {
            return false;
        }

        return Value == code;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static VerificationCode Random()
    {
        var random = new Random();
        var code = random.Next(100000, 999999).ToString();

        return new VerificationCode(code);
    }
}