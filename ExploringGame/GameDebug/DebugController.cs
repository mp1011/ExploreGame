using ExploringGame.Entities;
using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Logics.Controllers;
using ExploringGame.Services;
using Microsoft.Xna.Framework.Input;
using System.Linq;

namespace ExploringGame.GameDebug;

public class DebugController
{
    private IPlayerInput _playerInput;
    private CameraService _cameraService;
    private Player _player;
    private LoadedLevelData _loadedLevelData;

    public DebugController(IPlayerInput playerInput, CameraService cameraService, Player player, LoadedLevelData loadedLevelData)
    {
        _player = player;
        _playerInput = playerInput;
        _cameraService = cameraService;
        _loadedLevelData = loadedLevelData;
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
        else if (_playerInput.IsKeyPressed(Keys.G))
        {
            Debug.FlyMode = !Debug.FlyMode;
        }
        else if (_playerInput.IsKeyPressed(Keys.D))
        {
            Debug.NoDepthStencil = !Debug.NoDepthStencil;
        }
        else if (_playerInput.IsKeyPressed(Keys.PageDown))
        {
            // Find LightSpiritController from loaded level data
            var lightSpiritController = _loadedLevelData.LoadedSegments
                .SelectMany(segment => segment.ActiveObjects)
                .OfType<LightSpiritController>()
                .FirstOrDefault();

            lightSpiritController?.ForceAdvancePhase();
        }
    }
}
