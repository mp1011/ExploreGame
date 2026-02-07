using ExploringGame.GameDebug;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Logics.Pathfinding;

public class WaypointGraph
{
    private Dictionary<Room, Waypoint> _waypoints = new();
    private WorldSegment _worldSegment;

    public WaypointGraph(WorldSegment worldSegment)
    {
        _worldSegment = worldSegment;
        BuildGraph(worldSegment);
    }

    private void BuildGraph(WorldSegment worldSegment)
    {
        // Get all rooms in the world segment
        var rooms = worldSegment.TraverseAllChildren().OfType<Room>().ToList();

        // Create waypoint for each room
        foreach (var room in rooms)
        {
            var waypoint = new Waypoint(room);
            _waypoints[room] = waypoint;
            worldSegment.AddChild(waypoint);
        }

        // Connect waypoints based on room connections
        foreach (var room in rooms)
        {
            var waypoint = _waypoints[room];
            
            foreach (var connection in room.RoomConnections)
            {
                var connectedRoom = connection.GetOtherRoom(room);
                if (connectedRoom != null && _waypoints.ContainsKey(connectedRoom))
                {
                    var connectedWaypoint = _waypoints[connectedRoom];
                    waypoint.AddNeighbor(connectedWaypoint);
                }
            }
        }
    }

    public Waypoint GetWaypointForRoom(Room room)
    {
        return _waypoints.TryGetValue(room, out var waypoint) ? waypoint : null;
    }

    public Room GetRoomContaining(Vector3 position)
    {
        foreach (var room in _waypoints.Keys)
        {
            if (room.ContainsPoint(position))
                return room;
        }
        return null;
    }

    public Waypoint FindNearestWaypoint(Vector3 position)
    {
        // First, try to find the room containing this position
        var containingRoom = GetRoomContaining(position);
        if (containingRoom != null && _waypoints.TryGetValue(containingRoom, out var roomWaypoint))
        {
            return roomWaypoint;
        }

        // Fallback: find closest waypoint
        Waypoint nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (var waypoint in _waypoints.Values)
        {
            var distance = Vector3.DistanceSquared(position, waypoint.Position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = waypoint;
            }
        }

        return nearest;
    }

    public List<Waypoint> FindPath(Waypoint start, Waypoint goal)
    {
        if (start == null || goal == null || start == goal)
            return new List<Waypoint>();

        var openSet = new HashSet<Waypoint> { start };
        var cameFrom = new Dictionary<Waypoint, Waypoint>();
        var gScore = new Dictionary<Waypoint, float> { [start] = 0 };
        var fScore = new Dictionary<Waypoint, float> { [start] = Heuristic(start, goal) };

        while (openSet.Count > 0)
        {
            // Find node in openSet with lowest fScore
            var current = openSet.OrderBy(n => fScore.GetValueOrDefault(n, float.MaxValue)).First();

            if (current == goal)
            {
                return ReconstructPath(cameFrom, current);
            }

            openSet.Remove(current);

            foreach (var neighbor in current.Neighbors)
            {
                var tentativeGScore = gScore[current] + Vector3.Distance(current.Position, neighbor.Position);

                if (tentativeGScore < gScore.GetValueOrDefault(neighbor, float.MaxValue))
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + Heuristic(neighbor, goal);

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        // No path found
        return new List<Waypoint>();
    }

    private float Heuristic(Waypoint a, Waypoint b)
    {
        return Vector3.Distance(a.Position, b.Position);
    }

    private List<Waypoint> ReconstructPath(Dictionary<Waypoint, Waypoint> cameFrom, Waypoint current)
    {
        var path = new List<Waypoint> { current };
        
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }

        return path;
    }
}
