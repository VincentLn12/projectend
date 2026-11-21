namespace ComSci.Application.Helpers;

public static class PhoneNumberNormalizer
{
    private const int MinimumDigits = 9;
    private const int MaximumDigits = 10;

    public static string? Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        string digits = new string(input.Where(char.IsAsciiDigit).ToArray());

        if (digits.StartsWith("66") && digits.Length == MaximumDigits + 1)
        {
            digits = "0" + digits.Substring(2);
        }

        bool hasValidLength = digits.Length >= MinimumDigits && digits.Length <= MaximumDigits;
        if (!hasValidLength || !digits.StartsWith('0'))
        {
            return null;
        }

        return digits;
    }
}
