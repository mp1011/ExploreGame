using ExploringGame.GeometryBuilder;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.Skyboxes;

public class TestSkybox : SkyboxShape
{
    private static TestSkybox _instance;
    public static TestSkybox Instance => _instance ??= new TestSkybox();

    public override Theme Theme { get; }

    private TestSkybox()
    {
        Theme = new Theme(TextureSheetKey.Upstairs);
        Theme.SideTextures[Side.Top] = new TextureInfo(new Color(135, 206, 235));
        Theme.SideTextures[Side.Bottom] = new TextureInfo(new Color(64, 64, 64));
        Theme.SideTextures[Side.North] = new TextureInfo(new Color(255, 182, 193));
        Theme.SideTextures[Side.South] = new TextureInfo(new Color(144, 238, 144));
        Theme.SideTextures[Side.East] = new TextureInfo(new Color(255, 255, 224));
        Theme.SideTextures[Side.West] = new TextureInfo(new Color(224, 255, 255));
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return TriangleMaker.BuildCuboid(this);
    }
}
