namespace Gs1DigitalLink.Core.Model.Interfaces;

public abstract class Entity<T> where T : Aggregate
{
    public int Id { get; internal set; }
}