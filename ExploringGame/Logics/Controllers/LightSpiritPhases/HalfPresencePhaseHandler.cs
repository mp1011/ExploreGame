using ExploringGame.Entities;
using Microsoft.Xna.Framework;

namespace ExploringGame.Logics.Controllers.LightSpiritPhases;

public class HalfPresencePhaseHandler : IPhaseHandler
{
    private readonly LightSpirit _lightSpirit;
    private readonly Player _player;

    public HalfPresencePhaseHandler(LightSpirit lightSpirit, Player player)
    {
        _lightSpirit = lightSpirit;
        _player = player;
    }

    public void OnEnter()
    {
        // TODO: Initialize half-presence phase
    }

    public void Update(GameTime gameTime)
    {
        // TODO: Implement half-presence phase logic
    }

    public void OnExit()
    {
        // Clean up if needed
    }
}
