using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder;
using ExploringGame.Logics.Collision;

namespace ExploringGame.Logics;

public interface IPhysicsShape : ICollidable
{
}

public static class IPhysicsShapeExtensions
{
    public static void InitializePhysicsObject(this IPhysicsShape shape)
    {
        shape.ColliderBodies[0].Position = shape.WorldPosition.ToJVector();
    }

    public static void SyncShapePosition(this IPhysicsShape shape)
    {
        shape.WorldPosition = shape.ColliderBodies[0].Position.ToVector3();
        shape.Rotation = new Rotation(shape.ColliderBodies[0].Orientation.ToQuaternion());
    }
}
