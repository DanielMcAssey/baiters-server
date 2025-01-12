namespace GLOKON.Baiters.Core.Models.Actor
{
    public sealed class GenericActor(string type, ulong ownerId = 0) : Actor(type, ownerId)
    {
    }
}
