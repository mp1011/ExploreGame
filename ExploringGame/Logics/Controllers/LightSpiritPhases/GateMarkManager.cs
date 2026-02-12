using ExploringGame.Entities;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Rendering;
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
    private readonly HashSet<(Room room, Side side)> _usedWalls = new();
    private readonly Random _random = new();

    public IReadOnlyList<GateMark> GateMarks => _gateMarks;

    public GateMarkManager(WorldSegment worldSegment, LoadedLevelData loadedLevelData, PointLights pointLights)
    {
        _worldSegment = worldSegment;
        _loadedLevelData = loadedLevelData;
        _pointLights = pointLights;
    }

    public GateMark SpawnGateMark()
    {
        // Find all eligible walls
        var eligibleWalls = FindEligibleWalls();

        if (eligibleWalls.Count == 0)
            return null;

        // Pick a random eligible wall
        var wall = eligibleWalls[_random.Next(eligibleWalls.Count)];
        
        // Mark this wall as used
        _usedWalls.Add((wall.room, wall.side));

        // Create placement (0.5x0.5 square, centered on wall)
        var wallWidth = wall.side == Side.North || wall.side == Side.South ? wall.room.Width : wall.room.Depth;
        var wallHeight = wall.room.Height;
        
        // Center the gatemark on the wall
        float left = (wallWidth - 0.5f) / 2f;
        float bottom = (wallHeight - 0.5f) / 2f;
        var placement = new Placement2D(left, bottom + 0.5f, left + 0.5f, bottom);

        // Create the gatemark
        var gateMark = new GateMark(wall.room, wall.side, placement, _pointLights);
        wall.room.AddChild(gateMark);
        _gateMarks.Add(gateMark);

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
        
        // Note: We don't remove from _usedWalls so the same wall won't be used again
    }

    private List<(Room room, Side side)> FindEligibleWalls()
    {
        var eligible = new List<(Room room, Side side)>();
        var rooms = _worldSegment.TraverseAllChildren().OfType<Room>();

        foreach (var room in rooms)
        {
            foreach (Side side in new[] { Side.North, Side.South, Side.East, Side.West })
            {
                // Check if already used
                if (_usedWalls.Contains((room, side)))
                    continue;

                // Check if wall is large enough (at least 0.5x0.5)
                var wallWidth = side == Side.North || side == Side.South ? room.Width : room.Depth;
                var wallHeight = room.Height;

                if (wallWidth >= 0.5f && wallHeight >= 0.5f)
                {
                    eligible.Add((room, side));
                }
            }
        }

        return eligible;
    }
}
