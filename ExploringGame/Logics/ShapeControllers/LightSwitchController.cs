using ExploringGame.Entities;
using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.LevelControl;
using Microsoft.Xna.Framework;

namespace ExploringGame.Logics.ShapeControllers;

public class LightSwitchController : IShapeController<LightSwitch>, IOnOff, IPlayerActivated
{
    private readonly PlayerInput _playerInput;
    private readonly Player _player;
    private readonly GameState _gameState;

    public StateKey StateKey => Shape.StateKey;

    public LightSwitchController(PlayerInput playerInput, Player player, GameState gameState)
    {
        _playerInput = playerInput;
        _player = player;
        _gameState = gameState;
    }

    public LightSwitch Shape { get; set; }

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

    PlayerInput IPlayerActivated.PlayerInput => _playerInput;

    Player IPlayerActivated.Player => _player;

    Shape IPlayerActivated.Shape => Shape;
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
        if(this.CheckPlayerActivation())
            On = !On;
    }
}
