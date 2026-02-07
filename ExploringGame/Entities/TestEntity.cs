using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
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
    public float MoveSpeed { get; set; } = 4.0f;    
    public override IColliderMaker ColliderMaker => new BoxColliderMaker(this);

    public override CollisionGroup CollisionGroup => CollisionGroup.SolidEntity;
    public override CollisionGroup CollidesWithGroups => CollisionGroup.Player | CollisionGroup.Environment | CollisionGroup.Steps;

    public TestEntity()
    {
        Width = 0.5f;
        Height = 1.0f;
        Depth = 1.0f;
        
        MainTexture = new TextureInfo(Color.Red, TextureKey.Wall);
        // North side is the "front" - make it yellow so we can see which way it's facing
        SideTextures[Side.North] = new TextureInfo(Color.Yellow, TextureKey.Wall);
        
        Rotation = new Rotation(0, 0, 0);
    }

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public IActiveObject CreateController(ServiceContainer serviceContainer)
    {
        var controller = serviceContainer.Get<TestEntityController>();
        controller.Shape = this;
        controller.WorldSegment = Parent?.FindFirstAncestor<WorldSegment>();
        return controller;
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid();
    }
}
