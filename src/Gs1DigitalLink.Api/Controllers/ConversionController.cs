using Gs1DigitalLink.Api.Contracts;
using Gs1DigitalLink.Core.Services.Conversion;
using Microsoft.AspNetCore.Mvc;

namespace Gs1DigitalLink.Api.Controllers;

[ApiController]
[Route("api")]
[Produces("application/json")]
public sealed class ConversionController(IDigitalLinkConverter converter) : ControllerBase
{
    [HttpPost("compress")]
    public IActionResult Compress([FromBody] CompressionRequest request)
    {
        var options = new DigitalLinkCompressionOptions { CompressionType = request.CompressionType, CompressQueryString = request.CompressQueryString };
        var compressionResult = converter.Compress(request.DigitalLink, options);
        var response = new
        {
            compressionResult.CompressedValue,
            compressionResult.UncompressedValue,
            compressionResult.CompressionRate,
            CanonicalUrl = $"{Request.Scheme}://{Request.Host}/{compressionResult.CompressedValue}"
        };

        return new OkObjectResult(response);
    }

    [HttpPost("decompress")]
    public IActionResult Decompress([FromBody] DecompressionRequest request)
    {
        var result = converter.Parse(request.DigitalLink);
        var response = new
        {
            Type = result.Type.ToString(),
            AIs = result.AIs.Select(ai =>
               new
               {
                   Type = ai.Key.Type.ToString(),
                   ai.Code,
                   ai.Value
               }),
            QueryString = result.QueryString.Select(query =>
               new
               {
                   query.Key,
                   query.Value
               }),
            DecompressedValue = result.ToString(),
            CanonicalUrl = $"{Request.Scheme}://{Request.Host}/{result}"
        };

        return new OkObjectResult(response);
    }
}
