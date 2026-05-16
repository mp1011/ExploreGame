using ExploringGame.GeometryBuilder.Shapes;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Logics.Pathfinding;

public class RoomGraph
{
    private readonly Dictionary<Room, List<Room>> _adjacency = new();

    public void AddRoom(Room room)
    {
        if (!_adjacency.ContainsKey(room))
        {
            _adjacency[room] = new List<Room>();
        }
    }

    public void AddConnection(Room room1, Room room2)
    {
        AddRoom(room1);
        AddRoom(room2);

        if (!_adjacency[room1].Contains(room2))
        {
            _adjacency[room1].Add(room2);
        }

        if (!_adjacency[room2].Contains(room1))
        {
            _adjacency[room2].Add(room1);
        }
    }

    public IEnumerable<Room> GetNeighbors(Room room)
    {
        return _adjacency.TryGetValue(room, out var neighbors) ? neighbors : Enumerable.Empty<Room>();
    }

    public IEnumerable<Room> GetAllRooms()
    {
        return _adjacency.Keys;
    }

    public List<Room> FindPath(Room start, Room goal)
    {
        if (start == null || goal == null || start == goal)
            return new List<Room>();

        if (!_adjacency.ContainsKey(start) || !_adjacency.ContainsKey(goal))
            return new List<Room>();

        var openSet = new HashSet<Room> { start };
        var cameFrom = new Dictionary<Room, Room>();
        var gScore = new Dictionary<Room, float> { [start] = 0 };
        var fScore = new Dictionary<Room, float> { [start] = Heuristic(start, goal) };

        while (openSet.Count > 0)
        {
            var current = openSet.OrderBy(n => fScore.GetValueOrDefault(n, float.MaxValue)).First();

            if (current == goal)
            {
                return ReconstructPath(cameFrom, current);
            }

            openSet.Remove(current);

            foreach (var neighbor in GetNeighbors(current))
            {
                var tentativeGScore = gScore[current] + Heuristic(current, neighbor);

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

        return new List<Room>();
    }

    private float Heuristic(Room a, Room b)
    {
        return Microsoft.Xna.Framework.Vector3.Distance(a.Position, b.Position);
    }

    private List<Room> ReconstructPath(Dictionary<Room, Room> cameFrom, Room current)
    {
        var path = new List<Room> { current };

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }

        return path;
    }
}
