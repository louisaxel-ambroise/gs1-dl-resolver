using Goto.Infrastructure.Validation;
using System.ComponentModel.DataAnnotations;

namespace Goto.Controllers.Requests;

public sealed class AddAnchorRequest
{
    [MaxLength(255)]
    public required string Prefix { get; init; }
    [CompanyPrefix]
    public required string CompanyPrefix { get; set; }
    [MaxLength(1024)]
    public required string Description { get; init; }
}
