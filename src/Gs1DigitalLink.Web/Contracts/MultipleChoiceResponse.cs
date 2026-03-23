namespace Gs1DigitalLink.Web.Contracts;

public sealed record MultipleChoiceResponse(IEnumerable<LinkDefinition> Links);