using ExploringGame.Entities;
using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes.Furniture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ExploringGame.Logics.ShapeControllers;

public class DoorController : IShapeController<Door>
{
    public float ActivationRange = 2.0f;

    public Door Shape { get; set; }

    private readonly PlayerInput _playerInput;
    private readonly Player _player;

    public DoorController(PlayerInput playerInput, Player player)
    {
        _playerInput = playerInput;
        _player = player;
    }

    public void Update(GameTime gameTime)
    {
        if (_player.Position.SquaredDistance(Shape.Position) < ActivationRange * ActivationRange)
        {
            if (_playerInput.IsKeyPressed(GameKey.Use))
                Shape.Open = !Shape.Open;
        }

        var targetDegrees = Shape.Open ? Shape.OpenDegrees : Shape.ClosedDegrees;
        AdjustAngle(targetDegrees);

        PlaceDoor();
    }


    private void AdjustAngle(float targetDegrees)
    {
        if (Shape.Angle.Degrees == targetDegrees)
            return;

        Shape.Angle = Shape.Angle.RotateTowards(targetDegrees, Shape.OpenSpeed);
    }

    private void PlaceDoor()
    {
        Vector3 d = new Vector3(Shape.Width / 2.0f, 0, 0);
        d = Vector3.Transform(d, Shape.Rotation.AsMatrix());
        Shape.Position = Shape.Hinge - d;
    }

}
