using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.SimpleShapes;

/// <summary>
/// General-purpose cylinder shape for use in geometry (e.g., curtain rods).
/// </summary>
public class Cylinder : Shape
{
    public override ViewFrom ViewFrom => ViewFrom.Outside;
    public override IColliderMaker ColliderMaker => ColliderMakers.BoundingBox(this);
    private Theme _theme = new Theme();
    public override Theme Theme => _theme;

    public Cylinder() { }
    public Cylinder(TextureKey textureKey)
    {
        MainTexture = new TextureInfo(Key: textureKey);
    }
    public Cylinder(Theme theme)
    {
        _theme = theme;
    }

    /// <summary>
    /// detail: number of segments around the circumference (default: 32)
    /// axis: which axis the cylinder is aligned to (default: X)
    /// </summary>
    public int Detail { get; set; } = 32;
    public Axis Axis { get; set; } = Axis.X;

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return TriangleMaker.BuildCylinder(this, Detail, Axis);
    }
}
