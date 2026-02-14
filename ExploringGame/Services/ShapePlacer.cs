using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.Decals;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace ExploringGame.Services;

public static partial class ShapeExtensions
{
    public static ShapePlacer Place(this Shape shape)
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
    protected Shape _shape;
    
    public ShapePlacer(Shape shape)
    {
        _shape = shape;
    }

    public ShapePlacer OnFloor(Shape other = null)
    {
        _shape.BottomAnchored = (other ?? _shape.Parent).BottomAnchored;
        return this;
    }

    public ShapePlacer OnSideInner(Side side, Shape other = null)
    {
        other = other ?? _shape.Parent;
        foreach(var s in side.Decompose())
        {
            _shape.SetSide(s, other.GetSide(s));
        }
        return this;
    }

    public ShapePlacer OnSideOuter(Side side, Shape other = null, float offset = 0f)
    {
        other = other ?? _shape.Parent;
        foreach (var s in side.Decompose())
        {
            _shape.SetSide(s.Opposite(), other.GetSide(s) + offset);
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

        _shape.SetSide(side, _shape.Parent.GetSide(side) + amount);
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
        
        // Get quad bounds in world space
        var quadMinX = quad.Vertices.Min(v => v.X);
        var quadMaxX = quad.Vertices.Max(v => v.X);
        var quadMinY = quad.Vertices.Min(v => v.Y);
        var quadMaxY = quad.Vertices.Max(v => v.Y);
        
        // Get decal dimensions
        float decalWidth = _wallDecal.Width;
        float decalHeight = _wallDecal.Height;
        
        // Calculate valid placement area (with padding from edges)
        float xMin = quadMinX + (decalWidth / 2f) + padding;
        float xMax = quadMaxX - (decalWidth / 2f) - padding;
        float yMin = quadMinY + (decalHeight / 2f) + padding;
        float yMax = quadMaxY - (decalHeight / 2f) - padding;

        // Random position within quad
        float decalCenterX = (float)(xMin + random.NextDouble() * (xMax - xMin));
        float decalCenterY = (float)(yMin + random.NextDouble() * (yMax - yMin));

        // Convert to wall-relative coordinates (relative to room's west and bottom sides)
        float wallLeft = decalCenterX - (decalWidth / 2f) - quad.Room.GetSide(Side.West);
        float wallRight = decalCenterX + (decalWidth / 2f) - quad.Room.GetSide(Side.West);
        float wallBottom = decalCenterY - (decalHeight / 2f) - quad.Room.GetSide(Side.Bottom);
        float wallTop = decalCenterY + (decalHeight / 2f) - quad.Room.GetSide(Side.Bottom);

        var placement = new Placement2D(wallLeft, wallTop, wallRight, wallBottom);
        _wallDecal.Placement = placement;
        _wallDecal.WallSide = quad.Side;
        
        // Recalculate transform with new placement
        var calculateTransform = _wallDecal.GetType().GetMethod("CalculateTransform", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        calculateTransform?.Invoke(_wallDecal, new object[] { quad.Room });

        return this;
    }
}
