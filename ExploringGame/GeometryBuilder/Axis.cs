using System.Runtime.CompilerServices;

namespace ExploringGame.GeometryBuilder;

public enum Axis
{
    None = 0,
    X = 1,
    Y = 2,
    Z = 4,
}

public static class AxisExtensions
{
    public static Axis Orthogonal(this Axis axis) => axis switch
    {
        Axis.X => Axis.Z,
        Axis.Z => Axis.X,
        _ => throw new System.NotImplementedException("work on this")
    };
}