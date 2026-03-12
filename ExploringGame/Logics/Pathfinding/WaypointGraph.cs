using ExploringGame.GameDebug;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Logics.Pathfinding;


public class WaypointGraph
{
    private AnnotatedGraph<Waypoint> _annotatedGraph;

    public WaypointGraph(RoomGraph roomGraph)
    {
        _annotatedGraph = new AnnotatedGraph<Waypoint>(roomGraph);
    }

    public void AddRoomAndWaypoint(Room room, WorldSegment segment)
    {
        if (_annotatedGraph.Get(room) == null)
        {
            var waypoint = new Waypoint(room);
            _annotatedGraph.Add(room, waypoint);
            segment.AddChild(waypoint);
        }
    }

    public void AddRoomsAndWaypoints(IEnumerable<Room> rooms, WorldSegment segment)
    {
        foreach (var room in rooms)
        {
            AddRoomAndWaypoint(room, segment);
        }
    }


    public Waypoint GetWaypointForRoom(Room room)
    {
        return _annotatedGraph.Get(room);
    }


    public Room GetRoomContaining(Vector3 position)
    {
        foreach (var room in _annotatedGraph.GetAllRooms())
        {
            if (room.ContainsPoint(position))
                return room;
        }
        return null;
    }

    public Waypoint FindNearestWaypoint(Vector3 position)
    {
        var containingRoom = GetRoomContaining(position);
        if (containingRoom != null && _annotatedGraph.TryGet(containingRoom, out var roomWaypoint))
        {
            return roomWaypoint;
        }

        Waypoint nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (var waypoint in _annotatedGraph.GetAllAnnotations())
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

        return _annotatedGraph.FindPath(start.Room, goal.Room);
    }
}
