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

public record Rotation(float Pitch, float Yaw, float Roll);

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

    public Triangle2D As2D(Vector3 faceOrigin)
    {
       return new Triangle2D(this, faceOrigin);
    }
}

public record Placement2D(float Left, float Top, float Right, float Bottom);