namespace ExploringGame.Logics;

/// <summary>
/// Interface for objects that can provide ambient lighting information
/// </summary>
public interface ILightingGroup
{
    /// <summary>
    /// If set, this value is used as the ambient light instead of LightIntensity.DefaultAmbientLight
    /// </summary>
    float? FixedAmbientLight { get; }
}
