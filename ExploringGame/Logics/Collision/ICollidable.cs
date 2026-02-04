using ExploringGame.Entities;
using ExploringGame.Services;
using Jitter2.Dynamics;

namespace ExploringGame.Logics.Collision;

public interface ICollidable : IWithPosition
{
    public CollisionGroup CollisionGroup { get; }
    public CollisionGroup CollidesWithGroups { get; }

    public RigidBody[] ColliderBodies { get; }
}

public static class CollidableExtensions
{
    public static float Width(this ICollidable collidable) => collidable.Size.X;
    public static float Height(this ICollidable collidable) => collidable.Size.Y;
    public static float Depth(this ICollidable collidable) => collidable.Size.Z;
}
