namespace Gs1DigitalLink.Api.Contracts;

public sealed record MultipleChoiceResponse(IEnumerable<LinkDefinition> Links);