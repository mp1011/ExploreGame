using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.Logics;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.Testing;

public class TestShapeStampGenerator : PlaceableShape, IControllable
{
    public override CollisionGroup CollisionGroup => CollisionGroup.None;
    public override CollisionGroup CollidesWithGroups => CollisionGroup.None;

    public TestShapeStampGenerator()
    {
        Width = 1.0f;
        Height = 0.5f;
        Depth = 1.0f;
        MainTexture = new TextureInfo(Color.Purple, TextureKey.Wall);
    }

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public IActiveObject CreateController(ServiceContainer serviceContainer)
    {
        var controller = serviceContainer.Get<TestShapeStampGeneratorController>();
        controller.Generator = this;
        return controller;
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid();
    }
}
