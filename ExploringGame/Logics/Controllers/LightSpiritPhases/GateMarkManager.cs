using ExploringGame.Entities;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Rendering;
using ExploringGame.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Logics.Controllers.LightSpiritPhases;

/// <summary>
/// Manages spawning and tracking of GateMarks
/// </summary>
public class GateMarkManager
{
    private readonly WorldSegment _worldSegment;
    private readonly LoadedLevelData _loadedLevelData;
    private readonly PointLights _pointLights;
    private readonly List<GateMark> _gateMarks = new();
    private readonly List<WallQuad> _availableQuads = new();
    private readonly Random _random = new();

    public IReadOnlyList<GateMark> GateMarks => _gateMarks;

    public GateMarkManager(WorldSegment worldSegment, LoadedLevelData loadedLevelData, PointLights pointLights)
    {
        _worldSegment = worldSegment;
        _loadedLevelData = loadedLevelData;
        _pointLights = pointLights;
        
        // Build and extract wall quads for validation
        InitializeWallQuads();
    }

    private void InitializeWallQuads()
    {
        var rooms = _worldSegment.TraverseAllChildren().OfType<Room>();

        foreach (var room in rooms)
        {
            // Build with basic quality to detect holes/cutouts
            var shapesAndTriangles = room.Build(QualityLevel.Basic);
            
            if (!shapesAndTriangles.TryGetValue(room, out var triangles))
                continue;

            // Process each wall side
            foreach (Side side in new[] { Side.North, Side.South, Side.East, Side.West })
            {
                var sideTriangles = triangles.Where(t => t.Side == side).ToArray();
                if (sideTriangles.Length == 0)
                    continue;

                // Extract quads from triangles
                var quads = ExtractQuadsFromTriangles(room, side, sideTriangles);
                
                // Keep only quads large enough for gatemarks (0.5x0.5)
                var validQuads = quads.Where(q => q.Width >= 0.5f && q.Height >= 0.5f);
                _availableQuads.AddRange(validQuads);
            }
        }
    }

    private List<WallQuad> ExtractQuadsFromTriangles(Room room, Side side, Triangle[] triangles)
    {
        var quads = new List<WallQuad>();
        var processedTriangles = new HashSet<Triangle>();

        foreach (var triangle in triangles)
        {
            if (processedTriangles.Contains(triangle))
                continue;

            // Find a connected triangle (shares 2 vertices)
            var connectedTriangle = triangles.FirstOrDefault(t => 
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

    public GateMark SpawnGateMark()
    {
        if (_availableQuads.Count == 0)
            return null;

        // Pick a random quad
        var quad = _availableQuads[_random.Next(_availableQuads.Count)];

        // Remove this quad so it's not used again
        _availableQuads.Remove(quad);

        // Create the gatemark
        var gateMark = new GateMark(quad.Room, quad.Side, null, _pointLights);
        quad.Room.AddChild(gateMark);
        _gateMarks.Add(gateMark);

        gateMark.Place().OnQuad(quad, _random);

        // Add to rendering system as a stamped shape
        var levelData = _loadedLevelData.FindLevelDataForWorldSegment(_worldSegment);
        if (levelData != null)
        {
            _loadedLevelData.AddWallDecal(_worldSegment, gateMark);
        }

        return gateMark;
    }

    public void ActivateRandomGateMark()
    {
        var inactiveMarks = _gateMarks.Where(gm => !gm.IsActive).ToList();
        
        if (inactiveMarks.Count == 0)
            return;

        // 10% chance for each inactive gatemark
        foreach (var mark in inactiveMarks)
        {
            if (_random.NextDouble() < 0.10)
            {
                mark.IsActive = true;
                return;
            }
        }
    }

    public GateMark GetClosestActiveGateMark(Vector3 position)
    {
        var activeMarks = _gateMarks.Where(gm => gm.IsActive).ToList();
        
        if (activeMarks.Count == 0)
            return null;

        return activeMarks.OrderBy(gm => Vector3.Distance(gm.Position, position)).First();
    }

    public void RemoveGateMark(GateMark gateMark)
    {
        if (gateMark == null || !_gateMarks.Contains(gateMark))
            return;

        gateMark.IsActive = false;
        _gateMarks.Remove(gateMark);
    }
}

