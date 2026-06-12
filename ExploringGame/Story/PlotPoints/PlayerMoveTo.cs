using ExploringGame.Entities;
using Microsoft.Xna.Framework;

namespace ExploringGame.Story.PlotPoints;

public class PlayerMoveTo : PlotPoint
{
    private Vector3 _position;
    private Player _player;

    public PlayerMoveTo(Player player, Vector3 position, params PlotPoint[] requiredDone): base(requiredDone)
    {
        _player = player;
        _position = position;
    }

    protected override bool CheckActivation(Microsoft.Xna.Framework.GameTime gameTime) => true;

    protected override PlotUpdate UpdateActive(Microsoft.Xna.Framework.GameTime gameTime)
    {
        _player.WorldPosition = _position;
        _player.Mover.RefreshPosition();
        return PlotUpdate.End;
    }
}
