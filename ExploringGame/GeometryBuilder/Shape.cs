using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.GeometryBuilder;

public enum ViewFrom
{
    Inside,
    Outside
}

public record Rotation(float Pitch, float Yaw, float Roll);
public record Triangle(Vector3 A, Vector3 B, Vector3 C, Color Color)
{
    public Triangle Invert() => new Triangle(C, B, A, Color);

    public IEnumerable<Vector3> Vertices
    {
        get
        {
            yield return A;
            yield return B;
            yield return C;
        }
    }
}

public abstract class Shape
{
    public Shape? Parent { get; private set; }

    private List<Shape> _children = new();

    public IEnumerable<Shape> Children => _children.AsReadOnly();

    public abstract ViewFrom ViewFrom { get; }
    public Vector3 Position { get; set; }
    public Vector3 Size { get; set; }

    public Color MainColor { get; set; }

    public Dictionary<Side, Color> SideColors { get; } = new Dictionary<Side, Color>();

    public float X
    {
        get => Position.X; set => Position = new Vector3(value, Position.Y, Position.Z);
    }
    public float Y
    {
        get => Position.Y; set => Position = new Vector3(Position.X, value, Position.Z);
    }
    public float Z
    {
        get => Position.Z; set => Position = new Vector3(Position.X, Position.Y, value);
    }
    public float Width
    {
        get => Size.X; set => Size = new Vector3(value, Size.Y, Size.Z);
    }
    public float Height
    {
        get => Size.Y; set => Size = new Vector3(Size.X, value, Size.Z);
    }
    public float Depth
    {
        get => Size.Z; set => Size = new Vector3(Size.X, Size.Y, value);
    }

    /// <summary>
    /// Sets the bottom Y coordinate while preserving height
    /// </summary>
    public float BottomAnchored
    {
        get => Position.Y - Size.Y / 2f;
        set
        {
            var currentBottom = this.BottomAnchored;
            var delta = value - currentBottom;
            Y += delta;
        }
    }

    public float GetSide(Side side)
    {
        return side switch
        {
            Side.North => Position.Z + Size.Z / 2f,
            Side.South => Position.Z - Size.Z / 2f,
            Side.West => Position.X - Size.X / 2f,
            Side.East => Position.X + Size.X / 2f,
            _ => throw new ArgumentException("Only singular sides can be used")
        };
    }

    public void SetSide(Side side, float value)
    {
        switch(side)
        {
            case Side.North:
                Z = value - Size.Z / 2f;
                return;
            case Side.South:
                Z = value + Size.Z / 2f;
                return;
            case Side.West:
                X = value + Size.X / 2f;
                return;
            case Side.East:
                X = value - Size.X / 2f;
                return;
            default:
                throw new ArgumentException("Only singular sides can be used");
        }
    }

    public void AddChild(Shape child)
    {
        child.Parent = this;
        _children.Add(child);
    }

    public Rotation Rotation { get; set; }

    public Triangle[] Build(int quality)
    {
        if (quality == 0)
            return Array.Empty<Triangle>();
        else if (quality == 1)
            return BuildCuboid();
        else
        {
            var thisTriangles = BuildInternal(quality);
            var childrenTriangles = Children.SelectMany(p => p.Build(quality - 1)).ToArray();
            return thisTriangles.Union(childrenTriangles).ToArray();
        }
    }

    public Color ColorForSide(Side side)
    {
        Color c;
        if (SideColors.TryGetValue(side, out c))
            return c;
        else
            return MainColor;
    }

    /// <summary>
    /// Generates the triangles of the bounding volume of this shape
    /// </summary>
    /// <returns></returns>
    protected Triangle[] BuildCuboid()
    {
        // Calculate the 8 corners of the cuboid
        var min = Position - Size / 2f;
        var max = Position + Size / 2f;

        Vector3[] corners = new Vector3[8];
        // Bottom face (Y = min.Y)
        corners[0] = new Vector3(min.X, min.Y, min.Z); 
        corners[1] = new Vector3(max.X, min.Y, min.Z);
        corners[2] = new Vector3(max.X, min.Y, max.Z);
        corners[3] = new Vector3(min.X, min.Y, max.Z);
        // Top face (Y = max.Y)
        corners[4] = new Vector3(min.X, max.Y, min.Z);
        corners[5] = new Vector3(max.X, max.Y, min.Z);
        corners[6] = new Vector3(max.X, max.Y, max.Z);
        corners[7] = new Vector3(min.X, max.Y, max.Z);

  
        List<Triangle> triangles = new();

        //floor
        triangles.Add(new Triangle(corners[0], corners[1], corners[2], ColorForSide(Side.Bottom)));
        triangles.Add(new Triangle(corners[2], corners[3], corners[0], ColorForSide(Side.Bottom)));

        //ceiling
        triangles.Add(new Triangle(corners[6], corners[5], corners[4], ColorForSide(Side.Top)));
        triangles.Add(new Triangle(corners[4], corners[7], corners[6], ColorForSide(Side.Top)));

        // wall(min z)
        triangles.Add(new Triangle(corners[5], corners[1], corners[0], ColorForSide(Side.South)));
        triangles.Add(new Triangle(corners[0], corners[4], corners[5], ColorForSide(Side.South)));

        //wall (max z)
        triangles.Add(new Triangle(corners[6], corners[3], corners[2], ColorForSide(Side.North)));
        triangles.Add(new Triangle(corners[6], corners[7], corners[3], ColorForSide(Side.North)));

        //wall (min x)
        triangles.Add(new Triangle(corners[0], corners[3], corners[7], ColorForSide(Side.West)));
        triangles.Add(new Triangle(corners[7], corners[4], corners[0], ColorForSide(Side.West)));

        //wall (max x)
        triangles.Add(new Triangle(corners[5], corners[2], corners[1], ColorForSide(Side.East)));
        triangles.Add(new Triangle(corners[5], corners[6], corners[2], ColorForSide(Side.East)));
        
        if(ViewFrom == ViewFrom.Outside)
        {
            triangles = triangles.Select(p => p.Invert()).ToList();
        }
            
        return triangles.ToArray();
    }

    protected abstract Triangle[] BuildInternal(int quality);
}

public class SimpleRoom : Shape
{
    public override ViewFrom ViewFrom => ViewFrom.Inside;

    protected override Triangle[] BuildInternal(int quality)
    {
        return BuildCuboid();
    }
}

public class Box : Shape
{
    public override ViewFrom ViewFrom => ViewFrom.Outside;

    protected override Triangle[] BuildInternal(int quality)
    {
        return BuildCuboid();
    }
}