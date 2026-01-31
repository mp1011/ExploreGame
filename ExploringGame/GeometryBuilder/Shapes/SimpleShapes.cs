using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.VisualBasic;

namespace ExploringGame.GeometryBuilder.Shapes;

public class SimpleRoom : Shape
{
    public override Theme Theme { get; }
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

    public override IColliderMaker ColliderMaker => ColliderMakers.BoundingBox(this);

    public Side OmitSides { get; set; }

    private Theme _theme;
    public override Theme Theme => _theme;

    public Box() { }

    public Box(TextureKey textureKey)
    {
        MainTexture = new TextureInfo(Key: textureKey);
    }

    public Box(Theme theme)
    {
        _theme = theme;
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        var shape = BuildCuboid();
        return new SideRemover().Execute(shape, OmitSides);
    }
}
