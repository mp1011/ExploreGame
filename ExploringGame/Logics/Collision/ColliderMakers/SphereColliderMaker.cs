using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.Logics.Collision;
using ExploringGame.Services;
using Jitter2.Dynamics;
using System.Collections.Generic;

namespace ExploringGame.Logics.Collision.ColliderMakers;

public class SphereColliderMaker : IColliderMaker
{
    public ICollidable Entity { get; }

    public SphereColliderMaker(ICollidable entity)
    {
        Entity = entity;
    }

    public IEnumerable<RigidBody> CreateColliders(Physics physics)
    {
        // since spherical, just use X for radius
        yield return physics.CreateSphere(Entity, Entity.Size.X / 2f, Entity.CollisionGroup, Entity.CollidesWithGroups);
    }
}
