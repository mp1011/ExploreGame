using ExploringGame.Entities;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Services;
using Microsoft.Xna.Framework;

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

        // Add current segment first
        ActivateSegment(worldSegment);

        // Add all neighboring segments
        foreach (var transition in worldSegment.Transitions)
        {
            // Get or create the neighbor segment from the ServiceContainer
            var neighborSegment = _serviceContainer.Get(transition.WorldSegmentType) as GeometryBuilder.Shapes.WorldSegments.WorldSegment;
            if (neighborSegment != null)
            {
                ActivateSegment(neighborSegment);
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
