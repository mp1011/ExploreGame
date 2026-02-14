using ExploringGame.GameDebug;
using Microsoft.Xna.Framework;

namespace ExploringGame.Logics.Controllers.LightSpiritPhases;

/// <summary>
/// Interface for Light Spirit phase behavior handlers
/// </summary>
public interface IPhaseHandler : IDebugControllable
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

    /// <summary>
    /// Forces the phase to transition to the next phase by setting up the state
    /// as if the phase transition conditions were met naturally
    /// </summary>
    void ForceNextPhase();
}

