namespace Gs1DigitalLink.Core.Contracts.Registration;

public interface IUserContext
{
    public string Name { get; }
    public string CompanyPrefix { get; }
}