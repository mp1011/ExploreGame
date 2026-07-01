using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder;
using ExploringGame.Logics.Collision;

namespace ExploringGame.Logics;

public interface IPhysicsShape : ICollidable, IShape
{
    bool Active { get; set; }
}

public static class IPhysicsShapeExtensions
{
    public static void InitializePhysicsObject(this IPhysicsShape shape)
    {
        shape.ColliderBodies[0].Position = shape.WorldPosition.ToJVector();
        if(shape.Rotation != null)
            shape.ColliderBodies[0].Orientation = shape.Rotation.Quaternion.ToJQuaternion();
    }

    public static void SyncShapePosition(this IPhysicsShape shape)
    {
        if (!shape.Active)
        {
            shape.InitializePhysicsObject();
        }
        else
        {
            shape.WorldPosition = shape.ColliderBodies[0].Position.ToVector3();
            shape.Rotation = new Rotation(shape.ColliderBodies[0].Orientation.ToQuaternion());
        }
    }
}
