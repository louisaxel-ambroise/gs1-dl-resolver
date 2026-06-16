namespace Gs1DigitalLink.Web.Contracts;

public class GS1ResolverResult
{
    public required string ResolverRoot { get; set; }
    public required string Name { get; set; }
    public required string[] SupportedPrimaryKeys { get; set; }
    public required bool LinkTypeDefaultCanBeLinkset { get; set; }
    public required Contact Contact { get; set; }
}

public class Contact
{
    public required string Fn { get; set; }
}