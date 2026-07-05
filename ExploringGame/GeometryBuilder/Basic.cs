using ExploringGame.Extensions;
using ExploringGame.Texture;
using Jitter2.LinearMath;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Runtime;
using MathHelper = Microsoft.Xna.Framework.MathHelper;

namespace ExploringGame.GeometryBuilder;

public enum ViewFrom
{
    None,
    Inside,
    Outside
}

public enum QualityLevel
{
    DoNotRender = 0,
    CuboidOnly = 1,
    Basic = 2
}

public enum Winding
{
    Clockwise,
    CounterClockwise,
}

public interface IPolygon2D
{
    IEnumerable<Vector2> Vertices { get; }
}

public enum HAlign
{
    Left,
    Center,
    Right
};

public enum DoorDirection
{
    Push,
    Pull
}

/// <summary>
/// Yaw = side to side
/// Pitch = up and down
/// </summary>
/// <param name="Yaw"></param>
/// <param name="Pitch"></param>
/// <param name="Roll"></param>
public record Rotation(Quaternion Quaternion)
{

    public Rotation(float Yaw, float Pitch, float Roll) : this(Quaternion.CreateFromYawPitchRoll(Yaw, Pitch, Roll))
    {
    }

    public static Rotation YawFromDegrees(float YawDegrees) => new Rotation(
        Yaw: (YawDegrees * MathHelper.Pi) / 180.0f, 
        Pitch: 0, Roll: 0);

    public Angle Yaw
    {
        get
        {
            var q = Quaternion.Normalize(Quaternion);

            return Angle.FromRad(MathF.Atan2(
                2f * (q.W * q.Y + q.X * q.Z),
                1f - 2f * (q.Y * q.Y + q.X * q.X)
            ));
        }
    }

    public Angle Pitch
    {
        get
        {
            var q = Quaternion.Normalize(Quaternion);

            float sinp = 2f * (q.W * q.X - q.Y * q.Z);

            if (MathF.Abs(sinp) >= 1f)
            {
                // Clamp at 90 degrees if out of range
                return Angle.FromRad(MathF.CopySign(MathF.PI / 2f, sinp));
            }

            return Angle.FromRad(MathF.Asin(sinp));
        }
    }

    public Angle Roll
    {
        get
        {
            var q = Quaternion.Normalize(Quaternion);

            return Angle.FromRad(MathF.Atan2(
                2f * (q.W * q.Z + q.X * q.Y),
                1f - 2f * (q.Z * q.Z + q.X * q.X)
            ));
        }
    }

    public Matrix AsMatrix() => Matrix.CreateFromQuaternion(Quaternion);

    public bool IsCloseTo(Rotation other, float toleranceDegrees = 5f)
    {
        // Ensure both are normalized (important for stability)
        var a = Quaternion.Normalize(this.Quaternion);
        var b = Quaternion.Normalize(other.Quaternion);

        float dot = Quaternion.Dot(a, b);

        // Account for double-cover: q and -q represent same rotation
        dot = MathF.Abs(dot);

        // Clamp for safety against floating point drift
        dot = MathF.Min(1.0f, MathF.Max(-1.0f, dot));

        // Angle between rotations
        float angleRadians = 2.0f * MathF.Acos(dot);
        float angleDegrees = angleRadians * (180.0f / MathF.PI);

        return angleDegrees <= toleranceDegrees;
    }

    public override string ToString()
    {
        return $"Yaw: {Yaw.Degrees.ToString("0.000")} Pitch: {Pitch.Degrees.ToString("0.000")} Roll: {Roll.Degrees.ToString("0.000")}";
    }
}

public record Triangle(Vector3 A, Vector3 B, Vector3 C, TextureInfo TextureInfo, Side Side)
{
    public Triangle Invert() => new Triangle(C, B, A, TextureInfo, Side);

    public IEnumerable<Vector3> Vertices
    {
        get
        {
            yield return A;
            yield return B;
            yield return C;
        }
    }

    public Triangle Offset(Vector3 offset)
    {
        return new Triangle(A + offset, B + offset, C + offset, TextureInfo, Side);
    }

    public Vector3 Normal => Vector3.Normalize(Vector3.Cross(C - A, B - A));

    public Winding CalcWinding(Vector3 observationPoint)
    {
        Vector3 normal = Vector3.Cross(B - A, C - A);
        Vector3 toCamera = observationPoint - A;

        var ccw = Vector3.Dot(normal, toCamera) > 0f;
        return ccw ? Winding.CounterClockwise : Winding.Clockwise;
    }

    public Triangle2D As2D(Vector3 faceOrigin, ViewFrom viewFrom)
    {
        return new Triangle2D(this, faceOrigin, viewFrom);
    }

    public Triangle Rotate(Vector3 pivot, Rotation rotation)
    {
        return new Triangle(A.Rotate(pivot, rotation),
                            B.Rotate(pivot, rotation),
                            C.Rotate(pivot, rotation),
                            TextureInfo,
                            Side); // note: side will still refer to the original, unrotated side
    }

    public Triangle ReplaceVertex(Vector3 oldVertex, Vector3 newVertex)
    {
        if (A.IsAlmost(oldVertex))
            return new Triangle(newVertex, B, C, TextureInfo, Side);
        else if (B.IsAlmost(oldVertex))
            return new Triangle(A, newVertex, C, TextureInfo, Side);
        else if (C.IsAlmost(oldVertex))
            return new Triangle(A, B, newVertex, TextureInfo, Side);
        else
            return this;
    }

    public Triangle SetSide(Side side, float value)
    {
        switch(side)
        {
            case Side.Top:
            case Side.Bottom:
                return new Triangle(A.SetY(value), B.SetY(value), C.SetY(value), TextureInfo, Side);
            default:
                throw new Exception("not sure about this");
        }
    }

    public bool IsDegenerate
    {
        get => (A == B) || (B == C) || (A == C) || Vector3.Cross(B - A, C - A).LengthSquared() < 1e-8f;
    }
}

public record Placement2D(float Left, float Top, float Right, float Bottom);

public record RectangleF(float Left, float Top, float Width, float Height)
{
    public float Right => Left + Width;
    public float Bottom => Top + Height;

    public bool Contains(Vector2 point)
    {
        return point.X >= Left && point.X <= Right &&
               point.Y >= Top && point.Y <= Bottom;
    }
}