using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.Rooms;
using ExploringGame.Services;
using Jitter2.Dynamics;
using System.Collections.Generic;
using GShape = ExploringGame.GeometryBuilder.Shape;

namespace ExploringGame.Logics.Collision.ColliderMakers;

public interface IColliderMaker
{
    IEnumerable<RigidBody> CreateColliders(Physics physics);
}

public static class ColliderMakers
{
    public static IColliderMaker BoundingBox(GShape shape) => new BoundingBoxColliderMaker(shape);

    public static IColliderMaker Room(Room room) => new RoomColliderMaker(room);

    public static IColliderMaker Step(StairStep step) => new StepColliderMaker(step);

}

public class BoundingBoxColliderMaker : IColliderMaker
{
    private GShape _shape;

    public BoundingBoxColliderMaker(GShape shape)
    {
        _shape = shape;
    }

    public IEnumerable<RigidBody> CreateColliders(Physics physics)
    {
        if (_shape.ViewFrom == ViewFrom.Inside)
        {
            yield return physics.CreateStaticSurface(_shape, Side.West);
            yield return physics.CreateStaticSurface(_shape, Side.East);
            yield return physics.CreateStaticSurface(_shape, Side.North);
            yield return physics.CreateStaticSurface(_shape, Side.South);
            yield return physics.CreateStaticSurface(_shape, Side.Bottom);
            yield return physics.CreateStaticSurface(_shape, Side.Top);
        }
        else
        {
            yield return physics.CreateStaticBody(_shape, CollisionGroup.Environment);
        }
    }
}
