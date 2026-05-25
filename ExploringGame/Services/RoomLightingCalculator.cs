using ExploringGame.Extensions;
using ExploringGame.GameDebug;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;
using ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;
using ExploringGame.GeometryBuilder.Shapes.Structures;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Logics.Pathfinding;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ExploringGame.Services;

/// <summary>
/// Calculates room lighting based on light sources and room connectivity
/// </summary>
public class RoomLightingCalculator
{
    private const float MinimumContribution = 0.01f;
    private const float DoorClosedScale = 0.1f;
    private const float DoorOpenScale = 1.0f;

    private RoomGraph _roomGraph;
    private AnnotatedGraph<RoomLightData> _roomLightGraph;
    private readonly List<ILightSource> _allLightSources = new();

    public ILightingGroup[] LightGroups { get; private set; }

    public AnnotatedGraph<RoomLightData> RoomLightGraph => _roomLightGraph;

    /// <summary>
    /// Set the room graph and initialize the lighting data
    /// </summary>
    public void SetRoomGraph(RoomGraph roomGraph)
    {
        _roomGraph = roomGraph;
        
        if(_roomLightGraph == null || !_roomLightGraph.HasRoomGraph(roomGraph))
            _roomLightGraph = new AnnotatedGraph<RoomLightData>(roomGraph);

        LightGroups = _roomGraph.GetAllRooms()
            .Select(r => r.LightingGroup)
            .Where(g => g != null)
            .Distinct()
            .ToArray();

        foreach (var lightingGroup in LightGroups.OfType<Room>())
        {
            if(_roomLightGraph.Get(lightingGroup) == null)
                _roomLightGraph.Add(lightingGroup, new RoomLightData(lightingGroup));
        }
    }

    /// <summary>
    /// Add segments to the lighting system and calculate initial lighting
    /// </summary>
    public void AddSegments(List<WorldSegment> segments)
    {
        // Find all light sources in the segments
        foreach (var segment in segments)
        {
            var lightSources = segment.TraverseAllChildren().OfType<ILightSource>().ToList();

            foreach (var light in lightSources)
            {
                if (!_allLightSources.Contains(light))
                {
                    _allLightSources.Add(light);

                    // Subscribe to state changes
                    light.StateChanged += OnLightStateChanged;
                }
            }

            // Find all doors and subscribe to position changes
            var doors = segment.TraverseAllChildren().OfType<Door>().ToList();
            foreach (var door in doors)
            {
                door.PositionChanged += OnDoorPositionChanged;
            }
        }

        // Calculate initial lighting for all rooms
        foreach (var room in _roomGraph.GetAllRooms())
        {
            RecalculateLightContributions(room, _allLightSources);
        }

        foreach(var lightData in _roomLightGraph.GetAllAnnotations())
        {
            lightData.RecalculateLightLevel();
        }
    }

    private void OnLightStateChanged(object sender, LightStateChangedEventArgs e)
    {
        if (sender is ILightSource lightSource)
        {
            RecalculateLightContributions(lightSource);

            foreach(var lightData in _roomLightGraph.GetAllAnnotations())
            {
                lightData.RecalculateLightLevel();
            }
        }
    }
    
    public float RecalculateRoomLight(Room room)
    {
        RecalculateLightContributions(room, _allLightSources);
        return _roomLightGraph.Get(room).RecalculateLightLevel();
    }

    private void OnDoorPositionChanged(object sender, EventArgs e)
    {
        foreach (var room in _roomGraph.GetAllRooms())
        {
            ExploringGame.GameDebug.Debug.Message(room is UpstairsHall, ".");

            RecalculateLightContributions(room, _allLightSources);
        }

        foreach(var lightData in _roomLightGraph.GetAllAnnotations())
        {
            lightData.RecalculateLightLevel();
        }
    }

