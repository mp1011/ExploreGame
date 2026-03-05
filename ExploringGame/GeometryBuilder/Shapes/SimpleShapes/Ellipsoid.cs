using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.SimpleShapes;

/// <summary>
/// Represents an ellipsoid (sphere or stretched sphere) with configurable axes.
/// </summary>
public class Ellipsoid : Shape
{
    public override ViewFrom ViewFrom => ViewFrom.Outside;
    public override IColliderMaker ColliderMaker => ColliderMakers.BoundingBox(this);

    private Theme _theme = new Theme();
    public override Theme Theme => _theme;

    public Ellipsoid() { }

    public Ellipsoid(float radius) 
    {
        Width = radius * 2;
        Height = radius * 2;
        Depth = radius * 2;
    }

    public Ellipsoid(TextureKey textureKey)
    {
        MainTexture = new TextureInfo(Key: textureKey);
    }
    public Ellipsoid(Theme theme)
    {
        _theme = theme;
    }

    /// <summary>
    /// Number of segments for mesh detail (default: 32)
    /// </summary>
    public int Detail { get; set; } = 32;

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        // Uses the current Width, Height, Depth for axes
        return TriangleMaker.BuildEllipsoid(this, Detail);
    }
}
