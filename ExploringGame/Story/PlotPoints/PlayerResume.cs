using ExploringGame.Entities;
using ExploringGame.Extensions;
using ExploringGame.Logics;
using ExploringGame.Services;
using Microsoft.Xna.Framework;

namespace ExploringGame.Story.PlotPoints;

public class PlayerResume : PlotPoint
{
    private readonly Player _player;
    private readonly PlayerMotion _playerMotion;
    private readonly CameraService _cameraService;

    public PlayerResume(Player player, PlayerMotion playerMotion, CameraService cameraService, params PlotPoint[] requiredDone) : base(requiredDone)
    {
        _cameraService = cameraService;
        _player = player;
        _playerMotion = playerMotion;
    }

    protected override bool CheckActivation(GameTime gameTime) => true;

    protected override PlotUpdate UpdateActive(GameTime gameTime)
    {
        _player.Mover.Active = true;
        _playerMotion.Active = true;
        _player.Rotation = _cameraService.Current.CreateViewMatrix().RotationFromView();
        _cameraService.SetCamera(_player);
        return PlotUpdate.End;
    }
}
