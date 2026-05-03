using ExploringGame.GeometryBuilder.Shapes.SimpleShapes;
using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Services;
using ExploringGame.Texture;
using ExploringGame.Texture.Themes;

namespace ExploringGame.GeometryBuilder.Shapes.Furniture;

public class Bed : PlaceableShape
{
    public override CollisionGroup CollisionGroup => CollisionGroup.Environment;

    public override CollisionGroup CollidesWithGroups => CollisionGroup.MovingObjects;

    public override IColliderMaker ColliderMaker =>  ColliderMakers.BoundingBox(this);

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public override Theme Theme => new BedTheme();

    public Bed()
    {
        Width = Measure.Inches(76);
        Depth = Measure.Inches(80);
        Height = Measure.Inches(24);

        var leftPillow = AddChild(new Box(new PillowTheme()));
        leftPillow.Width = Measure.Inches(30);
        leftPillow.Depth = Measure.Inches(20);
        leftPillow.Height = Measure.Inches(1);

        leftPillow.Place().At(this)
            .OnSideInner(Side.North, offset: 0.1f)
            .OnSideInner(Side.West, offset: 0.1f)
            .OnSideOuter(Side.Top);

        var rightPillow = AddChild(new Box(new PillowTheme()));
        rightPillow.Size = leftPillow.Size;

        rightPillow.Place().At(this)
            .OnSideInner(Side.North, offset: 0.1f)
            .OnSideInner(Side.East, offset: -0.1f)
            .OnSideOuter(Side.Top);
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid();
    }
}
