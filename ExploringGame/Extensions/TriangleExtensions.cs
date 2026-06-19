using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Extensions;

public static class TriangleExtensions
{
    public static Triangle[] InWorldCoordinates(this IEnumerable<Triangle> triangles, Vector3 origin)
    {
        return triangles.Select(p=>p.Offset(origin)).ToArray();
    }
}
