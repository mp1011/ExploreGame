using ExploringGame.GeometryBuilder;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.Testing;

public class TestShapeStamp : ShapeStamp
{
    public TestShapeStamp()
    {
        Width = 0.5f;
        Height = 0.5f;
        Depth = 0.5f;

        // Each side a different color
        MainTexture = new TextureInfo(Color.White, TextureKey.Wall);
        SideTextures[Side.North] = new TextureInfo(Color.Red, TextureKey.Wall);
        SideTextures[Side.South] = new TextureInfo(Color.Blue, TextureKey.Wall);
        SideTextures[Side.East] = new TextureInfo(Color.Green, TextureKey.Wall);
        SideTextures[Side.West] = new TextureInfo(Color.Yellow, TextureKey.Wall);
        SideTextures[Side.Top] = new TextureInfo(Color.Magenta, TextureKey.Wall);
        SideTextures[Side.Bottom] = new TextureInfo(Color.Cyan, TextureKey.Wall);
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid();
    }
}
