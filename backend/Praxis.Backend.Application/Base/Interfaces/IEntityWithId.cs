namespace Praxis.Backend.Application.Base.Interfaces;

public interface IEntityWithId<out TKey>
{
    TKey Id { get; }
}
