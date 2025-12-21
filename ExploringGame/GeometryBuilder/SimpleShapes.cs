namespace ExploringGame.GeometryBuilder;

public class SimpleRoom : Shape
{
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
