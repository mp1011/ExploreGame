using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ExploringGame.GeometryBuilder;

public enum ViewFrom
{
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

public record Rotation(float Yaw, float Pitch, float Roll);

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