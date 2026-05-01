using ExploringGame.Entities;
using ExploringGame.Logics;
using Microsoft.Xna.Framework;

namespace ExploringGame.Story.PlotPoints;

public class PlayerFreeze : PlotPoint
{
    private readonly Player _player;
    private readonly PlayerMotion _playerMotion;

    public PlayerFreeze(Player player, PlayerMotion playerMotion, params PlotPoint[] requiredDone) : base(requiredDone)
    {
        _player = player;
        _playerMotion = playerMotion;
    }

    protected override bool CheckActivation(GameTime gameTime) => true;

    protected override PlotUpdate UpdateActive(GameTime gameTime)
    {
        _player.Mover.Active = false;
        _playerMotion.Active = false;
        return PlotUpdate.End;
    }
}
