using ExploringGame.Logics.Collision;

namespace ExploringGame.GeometryBuilder.Shapes;

public class SimpleRoom : Shape
{
    public override IColliderMaker ColliderMaker => ColliderMakers.BoundingBox(this);
    public override ViewFrom ViewFrom => ViewFrom.Inside;

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid();
    }
}

public class Box : Shape
{
    public override ViewFrom ViewFrom => ViewFrom.Outside;


    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid();
    }
}
