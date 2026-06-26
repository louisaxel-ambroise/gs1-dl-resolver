namespace Goto.Controllers.Results;

public sealed class MetadataResult
{
    public required string ResolverRoot { get; init; }
    public required string Name { get; init; }
    public required string JsonLdContextLocation { get; set; }
    public required string[] SupportedPrimaryKeys { get; init; }
    public required bool LinkTypeDefaultCanBeLinkset { get; set; }
    public required MetadataResultContact Contact { get; set; }
}

public sealed class MetadataResultContact
{
    public required string Fn { get; set; }
}
