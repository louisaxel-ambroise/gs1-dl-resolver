namespace Gs1DigitalLink.Core.Services.Conversion.Utils.Validation;

internal static class ErrorCodes
{
    public static string InvalidInput => nameof(InvalidInput);
    public static string InvalidAIValue => nameof(InvalidAIValue);
    public static string InvalidCompanyPrefix => nameof(InvalidCompanyPrefix);
    public static string InvalidCheckDigit => nameof(InvalidCheckDigit);
    public static string DuplicateAI => nameof(DuplicateAI);
    public static string UnknownAI => nameof(UnknownAI);
    public static string UnsupportedGS1Algorithm => nameof(UnsupportedGS1Algorithm);
    public static string UnsupportedProprietaryAlgorithm => nameof(UnsupportedProprietaryAlgorithm);
    public static string InvalidCompressedValue => nameof(InvalidCompressedValue);
}