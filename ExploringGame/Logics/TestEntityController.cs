using ExploringGame.Entities;
using ExploringGame.GameDebug;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics.Pathfinding;
using ExploringGame.Services;
using Microsoft.Xna.Framework;
using static ExploringGame.Services.Physics;

namespace ExploringGame.Logics;

/// <summary>
/// Controls a test entity's behavior - moves slowly toward the player using pathfinding
/// </summary>
public class TestEntityController : IShapeController<TestEntity>
{
    private const float Acceleration = 0.2f;
    private const float Gravity = 10.0f;
    private const float GravityAccel = 0.4f;

    private readonly Player _player;
    private readonly Physics _physics;
    private EntityMover _entityMover;
    private PathFinder _pathFinder;

    public TestEntity Shape { get; set; }
    public WorldSegment WorldSegment { get; set; }

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

        // Get waypoint graph from WorldSegment
        if (WorldSegment?.WaypointGraph != null)
        {
            _pathFinder = new PathFinder(_physics, WorldSegment.WaypointGraph);
        }
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



        // Use pathfinder if available, otherwise use simple direct movement
        Vector3 targetDirection;
        if (_pathFinder != null)
        {
            targetDirection = _pathFinder.GetTargetDirection(Shape, _player);
        }
        else
        {
            // Fallback: simple direct movement
            targetDirection = _player.Position - Shape.Position;
            targetDirection.Y = 0;
            if (targetDirection.LengthSquared() > 0.01f)
            {
                targetDirection.Normalize();
            }
        }
        
        // Apply speed and movement
        if (targetDirection.LengthSquared() > 0.01f)
        {
            _entityMover.Motion.TargetMotion = targetDirection * Shape.MoveSpeed;
            
            // Rotate to face movement direction
            // North side (yellow front) needs 180 degree offset to face forward
            float targetYaw = (float)System.Math.Atan2(targetDirection.X, targetDirection.Z) + (float)System.Math.PI;
            Shape.Rotation = new GeometryBuilder.Rotation(targetYaw, 0, 0);
        }
        else
        {
            _entityMover.Motion.TargetMotion = Vector3.Zero;
        }

        // Apply gravity
        _entityMover.Motion.TargetY = Gravity;

        // Stop moving in FlyMode for testing
        if (Debug.FlyMode)
        {
            _entityMover.Motion.TargetMotion = Vector3.Zero;
        }

        // Update motion and physics
        _entityMover.Update(gameTime);       
    }
}
