using ExploringGame.GeometryBuilder;
using ExploringGame.Services;
using ExploringGame.Texture;
using System.Linq;

namespace ExploringGame.GeometryBuilder.Shapes.Skyboxes;

public class SkyDome : SkyboxShape
{
    private static SkyDome _instance;
    public static SkyDome Instance => _instance ??= new SkyDome();

    public override Theme Theme { get; }

    private SkyDome()
    {
        Theme = new SkyTheme();
        Height = 100f;
        Position = new Microsoft.Xna.Framework.Vector3(0, -12.5f, 0);
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return TriangleMaker.BuildDome(this, segments: 32);
    }
}
