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