using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Services;

public class QuadExtractor
{
    public List<WallQuad> ExtractQuadsFromTriangles(Room room, Side side, Triangle[] sideTriangles)
    {
        var quads = new List<WallQuad>();
        var processedTriangles = new HashSet<Triangle>();

        foreach (var triangle in sideTriangles)
        {
            if (processedTriangles.Contains(triangle))
                continue;

            // Find a connected triangle (shares 2 vertices)
            var connectedTriangle = sideTriangles.FirstOrDefault(t =>
                t != triangle &&
                !processedTriangles.Contains(t) &&
                SharedVertexCount(triangle, t) >= 2);

            if (connectedTriangle != null)
            {
                // Two triangles form a quad
                var quadVertices = GetQuadVertices(triangle, connectedTriangle);
                if (quadVertices != null)
                {
                    quads.Add(new WallQuad(room, side, quadVertices));
                    processedTriangles.Add(triangle);
                    processedTriangles.Add(connectedTriangle);
                }
            }
        }

        return quads;
    }

    private int SharedVertexCount(Triangle t1, Triangle t2)
    {
        int count = 0;
        var vertices1 = t1.Vertices;
        var vertices2 = t2.Vertices;

        foreach (var v1 in vertices1)
        {
            if (vertices2.Any(v2 => Vector3.Distance(v1, v2) < 0.001f))
                count++;
        }

        return count;
    }

    private Vector3[] GetQuadVertices(Triangle t1, Triangle t2)
    {
        // Find the 4 unique vertices that form the quad
        var allVertices = t1.Vertices.Concat(t2.Vertices).ToList();
        var uniqueVertices = new List<Vector3>();

        foreach (var vertex in allVertices)
        {
            if (!uniqueVertices.Any(v => Vector3.Distance(v, vertex) < 0.001f))
            {
                uniqueVertices.Add(vertex);
            }
        }

        if (uniqueVertices.Count != 4)
            return null; // Not a valid quad

        // Order vertices to form a proper quad (clockwise or counter-clockwise)
        return OrderQuadVertices(uniqueVertices.ToArray());
    }

    private Vector3[] OrderQuadVertices(Vector3[] vertices)
    {
        // Find center
        var center = (vertices[0] + vertices[1] + vertices[2] + vertices[3]) / 4f;

        // Sort vertices by angle around center
        var ordered = vertices.OrderBy(v =>
        {
            var dir = v - center;
            return Math.Atan2(dir.Y + dir.Z, dir.X + dir.Z);
        }).ToArray();

        return ordered;
    }

}
