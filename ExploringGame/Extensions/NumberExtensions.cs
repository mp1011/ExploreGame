using System;

namespace ExploringGame.Extensions;

public static class NumberExtensions
{
    public static int NMod(this int value, int modulus)
    {
        if (modulus <= 0)
            throw new ArgumentOutOfRangeException(nameof(modulus), "Modulus must be greater than zero.");

        while (value < 0)
            value += modulus;

        return value % modulus;
    }

    public static float NMod(this float value, float modulus)
    {
        if (modulus <= 0)
            throw new ArgumentOutOfRangeException(nameof(modulus), "Modulus must be greater than zero.");

        while (value < 0)
            value += modulus;

        return value % modulus;
    }

    public static double NMod(this double value, double modulus)
    {
        if (modulus <= 0)
            throw new ArgumentOutOfRangeException(nameof(modulus), "Modulus must be greater than zero.");

        while (value < 0)
            value += modulus;

        return value % modulus;
    }

    public static float MoveToward(this float number, float target, float step)
    {
        step = Math.Abs(step);

        if(number == target)
            return number;
        else if(number < target)
        {
            number += step;
            if (number > target)
                return target;
            else
                return number;
        }
        else
        {
            number -= step;
            if (number < target)
                return target;
            else
                return number;
        }
    }
}

