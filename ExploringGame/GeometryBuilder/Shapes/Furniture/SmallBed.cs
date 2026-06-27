using ExploringGame.GeometryBuilder.Shapes.SimpleShapes;
using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Services;
using ExploringGame.Texture;
using ExploringGame.Texture.Themes;

namespace ExploringGame.GeometryBuilder.Shapes.Furniture;

public class SmallBed : PlaceableShape
{
    public override CollisionGroup CollisionGroup => CollisionGroup.Environment;

    public override CollisionGroup CollidesWithGroups => CollisionGroup.MovingObjects;

    public override IColliderMaker ColliderMaker =>  ColliderMakers.BoundingBox(this);

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public override Theme Theme => new BedTheme();

    public SmallBed()
    {
        Width = Measure.Inches(40);
        Depth = Measure.Inches(80);
        Height = Measure.Inches(24);

        var pillow = AddChild(new Box(new PillowTheme()));
        pillow.Width = Measure.Inches(30);
        pillow.Depth = Measure.Inches(20);
        pillow.Height = Measure.Inches(1);

        pillow.Place().At(this)
            .OnSideInner(Side.South, offset: 0.1f)
            .OnSideInner(Side.West, offset: 0.1f)
            .OnSideOuter(Side.Top);
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid();
    }
}
