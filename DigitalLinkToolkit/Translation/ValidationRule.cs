namespace DigitalLinkToolkit.Translation;

internal static class ValidationRule
{
    internal static bool MinValue(string minValue, string input)
    {
        if (input.TrimStart('0').Length > minValue.TrimStart('0').Length) return true;
        if (input.TrimStart('0').Length < minValue.TrimStart('0').Length) return false;

        for (var i = 0; i < minValue.Length; i++)
            {
            if (input[i] < minValue[i]) return false;
        }

        return true;
    }
    public static bool MaxValue(string maxValue, string input)
    {
        if (input.TrimStart('0').Length < maxValue.TrimStart('0').Length) return true;
        if (input.TrimStart('0').Length > maxValue.TrimStart('0').Length) return false;

        for (var i = 0; i < maxValue.Length; i++)
        {
            if (input[i] > maxValue[i]) return false;
        }

        return true;
    }
}