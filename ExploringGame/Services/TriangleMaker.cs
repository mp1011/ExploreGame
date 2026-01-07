using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace ExploringGame.Services;

public static class TriangleMaker
{

    /// <summary>
    /// Generates the triangles of the bounding volume of this shape
    /// </summary>
    /// <returns></returns>
    public static Triangle[] BuildCuboid(Shape shape)
    {
        // Calculate the 8 corners of the cuboid
        var min = shape.Position - shape.Size / 2f;
        var max = shape.Position + shape.Size / 2f;

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
        triangles.Add(new Triangle(corners[0], corners[1], corners[2], shape.TextureInfoForSide(Side.Bottom), Side.Bottom));
        triangles.Add(new Triangle(corners[2], corners[3], corners[0], shape.TextureInfoForSide(Side.Bottom), Side.Bottom));

        //ceiling
        triangles.Add(new Triangle(corners[6], corners[5], corners[4], shape.TextureInfoForSide(Side.Top), Side.Top));
        triangles.Add(new Triangle(corners[4], corners[7], corners[6], shape.TextureInfoForSide(Side.Top), Side.Top));

        // wall(min z)
        triangles.Add(new Triangle(corners[5], corners[1], corners[0], shape.TextureInfoForSide(Side.North), Side.North));
        triangles.Add(new Triangle(corners[0], corners[4], corners[5], shape.TextureInfoForSide(Side.North), Side.North));

        //wall (max z)
        triangles.Add(new Triangle(corners[6], corners[3], corners[2], shape.TextureInfoForSide(Side.South), Side.South));
        triangles.Add(new Triangle(corners[6], corners[7], corners[3], shape.TextureInfoForSide(Side.South), Side.South));

        //wall (min x)
        triangles.Add(new Triangle(corners[0], corners[3], corners[7], shape.TextureInfoForSide(Side.West), Side.West));
        triangles.Add(new Triangle(corners[7], corners[4], corners[0], shape.TextureInfoForSide(Side.West), Side.West));

        //wall (max x)
        triangles.Add(new Triangle(corners[5], corners[2], corners[1], shape.TextureInfoForSide(Side.East), Side.East));
        triangles.Add(new Triangle(corners[5], corners[6], corners[2], shape.TextureInfoForSide(Side.East), Side.East));

        return triangles.ToArray();
    }

    public static Triangle[] BuildCylinder(Shape shape, int detail, Axis axis)
    {
        if (detail < 3)
            throw new ArgumentException("detail must be >= 3");

        List<Triangle> triangles = new();

        Vector3 center = shape.Position;
        Vector3 size = shape.Size;
        Vector3 min = center - size / 2f;
        Vector3 max = center + size / 2f;

        //todo, figure these out 
        Side ovalSide = axis switch
        {
            Axis.X => Side.West,
            Axis.Y => Side.Bottom,
            Axis.Z => Side.North,
            _ => throw new ArgumentException("invalid axis")
        };

        Side barrelSide = axis switch
        {
            Axis.X => Side.North,
            Axis.Y => Side.West,
            Axis.Z => Side.West,
            _ => throw new ArgumentException("invalid axis")
        };

        float length;
        float radiusA, radiusB;

        // Axis setup
        Func<float, float, Vector3> ringPoint;
        Vector3 axisDir;
        float start, end;

        switch (axis)
        {
            case Axis.X:
                length = size.X;
                radiusA = size.Y * 0.5f;
                radiusB = size.Z * 0.5f;
                axisDir = Vector3.UnitX;
                start = min.X;
                end = max.X;

                ringPoint = (a, b) =>
                    new Vector3(0, a, b);
                break;

            case Axis.Y:
                length = size.Y;
                radiusA = size.X * 0.5f;
                radiusB = size.Z * 0.5f;
                axisDir = Vector3.UnitY;
                start = min.Y;
                end = max.Y;

                ringPoint = (a, b) =>
                    new Vector3(a, 0, b);
                break;

            default: // Z
                length = size.Z;
                radiusA = size.X * 0.5f;
                radiusB = size.Y * 0.5f;
                axisDir = Vector3.UnitZ;
                start = min.Z;
                end = max.Z;

                ringPoint = (a, b) =>
                    new Vector3(a, b, 0);
                break;
        }

        float angleStep = MathF.Tau / detail;

        // Precompute rings
        Vector3[] ringStart = new Vector3[detail];
        Vector3[] ringEnd = new Vector3[detail];

        for (int i = 0; i < detail; i++)
        {
            float angle = i * angleStep;
            float a = MathF.Cos(angle) * radiusA;
            float b = MathF.Sin(angle) * radiusB;

            ringStart[i] = center + ringPoint(a, b) + axisDir * (start - Vector3.Dot(center, axisDir));
            ringEnd[i] = center + ringPoint(a, b) + axisDir * (end - Vector3.Dot(center, axisDir));
        }

        // Side faces
        for (int i = 0; i < detail; i++)
        {
            int next = (i + 1) % detail;

            // Quad split into two triangles
            // todo, figure out sides
            triangles.Add(new Triangle(ringStart[i], ringEnd[i], ringEnd[next], shape.TextureInfoForSide(barrelSide), barrelSide));
            triangles.Add(new Triangle(ringStart[i], ringEnd[next], ringStart[next], shape.TextureInfoForSide(barrelSide.Opposite()), barrelSide.Opposite()));
        }

        // Caps
        Vector3 capStartCenter = center + axisDir * (start - Vector3.Dot(center, axisDir));
        Vector3 capEndCenter = center + axisDir * (end - Vector3.Dot(center, axisDir));

        for (int i = 0; i < detail; i++)
        {
            int next = (i + 1) % detail;

            // Start cap
            triangles.Add(new Triangle(capStartCenter, ringStart[next], ringStart[i], shape.TextureInfoForSide(ovalSide), ovalSide));

            // End cap
            triangles.Add(new Triangle(capEndCenter, ringEnd[i], ringEnd[next], shape.TextureInfoForSide(ovalSide.Opposite()), ovalSide.Opposite()));
        }

        return triangles.ToArray();
    }
}