using ExploringGame.Entities;
using ExploringGame.Extensions;
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
    private readonly Side _testWallSide;

    private int _decalsToPlace = 100;
    private bool _initialized = false;

    public IReadOnlyList<TestWallDecal> PlacedDecals => _placedDecals;

    public WallDecalTestController(IGapWorldSegment gapWorldSegment, PointLights pointLights, LoadedLevelData loadedLevelData, Side testWallSide = Side.North)
    {
        _gapWorldSegment = gapWorldSegment;
        _worldSegment = gapWorldSegment as WorldSegment;
        _pointLights = pointLights;
        _loadedLevelData = loadedLevelData;
        _testWallSide = testWallSide;
    }

    public void Initialize()
    {

    }

    private void ValidateDecalPosition(TestWallDecal decal, WallQuad sourceQuad)
    {
        const float epsilon = 0.0001f; // Tolerance for floating point precision
        
        var gapStart = _gapWorldSegment.GapStartX;
        var gapEnd = _gapWorldSegment.GapEndX;

        // Determine which axis to check based on wall orientation
        var (axisU, _) = sourceQuad.Side.GetAxisUV();
        float decalCenterU = decal.Position.AxisValue(axisU);
        float decalLeftU = decalCenterU - (decal.Width / 2f);
        float decalRightU = decalCenterU + (decal.Width / 2f);

        // Calculate actual overlap amount
        float overlapAmount = 0f;
        if (decalRightU > gapStart && decalLeftU < gapEnd)
        {
            overlapAmount = Math.Min(decalRightU, gapEnd) - Math.Max(decalLeftU, gapStart);
        }

        // Only fail if overlap exceeds epsilon tolerance
        if (overlapAmount > epsilon)
        {
            // DECAL OVERLAPS GAP - throw detailed exception
            var errorMsg = $"❌ INVALID DECAL PLACEMENT DETECTED!\n" +
                $"Decal Position: {decal.Position}\n" +
                $"Decal U bounds: [{decalLeftU:F2} to {decalRightU:F2}] (axis: {axisU})\n" +
                $"Gap U bounds: [{gapStart:F2} to {gapEnd:F2}]\n" +
                $"Overlap amount: {overlapAmount:F4} (tolerance: {epsilon:F4})\n" +
                $"Source Quad vertices:\n";
            
            for (int i = 0; i < sourceQuad.Vertices.Length; i++)
            {
                errorMsg += $"  V{i}: {sourceQuad.Vertices[i]}\n";
            }
            
            errorMsg += $"Source Quad U range: [{sourceQuad.Vertices.Min(v => v.AxisValue(axisU)):F2} to {sourceQuad.Vertices.Max(v => v.AxisValue(axisU)):F2}]\n";
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

        // Extract quads from test wall
        var quads = ExtractQuadsFromWall(_testWallSide);

        System.Console.WriteLine($"Extracted {quads.Count} quads from {_testWallSide} wall");
        System.Console.WriteLine($"Gap boundaries: U=[{_gapWorldSegment.GapStartX:F2} to {_gapWorldSegment.GapEndX:F2}]");

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
            var (axisU, _) = _testWallSide.GetAxisUV();
            System.Console.WriteLine($"Quad U range: [{quad.Vertices.Min(v => v.AxisValue(axisU)):F2} to {quad.Vertices.Max(v => v.AxisValue(axisU)):F2}] (axis: {axisU})");

            // Create decal with dummy center position, then use OnQuad to set actual position
            var decal = new TestWallDecal(_gapWorldSegment.MainRoom, _testWallSide, Vector2.Zero, _pointLights);
            decal.Place().OnQuad(quad, _random);

            // IMMEDIATE VALIDATION - throw exception if decal overlaps gap
            ValidateDecalPosition(decal, quad);

            _gapWorldSegment.MainRoom.AddChild(decal);
            _placedDecals.Add(decal);

            // Add to rendering system as a stamped shape
            var levelData = _loadedLevelData.FindLevelDataForWorldSegment(_worldSegment);
            _loadedLevelData.AddWallDecal(_worldSegment, decal);

            var (checkAxisU, _) = _testWallSide.GetAxisUV();
            System.Console.WriteLine($"✓ Decal placed at U={decal.Position.AxisValue(checkAxisU):F2} (axis: {checkAxisU})");
        }

        _initialized = true;
    }

    private List<WallQuad> ExtractQuadsFromWall(Side wallSide)
    {
        var quads = new List<WallQuad>();
        
        // Build room to get triangles
        var shapesAndTriangles = _gapWorldSegment.MainRoom.Build(QualityLevel.Basic);
        if (!shapesAndTriangles.TryGetValue(_gapWorldSegment.MainRoom, out var triangles))
            return quads;

        var wallTriangles = triangles.Where(t => t.Side == wallSide).ToArray();

        return new QuadExtractor().ExtractQuadsFromTriangles(_gapWorldSegment.MainRoom, wallSide, wallTriangles)
            .Where(p => p.Width >= 0.6f && p.Height >= 0.6f).ToList();      
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
