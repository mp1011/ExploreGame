using ExploringGame.Extensions;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics.Metrics;

namespace ExploringGame.GeometryBuilder;

public record Angle
{
    public float Degrees { get; } 

    public float Radians { get; }

    private Angle(float value, bool isDegrees)
    {
        if (isDegrees)
        {
            Degrees = value;
            Radians = MathHelper.ToRadians(value);
        }
        else
        {
            Radians = value;
            Degrees = MathHelper.ToDegrees(value);
        }
    }

    public static Angle FromRad(float radians) => new Angle(radians, isDegrees: false);
    public static Angle FromDeg(float degrees) => new Angle(degrees, isDegrees: true);

    public Angle RotateTowards(float target, float amount)
    {
        float delta = MathF.IEEERemainder(target - Degrees, 360f);

        if (MathF.Abs(delta) <= amount)
            return Angle.FromDeg(target);

        return Angle.FromDeg(Degrees + MathF.Sign(delta) * amount);
    }

    public float Delta(Angle other)
    {
        float diff = (other.Degrees - Degrees) % 360f;

        if (diff > 180f)
            diff -= 360f;
        else if (diff < -180f)
            diff += 360f;

        return diff;
    }

    public float ShortestRotation(float absDelta, Angle target)
    {
        // Shortest signed difference (-180, 180]
        float delta = ((target.Degrees - Degrees + 540f) % 360f) - 180f;

        // If we're already within absDelta, just return the remaining distance.
        if (MathF.Abs(delta) <= absDelta)
            return delta;

        // Otherwise move by exactly absDelta in the correct direction.
        return MathF.Sign(delta) * absDelta;
    }

    public Angle RotateCounterClockwise(float degrees) => Angle.FromDeg(Degrees + degrees);
    public Angle RotateClockwise(float degrees) => Angle.FromDeg(Degrees - degrees);

    public Angle(Side side) : this(side switch
    {
        Side.North => 0.0f,
        Side.East => 270.0f,
        Side.South => 180.0f,
        Side.West => 90.0f,
        Side.NorthEast => 270.0f + 45.0f,
        Side.SouthEast => 270.0f - 45.0f,
        Side.SouthWest => 180 - 45.0f,
        Side.NorthWest => 45.0f,
        _ => throw new ArgumentException("invalid side")
    }, isDegrees: true)
    { }

    public Side ToSide() => Degrees.NMod(360) switch
    {
        0.0f => Side.North,
        270.0f => Side.East,
        180.0f => Side.South,
        90.0f => Side.West,
        _ => throw new ArgumentException("invalid angle for side")
    };

    public static implicit operator Angle(Side value) => new Angle(value);
}
