using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics.Pathfinding;
using ExploringGame.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.LevelControl;

public class WorldSegmentAnchorProcessor
{
    public void ProcessPlaceholders(IEnumerable<LevelData> loadedSegments, RoomGraph roomGraph, WaypointGraph waypointGraph, RoomLightingCalculator lightingCalculator)
    {
        // Find all PlaceHolderShape instances
        var placeholders = loadedSegments
            .SelectMany(ld => ld.WorldSegment.TraverseAllChildren())
            .OfType<PlaceholderShape>()
            .ToList();

        foreach (var placeholder in placeholders)
        {
            var realShape = placeholder.FindMatchingRealShape(loadedSegments);

            if (realShape == null)
            {
                var placeholderType = placeholder.GetType().GetGenericArguments()[0];
                var tag = (placeholder as Room)?.Tag;
                var tagInfo = string.IsNullOrEmpty(tag) ? "" : $" with tag '{tag}'";
                throw new InvalidOperationException(
                    $"No matching real shape found for PlaceholderShape<{placeholderType.Name}>{tagInfo}");
            }

            ValidatePlaceholderMatch(placeholder, realShape);
            ReplacePlaceholderWithRealShape(placeholder, realShape, loadedSegments, roomGraph, waypointGraph, lightingCalculator);
        }
    }
  
    private void ValidatePlaceholderMatch(Shape placeholder, Shape realShape)
    {
        const float tolerance = 0.001f;

        var positionMatch = Vector3.Distance(placeholder.Position, realShape.Position) < tolerance;
        var sizeMatch = Vector3.Distance(placeholder.Size, realShape.Size) < tolerance;

        if (!positionMatch || !sizeMatch)
        {
            var placeholderType = placeholder.GetType().GetGenericArguments()[0];
            var tag = (placeholder as Room)?.Tag;
            var tagInfo = string.IsNullOrEmpty(tag) ? "" : $" (tag: '{tag}')";

            throw new InvalidOperationException(
                $"PlaceholderShape<{placeholderType.Name}>{tagInfo} does not match real shape.\n" +
                $"Placeholder - Position: {placeholder.Position}, Size: {placeholder.Size}\n" +
                $"Real Shape  - Position: {realShape.Position}, Size: {realShape.Size}\n" +
                $"Update the placeholder's hard-coded position and size to match.");
        }
    }

    private void ReplacePlaceholderWithRealShape(PlaceholderShape placeholder, Room realShape, 
        IEnumerable<LevelData> loadedSegments, RoomGraph roomGraph, WaypointGraph waypointGraph, 
        RoomLightingCalculator lightingCalculator)
    {        
        if (placeholder.Parent != null)
            placeholder.Parent.RemoveChild(placeholder);

        roomGraph.ReplaceRoom(placeholder, realShape);
        ReplaceRoomInConnections(placeholder, realShape, loadedSegments);
        ReplaceRoomInWaypointGraph(placeholder, realShape, waypointGraph);
        ReplaceRoomInLightingCalculator(placeholder, realShape, lightingCalculator);
    }

    private void ReplaceRoomInConnections(Room oldRoom, Room newRoom, IEnumerable<LevelData> loadedSegments)
    {
        // Get all rooms in all loaded segments
        var allRooms = loadedSegments
            .SelectMany(ld => ld.WorldSegment.TraverseAllChildren().OfType<Room>())
            .ToList();

        // Update all room connections
        foreach (var room in allRooms.Distinct())
        {
            room.ReplaceRoomInConnections(oldRoom, newRoom);
        }
    }

    private void ReplaceRoomInWaypointGraph(Room placeholderRoom, Room realRoom, WaypointGraph waypointGraph)
    {
        waypointGraph.ReplaceRoom(placeholderRoom, realRoom);
    }

    private void ReplaceRoomInLightingCalculator(Room placeholderRoom, Room realRoom, RoomLightingCalculator lightingCalculator)
    {
        lightingCalculator.ReplaceRoom(placeholderRoom, realRoom);
    }
}
