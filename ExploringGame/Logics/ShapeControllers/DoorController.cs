using ExploringGame.Entities;
using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes.Furniture;
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
    private RigidBody _rigidBody;
    private AngularMotor _motor;

    public DoorController(PlayerInput playerInput, Player player)
    {
        _playerInput = playerInput;
        _player = player;
    }


    public void Initialize()
    {
        _rigidBody = Shape.ColliderBodies.First();
        //_motor = _rigidBody.Constraints.OfType<AngularMotor>().First();
    }

    public void Update(GameTime gameTime)
    {
        var targetDegrees = Shape.Open ? Shape.OpenAngle : Shape.ClosedAngle;

        // door translate door angle to world angle
        if (Shape.HingePosition == HingePosition.Left)                 
            targetDegrees = targetDegrees.RotateCounterClockwise(90);
        else
            targetDegrees = targetDegrees.RotateClockwise(90);


        var delta = new Angle(Shape.Rotation.YawDegrees).Delta(targetDegrees);


        _rigidBody.AngularVelocity = new JVector(0, delta * .05f, 0f);

        Shape.Position = _rigidBody.Position.ToVector3();
        Shape.Rotation = Rotation.FromJQuaternion(_rigidBody.Orientation);
        if (_player.Position.SquaredDistance(Shape.Position) < ActivationRange * ActivationRange)
        {
            if (_playerInput.IsKeyPressed(GameKey.Use))
                Shape.Open = !Shape.Open;
        }

        Debug.Watch1 = Shape.Rotation.YawDegrees.ToString("00");
    }

    private void PlaceDoor()
    {
        Vector3 d = new Vector3(Shape.Width / 2.0f, 0, 0);
        d = Vector3.Transform(d, Shape.Rotation.AsMatrix());
        Shape.Position = Shape.Hinge - d;
    }

}
