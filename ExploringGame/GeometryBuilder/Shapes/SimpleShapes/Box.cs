using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.VisualBasic;

namespace ExploringGame.GeometryBuilder.Shapes.SimpleShapes;

public class Box : Shape
{
    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public override IColliderMaker ColliderMaker => ColliderMakers.BoundingBox(this);

    public Side OmitSides { get; set; }

    private Theme _theme = new Theme();
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
