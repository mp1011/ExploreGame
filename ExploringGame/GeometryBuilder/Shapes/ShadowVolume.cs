using ExploringGame.Logics;
using ExploringGame.Rendering;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ExploringGame.GeometryBuilder.Shapes;

public class ShadowVolume : Shape
{
    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public override Theme Theme { get; }

    public override ShapeBufferType ShapeBufferType => ShapeBufferType.StaticShadow;

    public ShadowVolume()
    {
        Theme = new Theme(Color.Black);
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid();
    }

    public override RasterizerState RasterizerState => new RasterizerState
    {
        DepthBias = -0.0001f,
        CullMode = CullMode.None
    };
}
