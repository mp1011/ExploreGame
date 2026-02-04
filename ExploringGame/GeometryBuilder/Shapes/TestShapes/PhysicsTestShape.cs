using ExploringGame.Extensions;
using ExploringGame.Logics;
using ExploringGame.Logics.Collision;
using ExploringGame.Services;
using Jitter2.Dynamics;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.TestShapes;

public class PhysicsTestShape : Shape, IPlaceableObject, IControllable, ICollidable
{
    public PhysicsTestShape()
    {
        Width = 1.0f;
        Height= 1.0f;
        Depth = 1.0f;
        MainTexture = new Texture.TextureInfo(Color.Green, Texture.TextureKey.Wall);
    }

    public Shape Self => this;

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public CollisionGroup CollisionGroup => CollisionGroup.SolidEntity;

    public CollisionGroup CollidesWithGroups => CollisionGroup.Environment;

    Shape[] IPlaceableObject.Children => TraverseAllChildren();

    public IActiveObject CreateController(ServiceContainer serviceContainer)
    {
        var controller = serviceContainer.Get<PhysicsTestShapeController>();
        controller.Shape = this;
        return controller;
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid();
    }
}

public class PhysicsTestShapeController : IShapeController<PhysicsTestShape>
{
    private readonly Physics _physics;
    private RigidBody _body;

    public PhysicsTestShape Shape { get; set; }

    public PhysicsTestShapeController(Physics physics)
    {
        _physics = physics;
    }

    public void Initialize()
    {
        _body = _physics.CreateDynamicBody(Shape);
        _body.Velocity = new Jitter2.LinearMath.JVector(1.0f, 0.0f, 0.0f);
        _body.Friction = 0.0f;
        _body.Damping = (0f, 0f);
        _body.SetMassInertia(100.0f);
        _body.AffectedByGravity = true;
    }

    public void Stop()
    {
        _body = null;
    }

    public void Update(GameTime gameTime)
    {
        Shape.Position = _body.Position.ToVector3();
    }
}