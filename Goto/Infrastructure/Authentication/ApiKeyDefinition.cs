namespace Goto.Infrastructure.Authentication;

public record ApiKeyDefinition
{
    public Key[] Keys { get; set; }
}

public record Key
{
    public string Name { get; set; }
    public string CompanyPrefix { get; set; }
}