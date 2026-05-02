using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;
using System;

namespace ExploringGame.Extensions;

public static class MatrixExtensions
{
    public static Rotation RotationFromView(this Matrix view)
    {
        var world = Matrix.Invert(view);
        var forward = Vector3.Normalize(Vector3.TransformNormal(Vector3.Forward, world));

        return new Rotation(
            Yaw: MathF.Atan2(-forward.X, -forward.Z),
            Pitch: MathF.Asin(Math.Clamp(forward.Y, -1f, 1f)),
            Roll: 0f);
    }
}
