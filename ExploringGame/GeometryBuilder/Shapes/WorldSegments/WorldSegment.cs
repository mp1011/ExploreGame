using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.GeometryBuilder.Shapes.WorldSegments;

public class WorldSegment : Shape
{
    public override ViewFrom ViewFrom => ViewFrom.None;

    public virtual SkyboxShape Skybox => null;

    public virtual Vector3 DefaultPlayerStart => TraverseAllChildren().OfType<Room>().FirstOrDefault().Position;

    public virtual IReadOnlyList<WorldSegmentTransition> Transitions { get; } = Array.Empty<WorldSegmentTransition>();

    public WorldSegment(params Shape[] contents)
    {
        foreach (var item in contents)
            AddChild(item);
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return Array.Empty<Triangle>();
    }

    public override Matrix GetWorldMatrix()
    {
        return Matrix.CreateTranslation(Vector3.Zero);
    }
}
