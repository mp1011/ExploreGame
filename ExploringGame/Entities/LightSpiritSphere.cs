using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.Logics.Collision;
using ExploringGame.Services;
using ExploringGame.Texture;
using Jitter2.Dynamics;
using Microsoft.Xna.Framework;

namespace ExploringGame.Entities;

/// <summary>
/// Spherical child shape of the Light Spirit. Used for collision and visual appearance.
/// </summary>
public class LightSpiritSphere : PlaceableShape, ICollidable
{
    private readonly LightSpirit _parent;
    private const float Radius = 0.5f;

    public override CollisionGroup CollisionGroup => CollisionGroup.SolidEntity;
    public override CollisionGroup CollidesWithGroups => CollisionGroup.Player | CollisionGroup.Environment;
    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public RigidBody[] ColliderBodies { get; private set; }

    public LightSpiritSphere(LightSpirit parent)
    {
        _parent = parent;
        Width = Radius * 2;
        Height = Radius * 2;
        Depth = Radius * 2;
        
        // Glowing white/light appearance
        MainTexture = new TextureInfo(Color.White, TextureKey.Wall);
    }

    public void InitializePhysics(Physics physics)
    {
        var body = physics.CreateSphere(this, Radius);
        ColliderBodies = new[] { body };
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildSphere(16, 16);
    }
}
