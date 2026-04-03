using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Services;

public class BestFitPlaneCalculator
{
    /// <summary>
    /// Computes the two corner vertices of an oriented bounding box for the given triangles.
    /// These corners represent the 0,0 and 1,1 texture coordinates.
    /// </summary>
    public (Vector3, Vector3) GetOrientedBoundingBoxCorners(IEnumerable<Triangle> triangles)
    {
        var triangleArray = triangles.ToArray();
        if (!triangleArray.Any())
            return (Vector3.Zero, Vector3.Zero);

        var vertices = triangleArray.SelectMany(t => t.Vertices).ToArray();
        
        // Compute the best fit plane
        var plane = ComputeBestFitPlane(triangleArray);
        
        // Create a 2D coordinate system on the plane
        var (uAxis, vAxis) = CreatePlaneCoordinateSystem(plane);
        
        // Project all vertices onto the plane and get their 2D coordinates
        var projected2D = vertices.Select(v => ProjectToPlane2D(v, plane, uAxis, vAxis)).ToArray();
        
        // Find the 2D bounding box
        var minU = projected2D.Min(p => p.X);
        var maxU = projected2D.Max(p => p.X);
        var minV = projected2D.Min(p => p.Y);
        var maxV = projected2D.Max(p => p.Y);
        
        // Convert the 2D corners back to 3D world space
        var corner1 = Reconstruct3DPoint(minU, minV, plane, uAxis, vAxis);
        var corner2 = Reconstruct3DPoint(maxU, maxV, plane, uAxis, vAxis);
        
        return (corner1, corner2);
    }

    /// <summary>
    /// Computes a best fit plane from the triangles by averaging their normals
    /// and using the centroid of all vertices as a point on the plane.
    /// </summary>
    private Plane ComputeBestFitPlane(Triangle[] triangles)
    {
        // Compute the centroid of all vertices
        var vertices = triangles.SelectMany(t => t.Vertices).ToArray();
        var centroid = new Vector3(
            vertices.Average(v => v.X),
            vertices.Average(v => v.Y),
            vertices.Average(v => v.Z)
        );

        // Average the normals of all triangles (weighted by area would be more accurate,
        // but for simplicity we'll treat all triangles equally)
        var averageNormal = Vector3.Zero;
        foreach (var triangle in triangles)
        {
            averageNormal += triangle.Normal;
        }
        averageNormal = Vector3.Normalize(averageNormal / triangles.Length);

        // Create a plane with the average normal passing through the centroid
        return new Plane(averageNormal, -Vector3.Dot(averageNormal, centroid));
    }

    /// <summary>
    /// Creates two perpendicular basis vectors on the plane.
    /// These form a 2D coordinate system where we can project vertices.
    /// </summary>
    private (Vector3 uAxis, Vector3 vAxis) CreatePlaneCoordinateSystem(Plane plane)
    {
        var normal = plane.Normal;

        // Choose an arbitrary vector that's not parallel to the normal
        var arbitrary = Math.Abs(normal.Y) < 0.9f ? Vector3.UnitY : Vector3.UnitX;

        // Create the first basis vector perpendicular to the normal
        var uAxis = Vector3.Normalize(Vector3.Cross(normal, arbitrary));

        // Create the second basis vector perpendicular to both the normal and uAxis
        var vAxis = Vector3.Normalize(Vector3.Cross(normal, uAxis));

        return (uAxis, vAxis);
    }

    /// <summary>
    /// Projects a 3D vertex onto the plane and returns its 2D coordinates in the plane's coordinate system.
    /// </summary>
    private Vector2 ProjectToPlane2D(Vector3 vertex, Plane plane, Vector3 uAxis, Vector3 vAxis)
    {
        // Project the vertex onto the plane
        var distance = Vector3.Dot(plane.Normal, vertex) + plane.D;
        var projectedVertex = vertex - plane.Normal * distance;

        // Find a reference point on the plane (using origin projected onto plane)
        var originDistance = plane.D;
        var planeOrigin = -plane.Normal * originDistance;

        // Express the projected vertex in the plane's 2D coordinate system
        var relativePosition = projectedVertex - planeOrigin;
        var u = Vector3.Dot(relativePosition, uAxis);
        var v = Vector3.Dot(relativePosition, vAxis);

        return new Vector2(u, v);
    }

    /// <summary>
    /// Reconstructs a 3D point from 2D coordinates in the plane's coordinate system.
    /// </summary>
    private Vector3 Reconstruct3DPoint(float u, float v, Plane plane, Vector3 uAxis, Vector3 vAxis)
    {
        // Find a reference point on the plane
        var originDistance = plane.D;
        var planeOrigin = -plane.Normal * originDistance;

        // Reconstruct the 3D point
        return planeOrigin + uAxis * u + vAxis * v;
    }
}
