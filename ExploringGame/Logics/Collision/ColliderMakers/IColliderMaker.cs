using ExploringGame.Entities;
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
    public static IColliderMaker BoundingBox(ICollidable shape, bool isStatic = true) => new BoundingBoxColliderMaker(shape, isStatic);
    public static IColliderMaker BoundingBox(IWithPosition shape) => new LegacyBoundingBoxColliderMaker(shape);

    public static IColliderMaker Room(Shape room) => new RoomColliderMaker(room);

    public static IColliderMaker Step(StairStep step) => new StepColliderMaker(step);

}

public class BoundingBoxColliderMaker : IColliderMaker
{
    private ICollidable _shape;
    private bool _isStatic;

    public BoundingBoxColliderMaker(ICollidable shape, bool isStatic=true)
    {
        _shape = shape;
        _isStatic = isStatic;
    }

    public IEnumerable<RigidBody> CreateColliders(Physics physics)
    {
        if (_isStatic)
            yield return physics.CreateStaticBody(_shape);
        else
            yield return physics.CreateDynamicBody(_shape);
    }
}

public class LegacyBoundingBoxColliderMaker : IColliderMaker
{
    private IWithPosition _shape;

    public LegacyBoundingBoxColliderMaker(IWithPosition shape)
    {
        _shape = shape;
    }

    public IEnumerable<RigidBody> CreateColliders(Physics physics)
    {
        yield return physics.CreateStaticBody(_shape, CollisionGroup.Environment, CollisionGroup.MovingObjects);
    }
}

