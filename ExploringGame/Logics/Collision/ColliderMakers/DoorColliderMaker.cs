using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.Services;
using Jitter2.Dynamics;
using System.Collections;
using System.Collections.Generic;

namespace ExploringGame.Logics.Collision.ColliderMakers;

public class DoorColliderMaker : IColliderMaker
{
    private readonly Door _door;

    public DoorColliderMaker(Door door)
    {
        _door = door;
    }

    public IEnumerable<RigidBody> CreateColliders(Physics physics)
    {
        yield return physics.CreateHingedDoor(_door);      
    }
}
