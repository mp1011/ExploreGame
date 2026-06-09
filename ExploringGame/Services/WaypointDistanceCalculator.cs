using ExploringGame.LevelControl;
using ExploringGame.Logics.Pathfinding;
using Microsoft.Xna.Framework;
using System.Linq;

namespace ExploringGame.Services;

/// <summary>
/// Service for calculating the travel distance between two points along the waypoint graph.
/// This provides an estimate of how far an entity would need to travel through the game world
/// by following pathfinding routes, rather than measuring straight-line distance.
/// </summary>
public class WaypointDistanceCalculator
{
    private readonly LoadedLevelData _loadedLevelData;

    public WaypointDistanceCalculator(LoadedLevelData loadedLevelData)
    {
        _loadedLevelData = loadedLevelData;
    }

    /// <summary>
    /// Calculates the path distance between two positions by finding the nearest waypoints
    /// and summing the distances along the path between them.
    /// </summary>
    /// <param name="from">Starting position</param>
    /// <param name="to">Target position</param>
    /// <returns>
    /// The total travel distance along waypoints, or null if no path exists.
    /// Returns 0 if both positions are at the same waypoint.
    /// </returns>
    public float? CalculateDistance(Vector3 from, Vector3 to)
    {
        var waypointGraph = _loadedLevelData.WaypointGraph;
        if (waypointGraph == null)
            return null;

        var fromWaypoint = waypointGraph.FindNearestWaypoint(from);
        var toWaypoint = waypointGraph.FindNearestWaypoint(to);

        if (fromWaypoint == null || toWaypoint == null)
            return null;

        if (fromWaypoint == toWaypoint)
            return 0f;

        var path = waypointGraph.FindPath(fromWaypoint, toWaypoint);
        if (path == null || path.Count == 0)
            return null;

        // Calculate total distance by summing distances between consecutive waypoints
        float totalDistance = 0f;
        for (int i = 0; i < path.Count - 1; i++)
        {
            totalDistance += Vector3.Distance(path[i].LocalPosition, path[i + 1].LocalPosition);
        }

        return totalDistance;
    }
}
