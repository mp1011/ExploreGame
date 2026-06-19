using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder.Shapes.Structures;
using ExploringGame.Logics.Collision;
using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Services;
using Jitter2.LinearMath;
using System;
using System.Numerics;

namespace ExploringGame.GeometryBuilder.Shapes.SimpleShapes;

/// <summary>
/// invisible shape which blocks the player
/// </summary>
public class Blocker : PlaceableShape, ICollidable
{
    public Shape BlockingShape { get; }
    public override ViewFrom ViewFrom => ViewFrom.None;

    public override CollisionGroup CollisionGroup => CollisionGroup.Environment;
    public override CollisionGroup CollidesWithGroups => CollisionGroup.MovingObjects;

    public override IColliderMaker ColliderMaker => new BoundingBoxColliderMaker(this);

    private bool _enabled;
    public bool Enabled
    {
        get => _enabled;
        set
        {
            _enabled = value;
            RefreshCollider();
        }
    }

    public Blocker(Shape blockingShape)
    {
        BlockingShape = blockingShape;
        RefreshCollider();
    }

    public void RefreshCollider()
    {
        this.AdjustShape().From(BlockingShape)
                .AxisStretch(Axis.X, 0.1f)
                .AxisStretch(Axis.Z, 0.1f);

        if (!_enabled)
            LocalY += 10000;

        Rotation = BlockingShape.Rotation;

        if(ColliderBodies != null)
            ColliderBodies[0].Position = WorldPosition.ToJVector();
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid();
    }

    public override string ToString()
    {
        return $"Blocker for {BlockingShape}";
    }
}
