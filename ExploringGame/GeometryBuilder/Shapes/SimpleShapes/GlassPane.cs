using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.SimpleShapes;

/// <summary>
/// A transparent glass pane shape intended for windows.
/// Rendered with a blur effect to simulate looking through glass.
/// </summary>
public class GlassPane : Shape
{
    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public override IColliderMaker ColliderMaker => ColliderMakers.BoundingBox(this);

    public Side OmitSides { get; set; }

    private Theme _theme = new Theme();
    public override Theme Theme => _theme;

    public GlassPane()
    {
        // Glass should be slightly transparent/white tinted
        MainTexture = new TextureInfo(new Color(255, 255, 255, 240), TextureKey.Wall);
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        var shape = BuildCuboid();
        return new SideRemover().Execute(shape, OmitSides);
    }
}
