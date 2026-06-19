using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Rendering;
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
    public override ShapeBufferType ShapeBufferType => ShapeBufferType.Glass;
    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public override IColliderMaker ColliderMaker { get; }

    private Side _wallSide;

    private Theme _theme = new Theme(TextureSheetKey.Upstairs, TextureKey.Plain, new Color(255, 255, 255, 240));
    public override Theme Theme => _theme;

    public GlassPane(Side wallSide, bool hasCollision)
    {
        if (hasCollision)
            ColliderMaker = ColliderMakers.BoundingBox(this);
        else
            ColliderMaker = null;

        _wallSide = wallSide;
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        var shape = BuildCuboid();
        return new SideRemover().Execute(shape, Side.Top | Side.Bottom | _wallSide.ClockwiseTurn() | _wallSide.CounterClockwiseTurn());
    }
}
