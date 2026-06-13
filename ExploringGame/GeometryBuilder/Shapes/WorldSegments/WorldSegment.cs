using ExploringGame.Logics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.GeometryBuilder.Shapes.WorldSegments;

public class WorldSegment : Shape, ILightingGroup
{
    public override ViewFrom ViewFrom => ViewFrom.None;

    public virtual Vector3 DefaultPlayerStart { get; }

    public virtual SkyboxShape Skybox => null;

    public IEnumerable<RoomConnection> RoomConnections => Array.Empty<RoomConnection>();

    WorldSegment IRoom.WorldSegment => this;

    public WorldSegment(params Shape[] contents)
    {
        foreach (var item in contents)
            AddChild(item);
    }

    /// <summary>
    /// Phase 2: Position children after all world segments have created their shapes.
    /// Override this to implement positioning logic that depends on shapes from other world segments.
    /// </summary>
    public virtual void PositionChildren(IEnumerable<WorldSegment> loadedSegments)
    {
        // Default implementation: do nothing
        // Derived classes can override to implement cross-segment positioning
    }

    /// <summary>
    /// Helper method to find a shape by tag across all loaded segments.
    /// Throws if shape is not found (fail-fast on missing dependencies).
    /// </summary>
    protected T FindShapeByTag<T>(IEnumerable<WorldSegment> loadedSegments, string tag) where T : Shape
    {
        var shape = loadedSegments
            .SelectMany(ws => ws.TraverseAllChildren())
            .OfType<T>()
            .FirstOrDefault(s => s.Tag == tag);

        if (shape == null)
            throw new InvalidOperationException($"Required shape with tag '{tag}' of type {typeof(T).Name} not found in loaded segments");

        return shape;
    }

    /// <summary>
    /// Helper method to find a single shape of a given type across all loaded segments.
    /// Throws if shape is not found or if multiple matches exist.
    /// </summary>
    protected T FindShape<T>(IEnumerable<WorldSegment> loadedSegments) where T : Shape
    {
        var shapes = loadedSegments
            .SelectMany(ws => ws.TraverseAllChildren())
            .OfType<T>()
            .ToList();

        if (shapes.Count == 0)
            throw new InvalidOperationException($"Required shape of type {typeof(T).Name} not found in loaded segments");

        if (shapes.Count > 1)
            throw new InvalidOperationException($"Multiple shapes of type {typeof(T).Name} found in loaded segments. Use FindShapeByTag instead.");

        return shapes[0];
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return Array.Empty<Triangle>();
    }
}
