using ExploringGame.GeometryBuilder.Shapes;

namespace ExploringGame.Logics.Pathfinding;

/// <summary>
/// Placeholder for future lighting work.
/// Will store metadata about the connection between two rooms,
/// such as connection size, door references, etc.
/// </summary>
public class RoomGraphEdge
{
    public Room Room1 { get; }
    public Room Room2 { get; }

    // TODO: Add connection size, door reference, and other metadata for lighting calculations

    public RoomGraphEdge(Room room1, Room room2)
    {
        Room1 = room1;
        Room2 = room2;
    }

    public Room GetOtherRoom(Room room)
    {
        if (room == Room1) return Room2;
        if (room == Room2) return Room1;
        return null;
    }
}
