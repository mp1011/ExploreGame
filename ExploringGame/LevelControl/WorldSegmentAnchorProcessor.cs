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
            .SelectMany(ld => ld.WorldSegment.TraverseAllChildren()
                .Where(s => s.GetType().IsGenericType && 
                            s.GetType().GetGenericTypeDefinition() == typeof(PlaceholderShape<>)))
            .ToList();

        foreach (var placeholder in placeholders)
        {
            var realShape = FindMatchingRealShape(placeholder, loadedSegments);

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

    private Shape FindMatchingRealShape(Shape placeholder, IEnumerable<LevelData> loadedSegments)
    {
        var placeholderType = placeholder.GetType();
        var targetType = placeholderType.GetGenericArguments()[0];
        var placeholderTag = (placeholder as Room)?.Tag;

        foreach (var levelData in loadedSegments)
        {
            foreach (var shape in levelData.WorldSegment.TraverseAllChildren())
            {
                // Skip other placeholders
                if (shape.GetType().IsGenericType && 
                    shape.GetType().GetGenericTypeDefinition() == typeof(PlaceholderShape<>))
                    continue;

                // Check if type matches
                if (shape.GetType() == targetType)
                {
                    // If placeholder has a tag, match on tag
                    if (!string.IsNullOrEmpty(placeholderTag))
                    {
                        if (shape is Room room && room.Tag == placeholderTag)
                            return shape;
                    }
                    else
                    {
                        // No tag, just match on type
                        return shape;
                    }
                }
            }
        }

        return null;
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

    private void ReplacePlaceholderWithRealShape(Shape placeholder, Shape realShape, 
        IEnumerable<LevelData> loadedSegments, RoomGraph roomGraph, WaypointGraph waypointGraph, 
        RoomLightingCalculator lightingCalculator)
    {
        // Replace in parent/child relations
        if (placeholder.Parent != null)
        {
            RemoveChildFromParent(placeholder);
        }

        // Replace in RoomConnections
        if (placeholder is Room placeholderRoom && realShape is Room realRoom)
        {
            ReplaceRoomInConnections(placeholderRoom, realRoom, loadedSegments);
        }

        // Replace in RoomGraph
        if (placeholder is Room pRoom && realShape is Room rRoom)
        {
            ReplaceRoomInGraph(pRoom, rRoom, roomGraph);
        }

        // WaypointGraph and LightingCalculator replacements would go here
        // These may need additional methods depending on their internal structure
    }

    private void RemoveChildFromParent(Shape child)
    {
        if (child.Parent == null)
            return;

        var parent = child.Parent;
        var childrenField = parent.GetType()
            .GetField("_children", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (childrenField?.GetValue(parent) is List<Shape> children)
        {
            children.Remove(child);
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

    private void ReplaceRoomInGraph(Room placeholderRoom, Room realRoom, RoomGraph roomGraph)
    {
        // This will need to be implemented based on RoomGraph's API
        // For now, we might need to:
        // 1. Remove placeholderRoom from graph
        // 2. Add realRoom if not already present
        // 3. Update connections that pointed to placeholderRoom

        // Placeholder for implementation
        // roomGraph.ReplaceRoom(placeholderRoom, realRoom);
    }
}
