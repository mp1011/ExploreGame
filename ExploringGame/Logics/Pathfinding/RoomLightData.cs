using ExploringGame.GeometryBuilder.Shapes;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Logics.Pathfinding;

/// <summary>
/// Stores lighting information for a room, including
/// light contributions from various light sources.
/// </summary>
public class RoomLightData : IWithRoom
{
    public Room Room { get; set; }

    private Dictionary<ILightSource, float> _lightContributions = new();
    private float _cachedTotalLight = 0f;

    public RoomLightData(Room room)
    {
        Room = room;
    }

    /// <summary>
    /// Stores light contribution from a specific source
    /// </summary>
    public void SetLightContribution(ILightSource lightSource, float contribution)
    {
        _lightContributions[lightSource] = contribution;
    }

    /// <summary>
    /// Removes a light source's contribution
    /// </summary>
    public void RemoveLightContribution(ILightSource lightSource)
    {
        _lightContributions.Remove(lightSource);
    }

    /// <summary>
    /// Gets total light value for this room using additive blending
    /// </summary>
    public float GetTotalLight(float maxBrightness = 10.0f)
    {
        if (!_lightContributions.Any())
            return 0f;

        var sortedContributions = _lightContributions.Values.OrderByDescending(x => x);
        float brightness = 0f;

        foreach (var contribution in sortedContributions)
        {
            brightness += contribution * (1 - brightness / maxBrightness);
        }

        return brightness;
    }

    /// <summary>
    /// Gets cached total light value.
    /// </summary>
    public float TotalLight => _cachedTotalLight;

    /// <summary>
    /// Recalculates and updates the cached total light level.
    /// </summary>
    public float RecalculateLightLevel()
    {
        _cachedTotalLight = GetTotalLight();
        return _cachedTotalLight;
    }

    /// <summary>
    /// Gets all light sources contributing to this room
    /// </summary>
    public IEnumerable<ILightSource> GetLightSources()
    {
        return _lightContributions.Keys;
    }

    /// <summary>
    /// Gets only light sources that are physically located in this room's lighting group.
    /// This is different from GetLightSources() which includes lights from neighboring rooms
    /// that contribute to ambient lighting.
    /// </summary>
    public IEnumerable<ILightSource> GetLightSourcesInRoom()
    {
        return _lightContributions.Keys.Where(light => light.Room?.LightingGroup == Room);
    }

    public override string ToString() => $"Light Data ({Room}) = {TotalLight}";  
}
