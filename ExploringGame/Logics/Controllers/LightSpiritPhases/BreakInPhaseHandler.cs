using ExploringGame.Entities;
using Microsoft.Xna.Framework;

namespace ExploringGame.Logics.Controllers.LightSpiritPhases;

public class BreakInPhaseHandler : IPhaseHandler
{
    private readonly LightSpirit _lightSpirit;

    public BreakInPhaseHandler(LightSpirit lightSpirit)
    {
        _lightSpirit = lightSpirit;
    }

    public void OnEnter()
    {
        // TODO: Initialize break-in phase
    }

    public void Update(GameTime gameTime)
    {
        // TODO: Implement break-in phase logic
    }

    public void OnExit()
    {
        // Clean up if needed
    }
}
