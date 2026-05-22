using ExploringGame.Entities;
using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.LevelControl;
using ExploringGame.Logics.Collision;
using ExploringGame.Services;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ExploringGame.Logics.ShapeControllers;


public interface ISwitchShape : ICollidable, IShape
{
    List<IOnOff> ControlledObjects { get; }
    StateKey StateKey { get; }
    bool On { get; set; }

    LightSwitchController Controller { get; }
}

public class LightSwitchController : IShapeController<ISwitchShape>, IOnOff, IPlayerActivated
{
    private readonly IPlayerInput _playerInput;
    private readonly Player _player;
    private readonly GameState _gameState;
    private readonly Physics _physics;

    public StateKey StateKey => Shape.StateKey;

    public LightSwitchController(IPlayerInput playerInput, Player player, GameState gameState, Physics physics)
    {
        _physics = physics;
        _playerInput = playerInput;
        _player = player;
        _gameState = gameState;
    }

    public ISwitchShape Shape { get; set; }

    private bool _on;
    public bool On
    {
        get => _on;
        set
        {
            _on = value;
            foreach (var item in Shape.ControlledObjects)
                item.On = value;
        }
    }

    #region IPlayerActivated
    float IPlayerActivated.ActivationRange => 2.0f;

    IPlayerInput IPlayerActivated.PlayerInput => _playerInput;

    Player IPlayerActivated.Player => _player;

    ICollidable IPlayerActivated.Shape => Shape;
    #endregion

    public void Initialize()
    {
        this.LoadState(_gameState);
    }

    public void Stop()
    {
        this.SaveState(_gameState);
    }

    public void Update(GameTime gameTime)
    {
        if(this.CheckPlayerActivation(_physics))
            On = !On;
    }
}
