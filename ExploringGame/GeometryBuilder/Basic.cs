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

/// <summary>
/// Yaw = side to side
/// Pitch = up and down
/// </summary>
/// <param name="Yaw"></param>
/// <param name="Pitch"></param>
/// <param name="Roll"></param>
public record Rotation(float Yaw = 0f, float Pitch = 0f, float Roll = 0f)
{
    public float YawDegrees => (Yaw * 180.0f / MathHelper.Pi).NMod(360f);

    public Matrix AsMatrix() => Matrix.CreateFromYawPitchRoll(Yaw, Pitch, Roll);

    public Quaternion AsQuaternion() => Quaternion.CreateFromYawPitchRoll(Yaw, Pitch, Roll);


    public static Rotation FromJQuaternion(JQuaternion q)
    {
        // Normalize to avoid drift
        q = JQuaternion.Normalize(q);

        // Pitch (X axis)
        float sinp = 2f * (q.W * q.X - q.Z * q.Y);
        float pitch;
        if (MathF.Abs(sinp) >= 1f)
            pitch = MathF.CopySign(MathF.PI / 2f, sinp); // gimbal lock
        else
            pitch = MathF.Asin(sinp);

        // Yaw (Y axis)
        var yaw = MathF.Atan2(
            2f * (q.W * q.Y + q.X * q.Z),
            1f - 2f * (q.X * q.X + q.Y * q.Y)
        );

        // Roll (Z axis)
        var roll = MathF.Atan2(
            2f * (q.W * q.Z + q.X * q.Y),
            1f - 2f * (q.X * q.X + q.Z * q.Z)
        );

        return new Rotation(yaw, pitch, roll);
    }

    public static Rotation YawFromDegrees(float degrees, float pitch = 0f, float roll = 0f) => 
        new Rotation(Yaw: (degrees * MathHelper.Pi) / 180.0f, Pitch: pitch, Roll: roll);
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
        if (A == oldVertex)
            return new Triangle(newVertex, B, C, TextureInfo, Side);
        else if (B == oldVertex)
            return new Triangle(A, newVertex, C, TextureInfo, Side);
        else if (C == oldVertex)
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