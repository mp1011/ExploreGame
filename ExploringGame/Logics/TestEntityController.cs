using ExploringGame.Entities;
using ExploringGame.Services;
using Microsoft.Xna.Framework;
using static ExploringGame.Services.Physics;

namespace ExploringGame.Logics;

/// <summary>
/// Controls a test entity's behavior - moves slowly toward the player
/// </summary>
public class TestEntityController : IShapeController<TestEntity>
{
    private const float MoveSpeed = 1.0f;
    private const float Acceleration = 0.2f;
    private const float Gravity = 10.0f;
    private const float GravityAccel = 0.4f;

    private readonly Player _player;
    private readonly Physics _physics;
    private EntityMover _entityMover;

    public TestEntity Shape { get; set; }

    public TestEntityController(Player player, Physics physics)
    {
        _player = player;
        _physics = physics;
    }

    public void Initialize()
    {
        _entityMover = new EntityMover(Shape, _physics);
        _entityMover.CollisionResponder.AddResponse(new Collision.DetectFloorCollision(_entityMover));
        _entityMover.Initialize();
        
        // Set up motion parameters
        _entityMover.Motion.Acceleration = Acceleration;
        _entityMover.Motion.Gravity = GravityAccel;
    }

    public void Stop()
    {
        _entityMover?.Stop();
        _entityMover = null;
    }

    public void Update(GameTime gameTime)
    {
        if (_entityMover == null)
            return;

        // Calculate direction to player
        var directionToPlayer = _player.Position - Shape.Position;
        
        // Only move in XZ plane (horizontal movement)
        directionToPlayer.Y = 0;
        
        // Normalize and apply speed
        if (directionToPlayer.LengthSquared() > 0.01f)
        {
            directionToPlayer.Normalize();
            _entityMover.Motion.TargetMotion = directionToPlayer * MoveSpeed;
            
            // Rotate to face movement direction
            // North side (yellow front) needs 180 degree offset to face forward
            float targetYaw = (float)System.Math.Atan2(directionToPlayer.X, directionToPlayer.Z) + (float)System.Math.PI;
            Shape.Rotation = new GeometryBuilder.Rotation(targetYaw, 0, 0);
        }
        else
        {
            _entityMover.Motion.TargetMotion = Vector3.Zero;
        }

        // Apply gravity
        _entityMover.Motion.TargetY = Gravity;

        // Update motion and physics
        _entityMover.Update(gameTime);
    }
}
