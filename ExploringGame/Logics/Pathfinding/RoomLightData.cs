using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Logics.Pathfinding;

public record LightContribution(ILightSource LightSource, float Amount);

/// <summary>
/// Stores lighting information for a room, including
/// light contributions from various light sources.
/// </summary>
public class RoomLightData : IWithRoom
{
    public IRoom Room { get; set; }

    private Dictionary<ILightSource, LightContribution> _lightContributions = new();
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
        _lightContributions[lightSource] = new LightContribution(lightSource, contribution);
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

        var sortedContributions = _lightContributions.Values.Select(p=>p.Amount).OrderByDescending(x => x);
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

    public IEnumerable<LightContribution> SortedContributions => _lightContributions.Values.Where(p => p.Amount > 0).OrderByDescending(p => p.Amount);

    public override string ToString() => $"Light Data ({Room}) = {TotalLight}";  
}
