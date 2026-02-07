using ExploringGame.GeometryBuilder.Shapes.Rooms;
using ExploringGame.Services;
using Jitter2.Dynamics;
using System.Collections.Generic;

namespace ExploringGame.Logics.Collision.ColliderMakers;

public class StepColliderMaker : IColliderMaker
{
    private StairStep _step;

    public StepColliderMaker(StairStep step)
    {
        _step = step;
    }

    public IEnumerable<RigidBody> CreateColliders(Physics physics)
    {
        yield return physics.CreateStaticBody(_step);
    }
}
