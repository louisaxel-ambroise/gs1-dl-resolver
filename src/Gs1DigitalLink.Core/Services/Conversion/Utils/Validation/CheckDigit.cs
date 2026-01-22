namespace Gs1DigitalLink.Core.Services.Conversion.Utils.Validation;

internal static class CheckDigit
{
    public static bool Validate(string input)
    {
        var weightedSum = 0;

        for (var i = 0; i < input.Length - 1; i++)
        {
            var weight = i % 2 == 0 ? 3 : 1;
            weightedSum += (input[i] - '0') * weight;
        }

        var checkDigit = 10 - weightedSum % 10;

        return checkDigit % 10 == input[^1] - '0';
    }
}
