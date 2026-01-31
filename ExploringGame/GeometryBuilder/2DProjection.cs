using ExploringGame.Extensions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace ExploringGame.GeometryBuilder;


/// <summary>
/// U = 2d X
/// N = Normal
/// V = 2d Y
/// </summary>
/// <param name="U"></param>
/// <param name="N"></param>
/// <param name="V"></param>
public record FaceBasis(Vector3 U, Vector3 N, Vector3 V)
{
    public static FaceBasis FromSide(Side side, ViewFrom viewFrom)
    {
        if (viewFrom == ViewFrom.Inside)
        {
            return side switch
            {
                Side.South => new FaceBasis(U: -Vector3.UnitX, V: Vector3.UnitY, N: Vector3.UnitZ),
                Side.North => new FaceBasis(U: Vector3.UnitX, V: Vector3.UnitY, N: -Vector3.UnitZ),
                Side.West => new FaceBasis(U: -Vector3.UnitZ, V: Vector3.UnitY, N: -Vector3.UnitX),
                Side.East => new FaceBasis(U: Vector3.UnitZ, V: Vector3.UnitY, N: Vector3.UnitX),
                Side.Top => new FaceBasis(U: Vector3.UnitX, V: -Vector3.UnitZ, N: Vector3.UnitY),
                Side.Bottom => new FaceBasis(U: Vector3.UnitX, V: Vector3.UnitZ, N: -Vector3.UnitY),
                _ => throw new ArgumentOutOfRangeException("Invalid side")
            };
        }
        else
        {
            return side switch
            {
                Side.South => new FaceBasis(U: Vector3.UnitX, V: Vector3.UnitY, N: Vector3.UnitZ),
                Side.North => new FaceBasis(U: -Vector3.UnitX, V: Vector3.UnitY, N: -Vector3.UnitZ),
                Side.West => new FaceBasis(U: Vector3.UnitZ, V: Vector3.UnitY, N: -Vector3.UnitX),
                Side.East => new FaceBasis(U: -Vector3.UnitZ, V: Vector3.UnitY, N: Vector3.UnitX),
                Side.Top => new FaceBasis(U: Vector3.UnitX, V: -Vector3.UnitZ, N: Vector3.UnitY),
                Side.Bottom => new FaceBasis(U: Vector3.UnitX, V: Vector3.UnitZ, N: -Vector3.UnitY),
                _ => throw new ArgumentOutOfRangeException("Invalid side")
            };
        }
    }
}

/// <summary>
/// 2D Projection of a 3D Triangle
/// </summary>
public class Triangle2D : IPolygon2D
{
    public Triangle Original { get; }

    public Triangle2D(Triangle original, Vector3 faceOrigin, ViewFrom viewFrom)
    {
        Original = original;
        var faceBasis = FaceBasis.FromSide(original.Side, viewFrom);
        A = original.A.Project(faceOrigin, faceBasis.U, faceBasis.V);
        B = original.B.Project(faceOrigin, faceBasis.U, faceBasis.V);
        C = original.C.Project(faceOrigin, faceBasis.U, faceBasis.V);
    }

    public Triangle2D(Vector2 a, Vector2 b, Vector2 c, Triangle original)
    {
        Original = original;
        A = a; B = b; C = c;
    }


    public Vector2 A { get; set; }
    public Vector2 B { get; set; }
    public Vector2 C { get; set; }

    public Vector2 Center => (A + B + C) / 3f;

    public float[] SideLengths => [(A - B).Length(), (B - C).Length(), (C - A).Length()];
        
    public Winding Winding
    {
        get
        {
            var p = Center;
            var a = A;
            var b = B;
            var c = C;
            int windingNumber = 0;

            // Edge AB
            if (a.Y <= p.Y)
            {
                if (b.Y > p.Y && IsLeft(a, b, p) > 0)
                    windingNumber++;
            }
            else
            {
                if (b.Y <= p.Y && IsLeft(a, b, p) < 0)
                    windingNumber--;
            }

            // Edge BC
            if (b.Y <= p.Y)
            {
                if (c.Y > p.Y && IsLeft(b, c, p) > 0)
                    windingNumber++;
            }
            else
            {
                if (c.Y <= p.Y && IsLeft(b, c, p) < 0)
                    windingNumber--;
            }

            // Edge CA
            if (c.Y <= p.Y)
            {
                if (a.Y > p.Y && IsLeft(c, a, p) > 0)
                    windingNumber++;
            }
            else
            {
                if (a.Y <= p.Y && IsLeft(c, a, p) < 0)
                    windingNumber--;
            }

            if (windingNumber < 0)
                return Winding.Clockwise;
            else
                return Winding.CounterClockwise;
        }
    }

    private static float IsLeft(Vector2 a, Vector2 b, Vector2 p)
    {
        // Cross product (b - a) x (p - a)
        return (b.X - a.X) * (p.Y - a.Y)
             - (p.X - a.X) * (b.Y - a.Y);
    }

    public double AngleA { get => A.AngleBetween(B, C); }
    public double AngleB { get => B.AngleBetween(A, C); }
    public double AngleC { get => C.AngleBetween(A, B); }

    public IEnumerable<Vector2> Vertices
    {
        get
        {
            yield return A;
            yield return B;
            yield return C;
        }
    }

    public Triangle To3D(Vector3 faceOrigin, ViewFrom viewFrom)
    {
        var faceBasis = FaceBasis.FromSide(Original.Side, viewFrom);
        return new Triangle(
            A: A.Unproject(faceOrigin, faceBasis.U, faceBasis.V),
            B: B.Unproject(faceOrigin, faceBasis.U, faceBasis.V),
            C: C.Unproject(faceOrigin, faceBasis.U, faceBasis.V),
            TextureInfo: Original.TextureInfo,
            Side: Original.Side);
    }

   
    public bool ContainsPoint(Vector2 point)
    {
        var t = this;
        var p = point;

        var v0 = new Vector2(t.C.X - t.A.X, t.C.Y - t.A.Y);
        var v1 = new Vector2(t.B.X - t.A.X, t.B.Y - t.A.Y);
        var v2 = new Vector2(p.X - t.A.X, p.Y - t.A.Y);

        float dot00 = v0.X * v0.X + v0.Y * v0.Y;
        float dot01 = v0.X * v1.X + v0.Y * v1.Y;
        float dot02 = v0.X * v2.X + v0.Y * v2.Y;
        float dot11 = v1.X * v1.X + v1.Y * v1.Y;
        float dot12 = v1.X * v2.X + v1.Y * v2.Y;

        float invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
        float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
        float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

        return (u >= 0) && (v >= 0) && (u + v <= 1);
    }

    public void ReplaceVertex(Vector2 oldV, Vector2 newV)
    {
        if (oldV == A)
            A = newV;
        else if (oldV == B)
            B = newV;
        else if (oldV == C)
            C = newV;
    }
}

