using ExploringGame.Entities;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.LevelControl;

public class WorldSegmentActivationManager
{
    private readonly LoadedLevelData _loadedLevelData;
    private readonly EntityRoomFinder _entityRoomFinder;
    private readonly Player _player;
    private readonly ServiceContainer _serviceContainer;

    public WorldSegmentActivationManager(LoadedLevelData loadedLevelData, 
        EntityRoomFinder entityRoomFinder, 
        Player player,
        ServiceContainer serviceContainer)
    {
        _loadedLevelData = loadedLevelData;
        _entityRoomFinder = entityRoomFinder;
        _player = player;
        _serviceContainer = serviceContainer;
    }

    public void Update()
    {
        UpdateActiveSegments(_player.Position);
    }

    public void ActivateSegmentAndNeighbors(WorldSegment worldSegment)
    {
        // If the current segment matches the first active segment, we're already set up correctly
        if (_loadedLevelData.ActiveSegments.Count > 0 && 
            _loadedLevelData.ActiveSegments[0].WorldSegment == worldSegment)
            return;

        // Clear and rebuild ActiveSegments
        _loadedLevelData.ActiveSegments.Clear();

        // PHASE 1: Create all segments (children are created in constructors)
        var segmentsToActivate = new List<WorldSegment> { worldSegment };

        foreach (var transition in worldSegment.Transitions)
        {
            var neighborSegment = _loadedLevelData.LoadedSegments
                .Select(ld => ld.WorldSegment)
                .FirstOrDefault(ws => ws.GetType() == transition.WorldSegmentType) 
                ?? Activator.CreateInstance(transition.WorldSegmentType) as WorldSegment;

            segmentsToActivate.Add(neighborSegment);
        }

        // Activate all segments (loads geometry but doesn't position cross-segment dependencies)
        foreach (var segment in segmentsToActivate)
        {
            ActivateSegment(segment);
        }

        // PHASE 2: Position children now that all shapes exist
        var loadedSegments = _loadedLevelData.LoadedSegments.Select(ld => ld.WorldSegment).ToList();
        foreach (var segment in segmentsToActivate)
        {
            segment.PositionChildren(loadedSegments);
        }
    }

    private void UpdateActiveSegments(Vector3 playerPosition)
    {
        // Find the room containing the player, then get its WorldSegment
        var currentRoom = _entityRoomFinder.FindRoom(playerPosition);
        if (currentRoom == null)
            return;

        var currentSegment = currentRoom.WorldSegment;
        if (currentSegment == null)
            return;

        // Activate this segment and its neighbors
        ActivateSegmentAndNeighbors(currentSegment);
    }

    private void ActivateSegment(GeometryBuilder.Shapes.WorldSegments.WorldSegment worldSegment)
    {
        // Load the segment if not already loaded
        _loadedLevelData.LoadSegment(worldSegment);

        // Add to active segments if not already active
        var levelData = _loadedLevelData.FindLevelDataForWorldSegment(worldSegment);
        if (levelData != null && !_loadedLevelData.ActiveSegments.Contains(levelData))
        {
            _loadedLevelData.ActiveSegments.Add(levelData);
        }
    }
}
