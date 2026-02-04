using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.Logics;
using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.Entities;

/// <summary>
/// A simple test entity that moves toward the player
/// </summary>
public class TestEntity : PlaceableShape, IWithPosition, IControllable
{
    public override IColliderMaker ColliderMaker => new BoxColliderMaker(this);

    public override CollisionGroup CollisionGroup => CollisionGroup.SolidEntity;
    public override CollisionGroup CollidesWithGroups => CollisionGroup.Player | CollisionGroup.Environment | CollisionGroup.Steps;

    public TestEntity()
    {
        Width = 1.0f;
        Height = 1.0f;
        Depth = 1.0f;
        
        MainTexture = new TextureInfo(Color.Red, TextureKey.Wall);       
        Rotation = new Rotation(0, 0, 0);
    }

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public IActiveObject CreateController(ServiceContainer serviceContainer)
    {
        var controller = serviceContainer.Get<TestEntityController>();
        controller.Shape = this;
        return controller;
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid();
    }
}
