using System.ComponentModel.DataAnnotations;

namespace Goto.Controllers.Requests;

public sealed class AddAnchorRequest
{
    [MaxLength(255)]
    public required string Prefix { get; init; }
    [MaxLength(1024)]
    public required string Description { get; init; }
}
