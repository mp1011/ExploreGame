using ExploringGame.Entities;
using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.Services;
using Jitter2.Dynamics;
using Jitter2.Dynamics.Constraints;
using Jitter2.LinearMath;
using Microsoft.Xna.Framework;
using System.Linq;

namespace ExploringGame.Logics.ShapeControllers;

public class DoorController : IShapeController<Door>
{
    public float ActivationRange = 2.0f;

    public Door Shape { get; set; }

    private readonly PlayerInput _playerInput;
    private readonly Player _player;
    private readonly AudioService _audioService;
    private readonly Physics _physics;
    private RigidBody _rigidBody;
    private AngularMotor _motor;
    private bool _closeSoundPlayed = true;

    public DoorController(PlayerInput playerInput, Player player, AudioService audioService, Physics physics)
    {
        _physics = physics;
        _audioService = audioService;
        _playerInput = playerInput;
        _player = player;
    }


    public void Initialize()
    {
        _rigidBody = Shape.ColliderBodies.First();
        //_motor = _rigidBody.Constraints.OfType<AngularMotor>().First();
    }

    public void Stop()
    {
        _rigidBody = null;
    }

    public void Update(GameTime gameTime)
    {
        var targetDegrees = Shape.Open ? Shape.OpenAngle : Shape.ClosedAngle;

        // door translate door angle to world angle
        if (Shape.HingePosition == HAlign.Left)                 
            targetDegrees = targetDegrees.RotateCounterClockwise(90);
        else
            targetDegrees = targetDegrees.RotateClockwise(90);


        var delta = new Angle(Shape.Rotation.YawDegrees).Delta(targetDegrees);

        if (delta.IsAlmost(0f, tolerance: 2.0f) && !Shape.Open && !_closeSoundPlayed)
        {
            _audioService.Play(SoundEffectKey.DoorClose);
            _closeSoundPlayed = true;
        }

        _rigidBody.AngularVelocity = new JVector(0, delta * .05f, 0f);

        Shape.Position = _rigidBody.Position.ToVector3();
        Shape.Rotation = Rotation.FromJQuaternion(_rigidBody.Orientation);

        if (_player.Position.SquaredDistance(Shape.Position) > ActivationRange * ActivationRange)
            return;

        if (_playerInput.IsKeyPressed(GameKey.Use))
        {
            Shape.Open = !Shape.Open;
            if(Shape.Open)
            {
                _audioService.Play(SoundEffectKey.DoorOpen);
                _closeSoundPlayed = false;
            }
        }
       
    }
}
