namespace Goto.Services.Data.Entities;

public sealed class ApiKey
{
    public string Id { get; init; }
    public required string Name { get; init; }
    public required string CompanyPrefix { get; init; }
    public required DateTimeOffset BeginValidityDate { get; init; }
    public required DateTimeOffset EndValidityDate { get; set; }

    public void Disable(DateTimeOffset endValidityDate)
    {
        EndValidityDate = endValidityDate;
    }
}