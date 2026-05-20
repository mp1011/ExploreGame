using ExploringGame.GeometryBuilder.Shapes;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Logics.Pathfinding;

/// <summary>
/// Associates arbitrary data of type T with rooms in a RoomGraph.
/// Relies on RoomGraph for all connectivity/pathfinding operations.
/// </summary>
public class AnnotatedGraph<T>
    where T:IWithRoom
{
    private readonly RoomGraph _roomGraph;
    private readonly Dictionary<Room, T> _annotations = new();

    public AnnotatedGraph(RoomGraph roomGraph)
    {
        _roomGraph = roomGraph;
    }

    public bool HasRoomGraph(RoomGraph g) => _roomGraph == g;

    public void Add(Room room, T annotation)
    {
        _annotations[room] = annotation;
    }

    public T Get(Room room)
    {
        return _annotations.TryGetValue(room, out var annotation) ? annotation : default;
    }

    public bool TryGet(Room room, out T annotation)
    {
        return _annotations.TryGetValue(room, out annotation);
    }

    public IEnumerable<T> GetNeighborAnnotations(Room room)
    {
        return _roomGraph.GetNeighbors(room)
            .Where(neighbor => _annotations.ContainsKey(neighbor))
            .Select(neighbor => _annotations[neighbor]);
    }

    public IEnumerable<Room> GetAllRooms()
    {
        return _annotations.Keys;
    }

    public IEnumerable<T> GetAllAnnotations()
    {
        return _annotations.Values;
    }

    public List<T> FindPath(Room start, Room goal)
    {
        var roomPath = _roomGraph.FindPath(start, goal);
        return roomPath
            .Where(room => _annotations.ContainsKey(room))
            .Select(room => _annotations[room])
            .ToList();
    }

    /// <summary>
    /// Replaces a room key in the annotations dictionary with a new room key,
    /// preserving the associated annotation value.
    /// </summary>
    public void ReplaceKey(Room oldRoom, Room newRoom)
    {
        if (_annotations.TryGetValue(oldRoom, out var annotation))
        {
            _annotations.Remove(oldRoom);
            _annotations[newRoom] = annotation;            
        }

        foreach(var ano in _annotations.Values)
        {
            if (ano.Room == oldRoom)
                ano.Room = newRoom;
        }
    }
}
