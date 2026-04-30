using ExploringGame.Logics.Collision;
using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Services;
using Jitter2.LinearMath;
using System;

namespace ExploringGame.GeometryBuilder.Shapes.SimpleShapes;

/// <summary>
/// invisible shape which blocks the player
/// </summary>
public class Blocker : Shape, ICollidable
{
    public override ViewFrom ViewFrom => ViewFrom.None;

    public CollisionGroup CollisionGroup => CollisionGroup.Environment;
    public CollisionGroup CollidesWithGroups => CollisionGroup.MovingObjects;

    public override IColliderMaker ColliderMaker => new BoundingBoxColliderMaker(this);

    public Blocker(string tag)
    {
        Tag = tag;
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid();
    }

    public void Remove()
    {
        ColliderBodies[0].Position = new JVector(0, -100000, 0);
    }
}
