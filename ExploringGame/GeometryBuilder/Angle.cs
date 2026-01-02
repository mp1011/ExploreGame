using System;

namespace ExploringGame.GeometryBuilder;

public record Angle(float Degrees)
{
    public Angle RotateTowards(float target, float amount)
    {
        float delta = MathF.IEEERemainder(target - Degrees, 360f);

        if (MathF.Abs(delta) <= amount)
            return new Angle(target);

        return new Angle(Degrees + MathF.Sign(delta) * amount);
    }
}
