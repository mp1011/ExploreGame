using ExploringGame.GameDebug;
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

            if (realShape != null)
            {
                ValidatePlaceholderMatch(placeholder, realShape);
                ReplacePlaceholderWithRealShape(placeholder, realShape, loadedSegments, roomGraph, waypointGraph, lightingCalculator);
            }
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

            var errorMessage = 
                $"PlaceholderShape<{placeholderType.Name}>{tagInfo} does not match real shape.\n" +
                $"Placeholder - Position: new Vector3({placeholder.Position.X}f, {placeholder.Position.Y}f, {placeholder.Position.Z}f), Size: new Vector3({placeholder.Size.X}f, {placeholder.Size.Y}f, {placeholder.Size.Z}f)\n" +
                $"Real Shape  - position: new Vector3({realShape.Position.X}f, {realShape.Position.Y}f, {realShape.Position.Z}f), size: new Vector3({realShape.Size.X}f, {realShape.Size.Y}f, {realShape.Size.Z}f)\n" +
                $"Update the placeholder's hard-coded position and size to match.";

            if (Debug.PlaceholderStrictMode)
            {
                throw new InvalidOperationException(errorMessage);
            }
            else
            {
                // Just log a warning but don't crash
                System.Diagnostics.Debug.WriteLine($"WARNING: {errorMessage}");
            }
        }
    }

    private void ReplacePlaceholderWithRealShape(PlaceholderShape placeholder, Shape realShape, 
        IEnumerable<LevelData> loadedSegments, RoomGraph roomGraph, WaypointGraph waypointGraph, 
        RoomLightingCalculator lightingCalculator)
    {        
        if (placeholder.Parent != null)
            placeholder.Parent.RemoveChild(placeholder);

        if (realShape is Room roomShape)
        {
            roomGraph.ReplaceRoom(placeholder, roomShape);
            ReplaceRoomInConnections(placeholder, roomShape, loadedSegments);
            ReplaceRoomInWaypointGraph(placeholder, roomShape, waypointGraph);
            ReplaceRoomInLightingCalculator(placeholder, roomShape, lightingCalculator);
        }
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
