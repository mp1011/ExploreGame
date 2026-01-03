using Jitter2.Collision.Shapes;
using Jitter2.Dynamics;
using Microsoft.Xna.Framework;

namespace ExploringGame.Logics.Collision;

class DetectFloorCollision : ICollisionResponse
{
    private readonly EntityMover _entityMover;

    public DetectFloorCollision(EntityMover entityMover)
    {
        _entityMover = entityMover;
    }

    public void OnCollision(RigidBody thisBody, RigidBody otherBody)
    {
        if (thisBody.Velocity.Y > 0)
            return;

        var box = otherBody.Shapes[0].WorldBoundingBox;
        var maybeFloorY = box.Max.Y;

        // maybe not entirely accurate
        if (thisBody.Position.Y > maybeFloorY)
            _entityMover.Motion.CurrentY = 0;
    }
}
