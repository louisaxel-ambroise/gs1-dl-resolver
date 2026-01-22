using Gs1DigitalLink.Core;
using Gs1DigitalLink.Core.Model;

internal class HttpContextEventDispatcher(IHttpContextAccessor contextAccessor) : IEventDispatcher
{
    public void Dispatch(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(contextAccessor.HttpContext);

        var type = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
        var method = type.GetMethod("Handle");
        var handlers = contextAccessor.HttpContext.RequestServices.GetServices(type);

        handlers.ToList().ForEach(handler => method?.Invoke(handler, [domainEvent]));
    }
}