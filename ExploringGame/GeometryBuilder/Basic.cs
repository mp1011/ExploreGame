using ExploringGame.Extensions;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Runtime;

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

/// <summary>
/// Yaw = side to side
/// Pitch = up and down
/// </summary>
/// <param name="Yaw"></param>
/// <param name="Pitch"></param>
/// <param name="Roll"></param>
public record Rotation(float Yaw = 0f, float Pitch = 0f, float Roll = 0f)
{
    public Matrix AsMatrix() => Matrix.CreateFromYawPitchRoll(Yaw, Pitch, Roll);

    public static Rotation YawFromDegrees(float degrees) => new Rotation(Yaw: (degrees * MathHelper.Pi) / 180.0f);
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

    public Winding CalcWinding(Vector3 observationPoint)
    {
        Vector3 normal = Vector3.Cross(B - A, C - A); 
        Vector3 toCamera = observationPoint - A;

        var ccw = Vector3.Dot(normal, toCamera) > 0f;
        return ccw ? Winding.CounterClockwise : Winding.Clockwise;
    }

    public Triangle2D As2D(Vector3 faceOrigin)
    {
       return new Triangle2D(this, faceOrigin);
    }

    public Triangle Rotate(Vector3 pivot, Rotation rotation)
    {
        return new Triangle(A.Rotate(pivot, rotation),
                            B.Rotate(pivot, rotation),
                            C.Rotate(pivot, rotation),
                            TextureInfo,
                            Side); // note: side will still refer to the original, unrotated side
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