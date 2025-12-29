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
    public static FaceBasis FromSide(Side side)
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

/// <summary>
/// 2D Projection of a 3D Triangle
/// </summary>
public class Triangle2D
{
    public Triangle Original { get; }

    public Triangle2D(Triangle original, Vector3 faceOrigin)
    {
        Original = original;
        var faceBasis = FaceBasis.FromSide(original.Side);
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

    public Triangle To3D(Vector3 faceOrigin)
    {
        var faceBasis = FaceBasis.FromSide(Original.Side);
        return new Triangle(
            A: A.Unproject(faceOrigin, faceBasis.U, faceBasis.V),
            B: B.Unproject(faceOrigin, faceBasis.U, faceBasis.V),
            C: C.Unproject(faceOrigin, faceBasis.U, faceBasis.V),
            TextureInfo: Original.TextureInfo,
            Side: Original.Side);
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

