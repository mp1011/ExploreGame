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
    public Shape? Parent { get; set; }
    public List<Shape> Children { get; } = new();

    public abstract ViewFrom ViewFrom { get; }
    public Vector3 Position { get; set; }
    public Vector3 Size { get; set; }

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
        corners[0] = new Vector3(min.X, min.Y, min.Z); // 0: left, bottom, back
        corners[1] = new Vector3(max.X, min.Y, min.Z); // 1: right, bottom, back
        corners[2] = new Vector3(max.X, min.Y, max.Z); // 2: right, bottom, front
        corners[3] = new Vector3(min.X, min.Y, max.Z); // 3: left, bottom, front
        // Top face (Y = max.Y)
        corners[4] = new Vector3(min.X, max.Y, min.Z); // 4: left, top, back
        corners[5] = new Vector3(max.X, max.Y, min.Z); // 5: right, top, back
        corners[6] = new Vector3(max.X, max.Y, max.Z); // 6: right, top, front
        corners[7] = new Vector3(min.X, max.Y, max.Z); // 7: left, top, front

  
        List<Triangle> triangles = new();

        //floor
        triangles.Add(new Triangle(corners[0], corners[1], corners[2], Color.LightGreen));
        triangles.Add(new Triangle(corners[2], corners[3], corners[0], Color.LightGreen));

        //ceiling
        triangles.Add(new Triangle(corners[6], corners[5], corners[4], Color.White));
        triangles.Add(new Triangle(corners[4], corners[7], corners[6], Color.White));

        // wall(min z)
        triangles.Add(new Triangle(corners[5], corners[1], corners[0], Color.Red));
        triangles.Add(new Triangle(corners[0], corners[4], corners[5], Color.Red));

        //wall (max z)
        triangles.Add(new Triangle(corners[6], corners[3], corners[2], Color.DarkRed));
        triangles.Add(new Triangle(corners[6], corners[7], corners[3], Color.DarkRed));

        //wall (min x)
        triangles.Add(new Triangle(corners[0], corners[3], corners[7], Color.Orange));
        triangles.Add(new Triangle(corners[7], corners[4], corners[0], Color.Orange));

        //wall (max x)
        triangles.Add(new Triangle(corners[5], corners[2], corners[1], Color.OrangeRed));
        triangles.Add(new Triangle(corners[5], corners[6], corners[2], Color.OrangeRed));
        
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