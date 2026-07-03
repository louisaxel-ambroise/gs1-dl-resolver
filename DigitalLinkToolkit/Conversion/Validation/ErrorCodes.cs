namespace DigitalLinkToolkit.Conversion.Validation;

public static class ErrorCodes
{
    #region DigitalLink 

    public static string InvalidInput => nameof(InvalidInput);
    public static string InvalidAIValue => nameof(InvalidAIValue);
    public static string NoCompanyPrefix => nameof(NoCompanyPrefix);
    public static string InvalidCompanyPrefix => nameof(InvalidCompanyPrefix);
    public static string InvalidCheckDigit => nameof(InvalidCheckDigit);
    public static string DuplicateAI => nameof(DuplicateAI);
    public static string UnknownAI => nameof(UnknownAI);

    #endregion

    #region Prefix

    public static string InvalidPrefix => nameof(InvalidPrefix);

    #endregion

    #region Compression

    public static string UnsupportedGS1Algorithm => nameof(UnsupportedGS1Algorithm);
    public static string UnsupportedProprietaryAlgorithm => nameof(UnsupportedProprietaryAlgorithm);
    public static string InvalidCompressedValue => nameof(InvalidCompressedValue);

    #endregion
}