using ExploringGame.Extensions;
using System;

namespace ExploringGame.GeometryBuilder;

public record Angle
{
    public float Degrees { get; } 

    public Angle(float degrees)
    {
        Degrees = degrees.NMod(360.0f);
    }

    public Angle RotateTowards(float target, float amount)
    {
        float delta = MathF.IEEERemainder(target - Degrees, 360f);

        if (MathF.Abs(delta) <= amount)
            return new Angle(target);

        return new Angle(Degrees + MathF.Sign(delta) * amount);
    }

    public Angle RotateCounterClockwise(float degrees) => new Angle(Degrees + degrees);
    public Angle RotateClockwise(float degrees) => new Angle(Degrees - degrees);

    public Angle(Side side) : this(side switch
    {
        Side.North => 0.0f,
        Side.East => 270.0f,
        Side.South => 180.0f,
        Side.West => 90/0f,
        Side.NorthEast => 270.0f + 45.0f,
        Side.SouthEast => 270.0f - 45.0f,
        Side.SouthWest => 180 - 45.0f,
        Side.NorthWest => 45.0f,
        _ => throw new ArgumentException("invalid side")
    })
    { }
}
