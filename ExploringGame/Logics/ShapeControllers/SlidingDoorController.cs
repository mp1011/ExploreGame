using ExploringGame.Entities;
using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder.Shapes.Structures;
using ExploringGame.LevelControl;
using ExploringGame.Logics.Collision;
using ExploringGame.Services;
using Jitter2.Dynamics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExploringGame.Logics.ShapeControllers;

public class SlidingDoorController : IShapeController<SlidingDoorPane>, IPlayerActivated
{
    private readonly Physics _physics;
    private readonly GameState _gameState;
    private RigidBody _rigidBody;

    public SlidingDoorPane Shape { get; set; }

    public float ActivationRange => 2.0f;

    public IPlayerInput PlayerInput { get; }

    public Player Player { get; }

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
        _rigidBody.Position = Shape.Position.ToJVector();
    }

    public void Stop()
    {
    }

    public void Update(GameTime gameTime)
    {
        Shape.Position = _rigidBody.Position.ToVector3();

    }
}
