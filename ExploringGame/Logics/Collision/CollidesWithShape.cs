using ExploringGame.Extensions;
using Jitter2.Dynamics;
using System;

namespace ExploringGame.Logics.Collision;

public class CollidesWithShape : ICollisionResponse
{
    private ICollidable _shape;

    public event EventHandler CollisionOccured;

    public CollidesWithShape(ICollidable shape)
    {
        _shape = shape;
    }

    public void OnCollision(RigidBody thisBody, RigidBody otherBody)
    {
        if(thisBody.BelongsTo(_shape) || otherBody.BelongsTo(_shape))
        {
            CollisionOccured.Invoke(this, new EventArgs());
        }
    }
}
