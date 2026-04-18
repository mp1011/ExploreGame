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

    public void ActivateGroup(WorldSegmentGroup worldSegmentGroup)
    {
        _loadedLevelData.ActiveSegments.Clear();

        var allSegments = worldSegmentGroup.CreateSegments().ToArray();

        // Filter to only process new segments that haven't been loaded yet
        var newSegments = allSegments
            .Where(segment => !_loadedLevelData.IsSegmentLoaded(segment))
            .ToList();

        // Only process new segments through the three phases
        if (newSegments.Any())
        {
            // PHASE 1: Create segment instances (constructors only - no geometry)
            foreach (var segment in newSegments)
            {
                _loadedLevelData.LoadSegment(segment);
            }

            // PHASE 2: Position children and set dependencies
            var loadedSegments = _loadedLevelData.LoadedSegments.Select(ld => ld.WorldSegment).ToList();
            foreach (var segment in newSegments)
            {
                segment.PositionChildren(loadedSegments);
            }

            // PHASE 3: Build geometry buffers now that everything is positioned
            foreach (var segment in newSegments)
            {
                _loadedLevelData.BuildSegmentGeometry(segment);
            }
        }

        // Add all segments (both new and already-loaded) to active segments
        foreach (var segment in allSegments)
        {
            var levelData = _loadedLevelData.FindLevelDataForWorldSegment(segment);
            if (levelData != null && !_loadedLevelData.ActiveSegments.Contains(levelData))
            {
                _loadedLevelData.ActiveSegments.Add(levelData);
            }
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

        // placeholder for when we have segment transitions
    }
}
