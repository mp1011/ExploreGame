using ExploringGame.GeometryBuilder;
using System.Linq;

namespace ExploringGame.Services;

public class SideRemover
{
    public Triangle[] Execute(Triangle[] shape, Side omitSides)
    {
        foreach(var side in omitSides.Decompose())
        {
            shape = shape.Where(p => p.Side != side).ToArray();
        }
        
        return shape;
    }
}
