using ExploringGame.Entities;
using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder.Shapes.Appliances;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ExploringGame.Logics.ShapeControllers;

public class LightSwitchController : IShapeController<LightSwitch>, IOnOff
{
    public const float ActivationRange = 2.0f;

    private readonly PlayerInput _playerInput;
    private readonly Player _player;

    public LightSwitchController(PlayerInput playerInput, Player player)
    {
        _playerInput = playerInput;
        _player = player;
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

    public void Initialize()
    {
        On = true;
    }

    public void Update(GameTime gameTime)
    {
        if (_player.Position.SquaredDistance(Shape.Position) > ActivationRange * ActivationRange)
            return;

        if (_playerInput.IsKeyPressed(GameKey.Use))
            On = !On;
    }
}
