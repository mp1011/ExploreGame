using Microsoft.Xna.Framework;

namespace ExploringGame.Logics.Controllers.LightSpiritPhases;

/// <summary>
/// Interface for Light Spirit phase behavior handlers
/// </summary>
public interface IPhaseHandler
{
    /// <summary>
    /// Updates the phase behavior
    /// </summary>
    /// <param name="gameTime">Game time</param>
    void Update(GameTime gameTime);

    /// <summary>
    /// Called when entering this phase
    /// </summary>
    void OnEnter();

    /// <summary>
    /// Called when exiting this phase
    /// </summary>
    void OnExit();

    /// <summary>
    /// Returns debug information about the current phase state
    /// </summary>
    string DebugDescribe();
}