    /// <summary>
    /// Calculate light contribution from a specific light source to a specific room
    /// </summary>
    public LightContribution CalculateLightContribution(ILightSource lightSource, IRoom targetRoom)
    {
        GameDebug.Debug.Message(lightSource.On && targetRoom is Kitchen && lightSource.Room is BackDeck, "TEST");
        
        if (!lightSource.On)
            return new LightContribution(lightSource, 0);

        // Find the room containing this light
        var lightRoom = lightSource.Room;
        if (lightRoom == null)
            return new LightContribution(lightSource, 0);

        // If light is in the same lighting group, full contribution
        if (lightRoom.LightingGroup == targetRoom.LightingGroup)
            return new LightContribution(lightSource, lightSource.Intensity); 

        // Find path from light's room to target room
        var roomPath = _roomGraph.FindPath(lightRoom, targetRoom);
        if (roomPath == null || roomPath.Count == 0)
            return new LightContribution(lightSource, 0);

        // Walk the path and calculate decay
        // Only apply decay when crossing between different lighting groups
        float contribution = lightSource.Intensity;
        float graphDistance = 0;

        Vector3 pos = lightSource.LightPosition;
        Vector3 lastDir = Vector3.Zero;

        var currentLightingGroup = lightRoom.LightingGroup;
        var doorDistanceModifier = 1.0f;

        for (int i = 0; i < roomPath.Count - 1; i++)
        {
            var currentRoom = roomPath[i];
            var nextRoom = roomPath[i + 1];
            var nextLightingGroup = nextRoom.LightingGroup;

            // Only apply decay if we're moving to a NEW lighting group
            if (nextLightingGroup != currentLightingGroup)
            {
                // Find the connection between these rooms
                var connection = FindConnection(currentRoom, nextRoom);
                if (connection == null)
                    continue;

                // Calculate decay based on connection size vs wall size
                float decayFactor = CalculateDecayFactor(currentRoom, nextRoom, connection);
                contribution *= decayFactor;

                // Check for door and apply door scaling
                var door = FindDoor(currentRoom);
                if (door != null)
                {
                    float doorScale = door.Open ? DoorOpenScale : DoorClosedScale;
                    contribution *= doorScale;

                    doorDistanceModifier = door.Open ? 1.0f : 3.0f;
                }
                else
                    doorDistanceModifier = 1.0f;

                // Stop if contribution is too small
                if (contribution < MinimumContribution)
                    return new LightContribution(lightSource, 0);

                graphDistance += (pos.DistanceTo(nextLightingGroup.Position) * 1.3f * doorDistanceModifier);
                pos = nextLightingGroup.Position;

                // lastDir = (nextLightingGroup.Position - currentLightingGroup.Position);
                lastDir = (currentLightingGroup.Position - nextLightingGroup.Position);

                lastDir.Normalize();

                currentLightingGroup = nextLightingGroup;
            }
        }

        LightingDebugger.Check(lightSource, targetRoom);

        var directDistance = lightSource.LightPosition.DistanceTo(targetRoom.Position);
        if (directDistance > graphDistance)
            return new LightContribution(lightSource, contribution);
        else 
            return new LightContribution(lightSource, graphDistance, targetRoom.Position, lastDir, contribution);
    }

    /// <summary>
    /// Determines the contribution of each light towards this room
    /// </summary>
    private void RecalculateLightContributions(IRoom room, IEnumerable<ILightSource> allLights)
    {
        if (!_roomLightGraph.TryGet(room, out var lightData))
            return;

        foreach (var light in allLights)
        {
            var contribution = CalculateLightContribution(light, room);
            if (contribution.Amount > 0)
                lightData.AddLightContribution(contribution);
            else
                lightData.RemoveLightContribution(light);
        }
    }

    /// <summary>
    /// Recalculate all rooms affected by a specific light source
    /// </summary>
    public void RecalculateLightContributions(ILightSource lightSource)
    {
        foreach (var room in _roomGraph.GetAllRooms())
        {
            if (_roomLightGraph.TryGet(room, out var lightData))
            {
                var contribution = CalculateLightContribution(lightSource, room);
                if (contribution.Amount > 0)
                    lightData.AddLightContribution(contribution);
                else
                    lightData.RemoveLightContribution(lightSource);
            }
        }
    }

    private float CalculateDecayFactor(IRoom from, IRoom to, RoomConnection connection)
    {
        var distance = from.Position.DistanceTo(to.Position);

        float c = 1.0f;
        float l = 0.16f;
        float q = 0.064f;

        //float l = 0.09f;
        //float q = 0.032f;

        var factor = 1.0f / (c + (l * distance) + (q * distance * distance));

        return System.Math.Max(0.3f, factor);
    }

    private RoomConnection FindConnection(IRoom room1, IRoom room2)
    {
        return room1.RoomConnections.FirstOrDefault(rc =>
            rc.GetOtherRoom(room1) == room2);
    }

    private Door FindDoor(IRoom room) =>    
        room.TraverseAllChildren().OfType<Door>().FirstOrDefault();
    
    private IRoom FindRoomContainingPoint(Vector3 position)
    {
        foreach (var room in _roomGraph.GetAllRooms())
        {
            if (room.ContainsPoint(position))
                return room;
        }
        return null;
    }

    /// <summary>
    /// Gets RoomLightData for a specific lighting group.
    /// </summary>
    public RoomLightData GetLightDataForGroup(ILightingGroup lightingGroup)
    {
        if (lightingGroup is Room r)
            return _roomLightGraph.Get(r);

        if (lightingGroup == null)
            return null;

        throw new System.NotImplementedException("check me"); 
    }

    /// <summary>
    /// Replaces a placeholder room with a real room in the lighting calculator.
    /// </summary>
    public void ReplaceRoom(Room oldRoom, Room newRoom)
    {
        _roomLightGraph.ReplaceKey(oldRoom, newRoom);
    }
}
