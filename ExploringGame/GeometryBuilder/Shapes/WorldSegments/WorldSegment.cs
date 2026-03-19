using ExploringGame.Entities;
using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Logics.Pathfinding;
using ExploringGame.Logics.ShapeControllers;
using Microsoft.Xna.Framework;
using System;

namespace ExploringGame.GeometryBuilder.Shapes.WorldSegments;

public class WorldSegment : Shape
{
    public override ViewFrom ViewFrom => ViewFrom.None;

    public virtual Vector3 DefaultPlayerStart { get; }


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
