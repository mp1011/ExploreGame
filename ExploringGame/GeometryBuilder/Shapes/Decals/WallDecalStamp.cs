using ExploringGame.GeometryBuilder;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ExploringGame.GeometryBuilder.Shapes.Decals;

public class WallDecalStamp : ShapeStamp
{
    public WallDecalStamp()
    {
        Width = 1.0f;
        Height = 1.0f;
        Depth = 0.01f; // Very thin
        MainTexture = new TextureInfo(Color.White, TextureKey.Wall);
    }

    public override RasterizerState RasterizerState => new RasterizerState
    {
        DepthBias = -0.0001f,
        CullMode = CullMode.None
    };

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        // Build a simple quad (two triangles)
        var halfWidth = Width / 2;
        var halfHeight = Height / 2;

        // Front face (facing +Z)
        var v1 = new Vector3(-halfWidth, -halfHeight, 0);
        var v2 = new Vector3(halfWidth, -halfHeight, 0);
        var v3 = new Vector3(halfWidth, halfHeight, 0);
        var v4 = new Vector3(-halfWidth, halfHeight, 0);

        return new[]
        {
            new Triangle(v1, v2, v3, MainTexture, Side.North),
            new Triangle(v1, v3, v4, MainTexture, Side.North)
        };
    }
}
