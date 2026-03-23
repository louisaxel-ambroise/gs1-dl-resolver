using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Gs1DigitalLink.Web.Contracts;

public sealed record ListInsightRequest
{
    [FromQuery(Name = "days")]
    [Range(1, 365)]
    public required int Days { get; init; } = 1;
}
