using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes;

public class SimpleRoom : Shape
{
    public Theme Theme { get; }
    public override IColliderMaker ColliderMaker => ColliderMakers.BoundingBox(this);
    public override ViewFrom ViewFrom => ViewFrom.Inside;

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid();
    }

    public SimpleRoom(Theme theme)
    {
        Theme = theme;
    }
}

public class Box : Shape
{
    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public Box() { }

    public Box(TextureKey textureKey)
    {
        MainTexture = new TextureInfo(Key: textureKey);
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid();
    }
}
