using ExploringGame.Entities;
using ExploringGame.Logics;
using ExploringGame.Services;
using Microsoft.Xna.Framework.Input;

namespace ExploringGame.GameDebug;

public class DebugController
{
    private PlayerInput _playerInput;
    private CameraService _cameraService;
    private Player _player;

    public DebugController(PlayerInput playerInput, CameraService cameraService, Player player)
    {
        _player = player;
        _playerInput = playerInput;
        _cameraService = cameraService;
    }

    public void Update()
    {
        if(_playerInput.IsKeyPressed(Keys.D1))
        {
            _cameraService.SetCamera(new DebugBirdsEyeCamera(_player));
        }
        else if (_playerInput.IsKeyPressed(Keys.D0))
        {
            _cameraService.SetCamera(_player);
        }

    }
}
