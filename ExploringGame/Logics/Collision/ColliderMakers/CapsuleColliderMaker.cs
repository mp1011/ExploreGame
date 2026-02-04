using ExploringGame.Services;
using Jitter2.Dynamics;
using System.Collections.Generic;

namespace ExploringGame.Logics.Collision.ColliderMakers;

public class CapsuleColliderMaker : IColliderMaker
{
    public ICollidable Entity { get; }

    public CapsuleColliderMaker(ICollidable entity)
    {
        Entity = entity;
    }

    public IEnumerable<RigidBody> CreateColliders(Physics physics)
    {
        yield return physics.CreateCapsule(Entity, Entity.CollisionGroup, Entity.CollidesWithGroups);
    }
}
