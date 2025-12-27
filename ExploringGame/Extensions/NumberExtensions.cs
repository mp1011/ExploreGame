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

    public static double NMod(this double value, double modulus)
    {
        if (modulus <= 0)
            throw new ArgumentOutOfRangeException(nameof(modulus), "Modulus must be greater than zero.");

        while (value < 0)
            value += modulus;

        return value % modulus;
    }
}

