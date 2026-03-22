using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.LevelControl;

public class WorldSegmentAnchorProcessor
{
    public void ApplyAnchorPositioning(WorldSegment worldSegment, IEnumerable<LevelData> loadedSegments)
    {
        if (!worldSegment.AnchorShapeTypes.Any())
            return;

        Vector3? translation = null;
        Shape existingAnchor = null;
        Shape newAnchor = null;

        // Check each anchor type to see if it's already loaded
        foreach (var anchorType in worldSegment.AnchorShapeTypes)
        {
            var newAnchorShape = FindShapeByType(worldSegment, anchorType);
            if (newAnchorShape == null)
                continue;

            var existingAnchorShape = FindLoadedShapeByType(anchorType, loadedSegments);
            if (existingAnchorShape == null)
                continue;

            // Found a matching anchor - calculate translation
            var delta = existingAnchorShape.Position - newAnchorShape.Position;

            // Validate anchor sizes match
            const float sizeTolerance = 0.001f;
            if (Vector3.Distance(existingAnchorShape.Size, newAnchorShape.Size) > sizeTolerance)
            {
                throw new InvalidOperationException(
                    $"Anchor shape size mismatch for type {anchorType.Name}. " +
                    $"Existing size: {existingAnchorShape.Size}, New size: {newAnchorShape.Size}");
            }

            // Check for conflicting translations
            const float translationTolerance = 0.001f;
            if (translation.HasValue && Vector3.Distance(translation.Value, delta) > translationTolerance)
            {
                throw new InvalidOperationException(
                    $"Conflicting anchor translations detected. " +
                    $"Previous translation: {translation.Value}, New translation: {delta}");
            }

            translation = delta;
            existingAnchor = existingAnchorShape;
            newAnchor = newAnchorShape;
        }

        // If we found a matching anchor, translate all shapes in the segment
        if (translation.HasValue && existingAnchor != null && newAnchor != null)
        {
            TranslateSegmentShapes(worldSegment, translation.Value);
            ReplaceAnchorShapeReferences(newAnchor, existingAnchor, loadedSegments);
        }
    }

    private Shape FindShapeByType(WorldSegment worldSegment, Type shapeType)
    {
        return worldSegment.TraverseAllChildren().FirstOrDefault(s => s.GetType() == shapeType);
    }

    private Shape FindLoadedShapeByType(Type shapeType, IEnumerable<LevelData> loadedSegments)
    {
        foreach (var levelData in loadedSegments)
        {
            var shape = levelData.WorldSegment.TraverseAllChildren()
                .FirstOrDefault(s => s.GetType() == shapeType);
            if (shape != null)
                return shape;
        }
        return null;
    }

    private void TranslateSegmentShapes(WorldSegment worldSegment, Vector3 translation)
    {
        foreach (var shape in worldSegment.TraverseAllChildren())
        {
            shape.Position += translation;
        }
    }

    private void ReplaceAnchorShapeReferences(Shape newAnchor, Shape existingAnchor, IEnumerable<LevelData> loadedSegments)
    {
        // Replace room connections that reference the new anchor
        if (newAnchor is Room newAnchorRoom && existingAnchor is Room existingAnchorRoom)
        {
            ReplaceRoomInConnections(newAnchorRoom, existingAnchorRoom, loadedSegments);
        }

        // Transfer children from new anchor to existing anchor
        foreach (var child in newAnchor.Children.ToList())
        {
            existingAnchor.AddChild(child);
        }

        // Remove the new anchor from its parent
        if (newAnchor.Parent != null)
        {
            RemoveChildFromParent(newAnchor);
        }
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

        // Also include rooms from the segment being loaded (not yet in LoadedSegments)
        allRooms.AddRange(oldRoom.WorldSegment.TraverseAllChildren().OfType<Room>());

        // Update all room connections
        foreach (var room in allRooms.Distinct())
        {
            room.ReplaceRoomInConnections(oldRoom, newRoom);
        }
    }
}
