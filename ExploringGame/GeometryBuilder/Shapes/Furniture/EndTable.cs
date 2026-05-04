using ExploringGame.GeometryBuilder.Shapes.SimpleShapes;
using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Services;
using ExploringGame.Texture;
using ExploringGame.Texture.Themes;

namespace ExploringGame.GeometryBuilder.Shapes.Furniture;

public class EndTable : PlaceableShape
{
    public override CollisionGroup CollisionGroup => CollisionGroup.Environment;

    public override CollisionGroup CollidesWithGroups => CollisionGroup.MovingObjects;

    public override IColliderMaker ColliderMaker => ColliderMakers.BoundingBox(this);

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public override Theme Theme => new BasicFurnitureTheme();

    public EndTable()
    {
        Width = Measure.Inches(28);
        Depth = Measure.Inches(18);
        Height = Measure.Inches(30);       
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid();
    }
}
