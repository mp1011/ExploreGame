using ExploringGame.Logics.Collision;
using ExploringGame.Services;
using Jitter2.Collision;
using Jitter2.Collision.Shapes;
using Jitter2.Dynamics;
using Jitter2.LinearMath;
using Microsoft.Xna.Framework;

namespace ExploringGame.Extensions;

public static class JitterExtensions
{
    public static JQuaternion ToJQuaternion(this Quaternion q)
    {
        return new JQuaternion(q.X, q.Y, q.Z, q.W);
    }

    public static Quaternion ToQuaternion(this JQuaternion q)
    {
        return new Quaternion(q.X, q.Y, q.Z, q.W);
    }

    public static string DiagnosticInfo(this RigidBody body)
    {
        if (body.Tag is CollisionInfo collisionInfo && collisionInfo.Shape != null)
           return collisionInfo.Shape.GetType().Name;
        else
            return null;
    }

    public static bool BelongsTo(this IDynamicTreeProxy proxy, ICollidable collidable)
    {
        if (proxy is RigidBodyShape bodyShape)
        {
            for(int i = 0; i < collidable.ColliderBodies.Length; i++)
            {
                if (bodyShape.RigidBody == collidable.ColliderBodies[i])
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static bool BelongsTo(this RigidBody bodyShape, ICollidable collidable)
    {        
        for (int i = 0; i < collidable.ColliderBodies.Length; i++)
        {
            if (bodyShape == collidable.ColliderBodies[i])
            {
                return true;
            }
        }
        
        return false;
    }

    public static CollisionInfo CollisionInfo(this IDynamicTreeProxy proxy)
    {
        if (proxy is RigidBodyShape bodyShape && bodyShape.RigidBody.Tag is CollisionInfo info)
        {
            return info;
        }
        else
            return null;
    }

    public static CollisionInfo CollisionInfo(this RigidBody body)
    {
        return body.Tag as CollisionInfo ?? new CollisionInfo(CollisionGroup.None, CollisionGroup.None);
    }
}
