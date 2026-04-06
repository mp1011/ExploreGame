using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Services;

/// <summary>
/// Helper class for calculating consistent plane and texture origin for tiling
/// Used by both triangle splitting and texture coordinate calculation to ensure consistency
/// </summary>
public static class TilingPlaneHelper
{
    public struct PlaneInfo
    {
        public Vector3 Normal;
        public Vector3 UAxis;
        public Vector3 VAxis;
        public Vector3 TextureOrigin;

        public PlaneInfo(Vector3 normal, Vector3 uAxis, Vector3 vAxis, Vector3 textureOrigin)
        {
            Normal = normal;
            UAxis = uAxis;
            VAxis = vAxis;
            TextureOrigin = textureOrigin;
        }
    }

    /// <summary>
    /// Computes the plane and texture origin for a set of triangles on the same side.
    /// The texture origin is the projection of the zero vector onto the plane,
    /// ensuring all shapes on the same plane share the same texture origin.
    /// </summary>
    public static PlaneInfo ComputePlaneInfo(IEnumerable<Triangle> sideTriangles, (Vector3, Vector3) cornerVertices)
    {
        var triangleArray = sideTriangles.ToArray();
        if (!triangleArray.Any())
            return new PlaneInfo(Vector3.UnitY, Vector3.UnitX, Vector3.UnitZ, Vector3.Zero);

        // Use the corner vertices to define the plane
        // We need to get the normal from the triangles themselves
        var averageNormal = Vector3.Zero;
        foreach (var triangle in triangleArray)
        {
            averageNormal += triangle.Normal;
        }
        averageNormal = Vector3.Normalize(averageNormal / triangleArray.Length);

        // Create a consistent coordinate system using the same approach as GridTriangleSubdivider
        Vector3 normal = averageNormal;
        
        // Choose a fixed world reference axis (same as GridTriangleSubdivider.ComputeCanonicalBasis)
        Vector3 reference =
            Math.Abs(normal.Y) < 0.999f ? Vector3.UnitY :
            Math.Abs(normal.X) < 0.999f ? Vector3.UnitX :
                                           Vector3.UnitZ;

        // Project reference axis onto the plane → U
        Vector3 uAxis = reference - Vector3.Dot(reference, normal) * normal;
        uAxis = Vector3.Normalize(uAxis);

        // Derive V (guaranteed orthonormal and consistent)
        Vector3 vAxis = Vector3.Cross(normal, uAxis);

        // Project the zero vector onto this plane to get our texture origin
        // This is the closest point on the plane to the world origin (0,0,0)
        Vector3 textureOrigin = ProjectPointOntoPlane(Vector3.Zero, normal, cornerVertices.Item1);

        return new PlaneInfo(normal, uAxis, vAxis, textureOrigin);
    }

    /// <summary>
    /// Projects a point onto a plane defined by a normal and a point on the plane
    /// </summary>
    private static Vector3 ProjectPointOntoPlane(Vector3 point, Vector3 planeNormal, Vector3 pointOnPlane)
    {
        // Distance from point to plane
        Vector3 toPoint = point - pointOnPlane;
        float distance = Vector3.Dot(toPoint, planeNormal);
        
        // Project the point onto the plane
        return point - planeNormal * distance;
    }

    /// <summary>
    /// Projects a 3D point onto the 2D plane coordinate system
    /// </summary>
    public static Vector2 ProjectTo2D(Vector3 point, PlaneInfo planeInfo)
    {
        Vector3 d = point - planeInfo.TextureOrigin;
        return new Vector2(Vector3.Dot(d, planeInfo.UAxis), Vector3.Dot(d, planeInfo.VAxis));
    }

    /// <summary>
    /// Unprojects a 2D point back to 3D space
    /// </summary>
    public static Vector3 UnprojectTo3D(Vector2 point, PlaneInfo planeInfo)
    {
        return planeInfo.TextureOrigin + point.X * planeInfo.UAxis + point.Y * planeInfo.VAxis;
    }
}
