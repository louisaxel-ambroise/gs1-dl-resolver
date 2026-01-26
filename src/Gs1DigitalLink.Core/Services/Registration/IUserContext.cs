namespace Gs1DigitalLink.Core.Services.Registration;

public interface IUserContext
{
    public string Name { get; }
    public string CompanyPrefix { get; }
}