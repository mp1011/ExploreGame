using ExploringGame.Entities;
using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.Decals;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace ExploringGame.Services;

public static partial class ShapeExtensions
{
    public static ShapePlacer Place(this IShape shape)
    {
        return new ShapePlacer(shape);
    }

    public static WallDecalShapePlacer Place(this WallDecal wallDecal)
    {
        return new WallDecalShapePlacer(wallDecal);
    }
}

    public class ShapePlacer
    {
        protected IShape _shape;

        public IShape Shape() => _shape;
        
        public ShapePlacer(IShape shape)
        {
            _shape = shape;
        }

    public ShapePlacer AtParent()
    {
        _shape.WorldPosition = _shape.Parent.WorldPosition;
        return this;
    }

    public ShapePlacer At(Shape other)
    {
        _shape.WorldPosition = other.WorldPosition;
        return this;
    }

    public ShapePlacer AtStandardSwitchHeight() => AtEyeLevel(_shape.Parent, offset: -Measure.Feet(1));

    public ShapePlacer AtEyeLevel(IShape container, float offset)
    {
        _shape.WorldPosition = _shape.WorldPosition.SetY(container.GetWorldSide(Side.Bottom) + Player.EyeHeight + offset);
        return this;
    }

    public ShapePlacer OnFloor(IShape other = null)
    {
        var target = other ?? _shape.Parent;
        _shape.SetWorldSide(Side.Bottom, target.GetWorldSide(Side.Bottom));
        return this;
    }

    public ShapePlacer OnSideInner(Side side, IShape other = null, float offset = 0f)
    {
        other = other ?? _shape.Parent;
        foreach(var s in side.Decompose())
        {
            _shape.SetWorldSide(s, other.GetWorldSide(s) + offset);
        }
        return this;
    }

    public ShapePlacer AlignSideWith(Side side, IShape other, float offset = 0f)
    {
        _shape.SetWorldSideUnanchored(side, other.GetWorldSide(side) + offset);
        return this;
    }

    public ShapePlacer OnSideOuter(Side side, IShape other = null, float offset = 0f)
    {
        other = other ?? _shape.Parent;
        foreach (var s in side.Decompose())
        {
            _shape.SetWorldSide(s.Opposite(), other.GetWorldSide(s) + offset);
        }
        return this;
    }

    public ShapePlacer FromNorth(float amount) => FromSide(Side.North, amount);
    public ShapePlacer FromSouth(float amount) => FromSide(Side.South, amount);
    public ShapePlacer FromEast(float amount) => FromSide(Side.East, amount);
    public ShapePlacer FromWest(float amount) => FromSide(Side.West, amount);

    public ShapePlacer FromSide(Side side, float amount)
    {
        if(side == Side.South || side == Side.East || side == Side.Top)
            amount = -amount;

        _shape.SetWorldSide(side, _shape.Parent.GetWorldSide(side) + amount);
        return this;
    }
}

public class WallDecalShapePlacer : ShapePlacer
{
    private WallDecal _wallDecal;

    public WallDecalShapePlacer(WallDecal wallDecal) : base(wallDecal)
    {
        _wallDecal = wallDecal;
    }

    /// <summary>
    /// Places a WallDecal randomly within a wall quad, avoiding gaps
    /// </summary>
    public WallDecalShapePlacer OnQuad(WallQuad quad, Random random = null, float padding = 0.05f)
    {
        random ??= new Random();
        
        // Get the U and V axes for this wall orientation
        var (axisU, axisV) = quad.Side.GetAxisUV();
        
        // Get quad bounds in world space along the U and V axes
        var quadMinU = quad.Vertices.Min(v => v.AxisValue(axisU));
        var quadMaxU = quad.Vertices.Max(v => v.AxisValue(axisU));
        var quadMinV = quad.Vertices.Min(v => v.AxisValue(axisV));
        var quadMaxV = quad.Vertices.Max(v => v.AxisValue(axisV));
        
        // Get decal dimensions
        float decalWidth = _wallDecal.Width;
        float decalHeight = _wallDecal.Height;
        
        // Calculate valid placement area (with padding from edges)
        float uMin = quadMinU + (decalWidth / 2f) + padding;
        float uMax = quadMaxU - (decalWidth / 2f) - padding;
        float vMin = quadMinV + (decalHeight / 2f) + padding;
        float vMax = quadMaxV - (decalHeight / 2f) - padding;

        // Random position within quad along U and V axes (world space)
        float decalWorldU = (float)(uMin + random.NextDouble() * (uMax - uMin));
        float decalWorldV = (float)(vMin + random.NextDouble() * (vMax - vMin));

        var decalWorldPosition = new Vector2(decalWorldU, decalWorldV);
        var wallCenter = new Vector2(quad.Room.LocalPosition.AxisValue(axisU), quad.Room.LocalPosition.AxisValue(axisV));
       
        _wallDecal.WallSide = quad.Side;
        _wallDecal.CenterUV = decalWorldPosition - wallCenter;

        return this;
    }
}
