using ExploringGame.Entities;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.Decals;
using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Rendering;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Tests.WallDecalPlacement;

/// <summary>
/// Test controller that places WallDecals using quad-based approach
/// </summary>
public class WallDecalTestController : IActiveObject
{
    private readonly WallWithGapWorldSegment _worldSegment;
    private readonly PointLights _pointLights;
    private readonly List<TestWallDecal> _placedDecals = new();
    private readonly Random _random = new(42); // Fixed seed for deterministic tests
    private readonly LoadedLevelData _loadedLevelData;

    private int _decalsToPlace = 100;
    private bool _initialized = false;

    public IReadOnlyList<TestWallDecal> PlacedDecals => _placedDecals;

    public WallDecalTestController(WallWithGapWorldSegment worldSegment, PointLights pointLights, LoadedLevelData loadedLevelData)
    {
        _worldSegment = worldSegment;
        _pointLights = pointLights;
        _loadedLevelData = loadedLevelData;
    }

    public void Initialize()
    {

    }

    private void ValidateDecalPosition(TestWallDecal decal, WallQuad sourceQuad)
    {
        const float epsilon = 0.0001f; // Tolerance for floating point precision
        
        var gapStart = _worldSegment.GapStartX;
        var gapEnd = _worldSegment.GapEndX;

        var decalLeftX = decal.Position.X - (decal.Width / 2f);
        var decalRightX = decal.Position.X + (decal.Width / 2f);

        // Calculate actual overlap amount
        float overlapAmount = 0f;
        if (decalRightX > gapStart && decalLeftX < gapEnd)
        {
            overlapAmount = Math.Min(decalRightX, gapEnd) - Math.Max(decalLeftX, gapStart);
        }

        // Only fail if overlap exceeds epsilon tolerance
        if (overlapAmount > epsilon)
        {
            // DECAL OVERLAPS GAP - throw detailed exception
            var errorMsg = $"❌ INVALID DECAL PLACEMENT DETECTED!\n" +
                $"Decal Position: X={decal.Position.X:F2}, Y={decal.Position.Y:F2}, Z={decal.Position.Z:F2}\n" +
                $"Decal X bounds: [{decalLeftX:F2} to {decalRightX:F2}]\n" +
                $"Gap X bounds: [{gapStart:F2} to {gapEnd:F2}]\n" +
                $"Overlap amount: {overlapAmount:F4} (tolerance: {epsilon:F4})\n" +
                $"Source Quad vertices:\n";
            
            for (int i = 0; i < sourceQuad.Vertices.Length; i++)
            {
                errorMsg += $"  V{i}: {sourceQuad.Vertices[i]}\n";
            }
            
            errorMsg += $"Source Quad X range: [{sourceQuad.Vertices.Min(v => v.X):F2} to {sourceQuad.Vertices.Max(v => v.X):F2}]\n";
            errorMsg += $"Source Quad dimensions: {sourceQuad.Width:F2} x {sourceQuad.Height:F2}";

            throw new InvalidOperationException(errorMsg);
        }
    }

    public void Stop()
    {
    }

    public void Update(GameTime gameTime)
    {
        if (_placedDecals.Any())
            return;

        // Extract quads from north wall
        var quads = ExtractQuadsFromNorthWall();

        System.Console.WriteLine($"Extracted {quads.Count} quads from north wall");
        System.Console.WriteLine($"Gap boundaries: X=[{_worldSegment.GapStartX:F2} to {_worldSegment.GapEndX:F2}]");

        // Try to place decals
        for (int i = 0; i < _decalsToPlace && quads.Count > 0; i++)
        {
            var quad = quads[_random.Next(quads.Count)];

            System.Console.WriteLine($"\n--- Placing Decal {i + 1} ---");
            System.Console.WriteLine($"Quad vertices:");
            for (int v = 0; v < quad.Vertices.Length; v++)
            {
                System.Console.WriteLine($"  V{v}: {quad.Vertices[v]}");
            }
            System.Console.WriteLine($"Quad X range: [{quad.Vertices.Min(v => v.X):F2} to {quad.Vertices.Max(v => v.X):F2}]");

            // Create decal with dummy placement, then use OnQuad to set actual position
            var decal = new TestWallDecal(_worldSegment.MainRoom, Side.North, new Placement2D(0, 0, 0, 0), _pointLights);
            decal.Place().OnQuad(quad, _random);

            // IMMEDIATE VALIDATION - throw exception if decal overlaps gap
            ValidateDecalPosition(decal, quad);

            _worldSegment.MainRoom.AddChild(decal);
            _placedDecals.Add(decal);

            // Add to rendering system as a stamped shape
            var levelData = _loadedLevelData.FindLevelDataForWorldSegment(_worldSegment);
            _loadedLevelData.AddWallDecal(_worldSegment, decal);


            System.Console.WriteLine($"✓ Decal placed at X={decal.Position.X:F2}");
        }

        _initialized = true;
    }

    private List<WallQuad> ExtractQuadsFromNorthWall()
    {
        var quads = new List<WallQuad>();
        
        // Build room to get triangles
        var shapesAndTriangles = _worldSegment.MainRoom.Build(QualityLevel.Basic);
        if (!shapesAndTriangles.TryGetValue(_worldSegment.MainRoom, out var triangles))
            return quads;

        var northTriangles = triangles.Where(t => t.Side == Side.North).ToArray();
        var processedTriangles = new HashSet<Triangle>();

        foreach (var triangle in northTriangles)
        {
            if (processedTriangles.Contains(triangle))
                continue;

            // Find connected triangle
            var connectedTriangle = northTriangles.FirstOrDefault(t =>
                t != triangle &&
                !processedTriangles.Contains(t) &&
                SharedVertexCount(triangle, t) >= 2);

            if (connectedTriangle != null)
            {
                var quadVertices = GetQuadVertices(triangle, connectedTriangle);
                if (quadVertices != null && quadVertices.Length == 4)
                {
                    var quad = new WallQuad(_worldSegment.MainRoom, Side.North, quadVertices);
                    
                    // Filter out quads that are too small or too close to gap edges
                    if (quad.Width >= 0.5f && quad.Height >= 0.5f)
                    {
                        quads.Add(quad);
                    }
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
        foreach (var v1 in t1.Vertices)
        {
            if (t2.Vertices.Any(v2 => Vector3.Distance(v1, v2) < 0.001f))
                count++;
        }
        return count;
    }

    private Vector3[] GetQuadVertices(Triangle t1, Triangle t2)
    {
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
            return null;

        // Order vertices
        var center = uniqueVertices.Aggregate(Vector3.Zero, (sum, v) => sum + v) / 4f;
        return uniqueVertices.OrderBy(v =>
        {
            var dir = v - center;
            return Math.Atan2(dir.Y, dir.X);
        }).ToArray();
    }
}

/// <summary>
/// Test wall decal (blue square for visibility)
/// </summary>
public class TestWallDecal : WallDecal
{
    public TestWallDecal(Room parentRoom, Side wallSide, Placement2D placement, PointLights pointLights)
        : base(parentRoom, wallSide, placement)
    {
        Width = 0.5f;
        Height = 0.5f;
        MainTexture = new TextureInfo(Color.Blue, TextureKey.Wall);
    }
}
