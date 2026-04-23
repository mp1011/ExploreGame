using ExploringGame.GeometryBuilder.Shapes.Structures;
using ExploringGame.Services;
using Jitter2.Dynamics;
using System.Collections.Generic;

namespace ExploringGame.Logics.Collision.ColliderMakers;

public class SlidingDoorColliderMaker : IColliderMaker
{
    private readonly SlidingDoorPane _door;

    public SlidingDoorColliderMaker(SlidingDoorPane door)
    {
        _door = door;
    }

    public IEnumerable<RigidBody> CreateColliders(Physics physics)
    {
        yield return physics.CreateSlidingDoor(_door);      
    }
}
