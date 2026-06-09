using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.LevelControl;
using ExploringGame.Logics.Pathfinding;
using Microsoft.Xna.Framework;

namespace ExploringGame.Services;

/// <summary>
/// Service for tracking and updating which room a PlaceableShape is currently in.
/// Optimized to check the current room first before performing an expensive search.
/// </summary>
public class EntityRoomFinder
{
    private readonly LoadedLevelData _loadedLevelData;

    public EntityRoomFinder(LoadedLevelData loadedLevelData)
    {
        _loadedLevelData = loadedLevelData;
    }

    /// <summary>
    /// Updates the Room property for a PlaceableShape based on its current position.
    /// Optimized to check current room first before searching.
    /// </summary>
    public void UpdateRoom(PlaceableShape shape)
    {
        // Fast path: Check if still in current room
        if (shape.Room != null && shape.Room.ContainsPoint(shape.LocalPosition))
            return;

        // Slow path: Find which room contains the position
        var newRoom = FindRoomContainingPosition(shape.LocalPosition);
        shape.Room = newRoom?.LightingGroup as Room ?? newRoom;
    }

    private IRoom FindRoomContainingPosition(Vector3 position)
    {
        var roomGraph = _loadedLevelData.RoomGraph;
        if (roomGraph == null)
            return null;

        // Check each room to see if it contains the position
        foreach (var room in roomGraph.GetAllRooms())
        {
            if (room.ContainsPoint(position))
                return room;
        }

        // If no room contains the point, find the nearest room
        IRoom nearestRoom = null;
        float nearestDistance = float.MaxValue;

        foreach (var room in roomGraph.GetAllRooms())
        {
            var distance = Vector3.DistanceSquared(position, room.LocalPosition);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestRoom = room;
            }
        }

        return nearestRoom;
    }

    public IRoom FindRoom(Vector3 position)
    {
        return FindRoomContainingPosition(position);
    }
}
