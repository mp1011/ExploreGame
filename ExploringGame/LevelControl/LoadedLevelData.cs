using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics.Collision;
using ExploringGame.Logics.Pathfinding;
using ExploringGame.Rendering;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.LevelControl;

public class LoadedLevelData
{
    private readonly Game _game;
    private readonly ServiceContainer _serviceContainer;
    private readonly SetupColliderBodies _setupColliderBodies;
    private readonly Physics _physics;
    private readonly LoadedTextureSheets _loadedTextureSheets;
    private readonly RoomLightingCalculator _lightingCalculator;
    private readonly WorldSegmentAnchorProcessor _anchorProcessor;

    public List<LevelData> LoadedSegments { get; } = new();
    public List<LevelData> ActiveSegments { get; } = new();
    public RoomGraph RoomGraph { get; private set; }
    public WaypointGraph WaypointGraph { get; private set; }
    public RoomLightingCalculator LightingCalculator => _lightingCalculator;
    public ShapeBuffer SkyboxBuffer { get; set; }

    public LoadedLevelData(Game game, SetupColliderBodies setupColliderBodies, Physics physics, 
        LoadedTextureSheets loadedTextureSheets, ServiceContainer serviceContainer, 
        RoomLightingCalculator lightingCalculator,
        WorldSegmentAnchorProcessor anchorProcessor)
    {
        _game = game;
        _physics = physics;
        _loadedTextureSheets = loadedTextureSheets;
        _serviceContainer = serviceContainer;
        _setupColliderBodies = setupColliderBodies;
        _lightingCalculator = lightingCalculator;
        _anchorProcessor = anchorProcessor;

        RoomGraph = new RoomGraph();
        WaypointGraph = new WaypointGraph(RoomGraph);
    }

    public void Update(GameTime gameTime)
    {
        foreach(var segment in ActiveSegments)
            segment.Update(gameTime);
    }

    public void LoadSegment(WorldSegment worldSegment)
    {
        // Check if segment is already loaded
        if (IsSegmentLoaded(worldSegment))
            return;

        // Build room graph for this segment
        var rooms = worldSegment.TraverseAllChildren().OfType<Room>().ToList();
        foreach (var room in rooms)
        {
            RoomGraph.AddRoom(room);

            foreach (var connection in room.RoomConnections)
            {
                var connectedRoom = connection.GetOtherRoom(room);
                if (connectedRoom != null)
                {
                    RoomGraph.AddConnection(room, connectedRoom);
                }
            }
        }

        // Add waypoints for this segment
        WaypointGraph.AddRoomsAndWaypoints(rooms, worldSegment);

        // Update lighting calculator
        _lightingCalculator.SetRoomGraph(RoomGraph);
        _lightingCalculator.AddSegments(new List<WorldSegment> { worldSegment });

        // Build geometry and create level data
        var triangles = worldSegment.Build((QualityLevel)8); //todo, quality level

        AssignRoomsToPlaceableShapes(worldSegment);

        var shapeBufferCreator = new ShapeBufferCreator(triangles, _loadedTextureSheets, _game.GraphicsDevice);
        var shapeBuffers = shapeBufferCreator.Execute();
        var activeObjects = _serviceContainer.CreateControllers(worldSegment.TraverseAllChildren());

        // Build skybox if this segment has one and we don't already have a skybox
        if (worldSegment.Skybox != null && SkyboxBuffer == null)
        {
            var skyboxTriangles = worldSegment.Skybox.Build((QualityLevel)8);
            var skyboxBufferCreator = new ShapeBufferCreator(skyboxTriangles, _loadedTextureSheets, _game.GraphicsDevice);
            SkyboxBuffer = skyboxBufferCreator.CreateSkyboxBuffer(worldSegment.Skybox);
        }

        var newLevelData = new LevelData(worldSegment, shapeBuffers, activeObjects);
        _setupColliderBodies.Execute(newLevelData.WorldSegment);
        newLevelData.Initialize();

        // Create grass renderer if this segment contains a FrontYard
        var frontYard = worldSegment.TraverseAllChildren().OfType<FrontYard>().FirstOrDefault();
        if (frontYard != null)
        {
            newLevelData.GrassRenderer = new GrassRenderer(_game.GraphicsDevice, _game.Content, frontYard);
        }

        LoadedSegments.Add(newLevelData);
    }

    private bool IsSegmentLoaded(WorldSegment worldSegment)
    {
        return LoadedSegments.Any(ld => ld.WorldSegment.GetType() == worldSegment.GetType());
    }

    private void AssignRoomsToPlaceableShapes(WorldSegment segment)
    {
        // Find all PlaceableShapes in the segments
        var placeableShapes = segment.TraverseAllChildren()
            .OfType<PlaceableShape>()
            .Where(p => p.ViewFrom != ViewFrom.None)
            .ToList();

        foreach (var shape in placeableShapes)
        {
            var room = FindRoomContainingPosition(shape.Position);

            // Use the LightingGroup for consistency (RoomParts point to their parent room)
            shape.Room = room?.LightingGroup;
        }
    }

    private Room FindRoomContainingPosition(Vector3 position)
    {
        // Check each room to see if it contains the position
        foreach (var room in RoomGraph.GetAllRooms())
        {
            if (room.ContainsPoint(position))
                return room;
        }

        // If no room contains the point, find the nearest room
        Room nearestRoom = null;
        float nearestDistance = float.MaxValue;

        foreach (var room in RoomGraph.GetAllRooms())
        {
            var distance = Vector3.DistanceSquared(position, room.Position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestRoom = room;
            }
        }

        return nearestRoom;
    }

    public LevelData FindLevelDataForWorldSegment(WorldSegment worldSegment)
    {
        return LoadedSegments.FirstOrDefault(ld => ld.WorldSegment == worldSegment);
    }

    public void AddStampedShape<TStamp>(WorldSegment worldSegment, StampedShape<TStamp> stampedShape)
        where TStamp : ShapeStamp
    {
        var levelData = FindLevelDataForWorldSegment(worldSegment);
        if (levelData == null)
        {
            throw new InvalidOperationException($"WorldSegment not found in loaded segments");
        }

        levelData.AddStampedShape(stampedShape, RoomGraph, _lightingCalculator);
    }

    public void AddWallDecal(WorldSegment worldSegment, GeometryBuilder.Shapes.Decals.WallDecal wallDecal)
    {
        AddStampedShape<GeometryBuilder.Shapes.Decals.WallDecalStamp>(worldSegment, wallDecal);
    }
}

