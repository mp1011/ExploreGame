using ExploringGame.Entities;
using Microsoft.Xna.Framework;

namespace ExploringGame.Logics.Controllers.LightSpiritPhases;

public class FullPresencePhaseHandler : IPhaseHandler
{
    private readonly LightSpirit _lightSpirit;
    private readonly Player _player;

    public FullPresencePhaseHandler(LightSpirit lightSpirit, Player player)
    {
        _lightSpirit = lightSpirit;
        _player = player;
    }

    public void OnEnter()
    {
        // TODO: Initialize full-presence phase
    }

    public void Update(GameTime gameTime)
    {
        // TODO: Implement full-presence phase logic
    }

    public void OnExit()
    {
        // Clean up if needed
    }

    public string DebugDescribe()
    {
        return string.Empty;
    }
}

