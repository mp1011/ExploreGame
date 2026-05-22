using ExploringGame.GeometryBuilder.Shapes;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Logics.Pathfinding;

public class RoomGraph
{
    private readonly Dictionary<IRoom, List<IRoom>> _adjacency = new();

    public void AddRoom(IRoom room)
    {
        if (!_adjacency.ContainsKey(room))
        {
            _adjacency[room] = new List<IRoom>();
        }
    }

    public void AddConnection(IRoom room1, IRoom room2)
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

    public IEnumerable<IRoom> GetNeighbors(IRoom room)
    {
        return _adjacency.TryGetValue(room, out var neighbors) ? neighbors : Enumerable.Empty<IRoom>();
    }

    public IEnumerable<IRoom> GetAllRooms()
    {
        return _adjacency.Keys;
    }

    public List<IRoom> FindPath(IRoom start, IRoom goal)
    {
        if (start == null || goal == null || start == goal)
            return new List<IRoom>();

        if (!_adjacency.ContainsKey(start) || !_adjacency.ContainsKey(goal))
            return new List<IRoom>();

        var openSet = new HashSet<IRoom> { start };
        var cameFrom = new Dictionary<IRoom, IRoom>();
        var gScore = new Dictionary<IRoom, float> { [start] = 0 };
        var fScore = new Dictionary<IRoom, float> { [start] = Heuristic(start, goal) };

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

        return new List<IRoom>();
    }

    private float Heuristic(IRoom a, IRoom b)
    {
        return Microsoft.Xna.Framework.Vector3.Distance(a.Position, b.Position);
    }

    private List<IRoom> ReconstructPath(Dictionary<IRoom, IRoom> cameFrom, IRoom current)
    {
        var path = new List<IRoom> { current };

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }

        return path;
    }
}
