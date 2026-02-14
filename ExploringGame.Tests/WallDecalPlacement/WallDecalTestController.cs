using ExploringGame.Entities;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.Decals;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
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
    private readonly IGapWorldSegment _gapWorldSegment;
    private readonly WorldSegment _worldSegment;
    private readonly PointLights _pointLights;
    private readonly List<TestWallDecal> _placedDecals = new();
    private readonly Random _random = new(42); // Fixed seed for deterministic tests
    private readonly LoadedLevelData _loadedLevelData;

    private int _decalsToPlace = 100;
    private bool _initialized = false;

    public IReadOnlyList<TestWallDecal> PlacedDecals => _placedDecals;

    public WallDecalTestController(IGapWorldSegment gapWorldSegment, PointLights pointLights, LoadedLevelData loadedLevelData)
    {
        _gapWorldSegment = gapWorldSegment;
        _worldSegment = gapWorldSegment as WorldSegment;
        _pointLights = pointLights;
        _loadedLevelData = loadedLevelData;
    }

    public void Initialize()
    {

    }

    private void ValidateDecalPosition(TestWallDecal decal, WallQuad sourceQuad)
    {
        const float epsilon = 0.0001f; // Tolerance for floating point precision
        
        var gapStart = _gapWorldSegment.GapStartX;
        var gapEnd = _gapWorldSegment.GapEndX;

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
        System.Console.WriteLine($"Gap boundaries: X=[{_gapWorldSegment.GapStartX:F2} to {_gapWorldSegment.GapEndX:F2}]");

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

            // Create decal with dummy center position, then use OnQuad to set actual position
            var decal = new TestWallDecal(_gapWorldSegment.MainRoom, Side.North, Vector2.Zero, _pointLights);
            decal.Place().OnQuad(quad, _random);

            // IMMEDIATE VALIDATION - throw exception if decal overlaps gap
            ValidateDecalPosition(decal, quad);

            _gapWorldSegment.MainRoom.AddChild(decal);
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
        var shapesAndTriangles = _gapWorldSegment.MainRoom.Build(QualityLevel.Basic);
        if (!shapesAndTriangles.TryGetValue(_gapWorldSegment.MainRoom, out var triangles))
            return quads;

        var northTriangles = triangles.Where(t => t.Side == Side.North).ToArray();

        return new QuadExtractor().ExtractQuadsFromTriangles(_gapWorldSegment.MainRoom, Side.North, northTriangles)
            .Where(p => p.Width >= 1.0f && p.Height >= 1.0f).ToList();      
    }
}

/// <summary>
/// Test wall decal (blue square for visibility)
/// </summary>
public class TestWallDecal : WallDecal
{
    public TestWallDecal(Room parentRoom, Side wallSide, Vector2 centerUV, PointLights pointLights)
        : base(parentRoom, wallSide, centerUV)
    {
        Width = 0.5f;
        Height = 0.5f;
        MainTexture = new TextureInfo(Color.Blue, TextureKey.Wall);
    }
}
