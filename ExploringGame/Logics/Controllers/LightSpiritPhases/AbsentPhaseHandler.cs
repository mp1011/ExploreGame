using ExploringGame.Entities;
using ExploringGame.Extensions;
using Microsoft.Xna.Framework;
using System;

namespace ExploringGame.Logics.Controllers.LightSpiritPhases;

public class AbsentPhaseHandler : IPhaseHandler
{
    private readonly LightSpirit _lightSpirit;
    private readonly TimedAction _healthRegenAction;
    private readonly TimedAction _phaseTransitionCheck;
    
    private const float UndergroundY = -100f;
    private const int HealthRegenPerSecond = 2;

    public AbsentPhaseHandler(LightSpirit lightSpirit)
    {
        _lightSpirit = lightSpirit;
        
        // Initialize timed actions
        _healthRegenAction = new TimedAction(TimeSpan.FromSeconds(1), () =>
        {
            // Health regeneration logic
            _lightSpirit.Health = Math.Min(100, _lightSpirit.Health + HealthRegenPerSecond);
        });

        _phaseTransitionCheck = new TimedAction(TimeSpan.FromSeconds(60), () => { }, false);
    }

    public void OnEnter()
    {
        _lightSpirit.Health = 0;
        SetUndergroundPosition();
        _healthRegenAction.Reset();
        _phaseTransitionCheck.Reset();
    }

    public void Update(GameTime gameTime)
    {
        // Keep underground
        SetUndergroundPosition();

        // Update timed actions
        _healthRegenAction.Update(gameTime);
        _phaseTransitionCheck.Update(gameTime);

        // Check transition conditions:
        // 1. At least 60 seconds have passed
        // 2. Health has reached 100
        if (_phaseTransitionCheck.IsReady && _lightSpirit.Health >= 100)
        {
            // Transition to Break-in Phase
            _lightSpirit.Phase = LightSpiritPhase.BreakIn;
        }
    }

    public void OnExit()
    {
        // Clean up if needed
    }

    private void SetUndergroundPosition()
    {
        _lightSpirit.Position = new Vector3(0, UndergroundY, 0);
        _lightSpirit.Sphere.Position = new Vector3(0, UndergroundY, 0);
        
        // Update physics body position
        if (_lightSpirit.Sphere.ColliderBodies != null && _lightSpirit.Sphere.ColliderBodies.Length > 0)
        {
            _lightSpirit.Sphere.ColliderBodies[0].Position = _lightSpirit.Position.ToJVector();
        }
    }
}
