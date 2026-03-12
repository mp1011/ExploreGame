using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.Logics.Collision;
using ExploringGame.Services;
using ExploringGame.Texture;
using Jitter2.Dynamics;
using Microsoft.Xna.Framework;

namespace ExploringGame.Entities;

/// <summary>
/// Spherical child shape of the Light Spirit. Used solely for visual appearance (not for collision or physics).
/// </summary>
public class LightSpiritSphere : Shape
{
    private readonly LightSpirit _parent;
    private const float Radius = 0.5f;

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public LightSpiritSphere(LightSpirit parent)
    {
        _parent = parent;
        Width = Radius * 2;
        Height = Radius * 2;
        Depth = Radius * 2;
        // Glowing white/light appearance
        MainTexture = new TextureInfo(Color.White, TextureKey.Wall);
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return TriangleMaker.BuildEllipsoid(this, 16);
    }
}
