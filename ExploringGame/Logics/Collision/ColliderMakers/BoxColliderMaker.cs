using ExploringGame.Services;
using Jitter2.Dynamics;
using System.Collections.Generic;

namespace ExploringGame.Logics.Collision.ColliderMakers;

public class BoxColliderMaker : IColliderMaker
{
    public ICollidable Entity { get; }

    public BoxColliderMaker(ICollidable entity)
    {
        Entity = entity;
    }

    public IEnumerable<RigidBody> CreateColliders(Physics physics)
    {
        yield return physics.CreateDynamicBody(Entity);
    }
}
