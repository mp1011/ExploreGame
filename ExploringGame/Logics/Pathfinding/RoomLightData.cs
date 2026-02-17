using ExploringGame.GeometryBuilder.Shapes;
using System.Collections.Generic;

namespace ExploringGame.Logics.Pathfinding;

/// <summary>
/// Placeholder for future lighting work.
/// Will store lighting information for a room, including
/// light contributions from various light sources.
/// </summary>
public class RoomLightData
{
    public Room Room { get; }

    // TODO: Add light intensity, contributions from each light source, etc.

    private Dictionary<object, float> _lightContributions = new();

    public RoomLightData(Room room)
    {
        Room = room;
    }

    /// <summary>
    /// Placeholder method for storing light contribution from a specific source
    /// </summary>
    public void SetLightContribution(object lightSource, float contribution)
    {
        _lightContributions[lightSource] = contribution;
    }

    /// <summary>
    /// Placeholder method for getting total light value for this room
    /// </summary>
    public float GetTotalLight()
    {
        // TODO: Implement proper light combination algorithm
        return 0f;
    }
}
