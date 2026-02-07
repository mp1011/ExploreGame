
using ExploringGame.Logics.Collision;
using ExploringGame.Services;
using Jitter2.Collision;
using Jitter2.Collision.Shapes;
using Jitter2.Dynamics;
using System;

namespace ExploringGame.Extensions;

public static class JitterExtensions
{
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
