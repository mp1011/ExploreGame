using ExploringGame.Audio;
using ExploringGame.Entities;
using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes.Structures;
using ExploringGame.LevelControl;
using ExploringGame.Logics.Collision;
using ExploringGame.Services;
using Jitter2.Dynamics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExploringGame.Logics.ShapeControllers;

public class SlidingDoorController : IShapeController<MovingSlidingDoorPane>, IPlayerActivated
{
    private readonly float OpenSpeed = 3.0f;
    private readonly Physics _physics;
    private readonly GameState _gameState;
    private float _closedPosition, _openPosition;
  
    private RigidBody _rigidBody;

    public MovingSlidingDoorPane Shape { get; set; }

    public float ActivationRange => 2.0f;

    public IPlayerInput PlayerInput { get; }

    public Player Player { get; }

    public bool Open { get; set; }

    public float TargetPosition => Open ? _openPosition : _closedPosition;

    public float CurrentAxisPosition => Shape.GetWorldAxisPosition(Shape.OpenAxis);

    ICollidable IPlayerActivated.Shape => Shape;
    public SlidingDoorController(IPlayerInput playerInput, Player player, AudioService audioService, Physics physics,
      GameState gameState)
    {
        _physics = physics;
        //_audioService = audioService;
        PlayerInput = playerInput;
        Player = player;
        _gameState = gameState;
    }

    public void Initialize()
    {
        _rigidBody = Shape.ColliderBodies.First();
        _rigidBody.Position = Shape.WorldPosition.ToJVector();

        _closedPosition = Shape.WorldPosition.AxisValue(Shape.OpenAxis);
        _openPosition = (Shape.WorldPosition + (Shape.OpenSide.AsVector() * 1.2f)).AxisValue(Shape.OpenAxis);
    }

    public void Stop()
    {
    }

    public void Update(GameTime gameTime)
    {
        Shape.WorldPosition = _rigidBody.Position.ToVector3();

        if (this.CheckPlayerActivation(_physics))
            Open = !Open;

        var distance = (CurrentAxisPosition - TargetPosition).Abs();
        if(distance < 0.01f)
        {
            _rigidBody.Position = Shape.WorldPosition.SetAxis(Shape.OpenAxis, TargetPosition).ToJVector();
            _rigidBody.Velocity = new Jitter2.LinearMath.JVector(0, 0, 0);
        }
        else
        {
            var speed = OpenSpeed * float.Sign(TargetPosition - CurrentAxisPosition);
            _rigidBody.Velocity = Vector3.Zero.SetAxis(Shape.OpenAxis, speed).ToJVector();
        }
    }
}
