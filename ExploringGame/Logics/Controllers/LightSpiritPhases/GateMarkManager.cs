using ExploringGame.Entities;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;
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
    private readonly Random _random;

    public IReadOnlyList<GateMark> GateMarks => _gateMarks;

    public GateMarkManager(WorldSegment worldSegment, LoadedLevelData loadedLevelData, PointLights pointLights, Random random)
    {
        _worldSegment = worldSegment;
        _loadedLevelData = loadedLevelData;
        _pointLights = pointLights;
        _random = random;
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

                var quads = new QuadExtractor().ExtractQuadsFromTriangles(room, side, sideTriangles)
                    .Where(q => q.Width >= 1.0f && q.Height >= 1.0f);

                _availableQuads.AddRange(quads);
            }
        }
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
        var gateMark = new GateMark(quad.Room, quad.Side, Vector2.Zero, _pointLights);
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

