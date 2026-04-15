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

    /// <summary>
    /// The render pass registry used when building level data.
    /// Must be set (from Game1.LoadContent) before the first segment loads.
    /// </summary>
    public RenderPassRegistry RenderPassRegistry { get; set; }

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
        foreach (var segment in ActiveSegments)
            segment.Update(gameTime);
    }

    public void LoadSegment(WorldSegment worldSegment)
    {
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

        WaypointGraph.AddRoomsAndWaypoints(rooms, worldSegment);

        _lightingCalculator.SetRoomGraph(RoomGraph);
        _lightingCalculator.AddSegments(new List<WorldSegment> { worldSegment });

        var triangles = worldSegment.Build((QualityLevel)8);

        AssignRoomsToPlaceableShapes(worldSegment);

        var shapeBufferCreator = new ShapeBufferCreator(triangles, _loadedTextureSheets,
            _game.GraphicsDevice, registry: RenderPassRegistry);
        var shapeBuffers = shapeBufferCreator.Execute();

        // Build skybox buffer via the registry when this segment has a skybox and
        // no segment has been given one yet. The skybox shape is not a child of the
        // WorldSegment tree, so ShapeBufferCreator won't process it automatically.
        if (worldSegment.Skybox != null && RenderPassRegistry != null)
        {
            var skyboxPass = RenderPassRegistry.FindSpecializedPassForShape(worldSegment.Skybox);
            bool alreadyHasSkybox = skyboxPass != null && LoadedSegments.Any(s =>
                s.BuffersByPass.TryGetValue(skyboxPass, out var list) && list.Count > 0);

            if (!alreadyHasSkybox && skyboxPass != null)
            {
                var skyboxTriangles = worldSegment.Skybox.Build((QualityLevel)8);
                var skyboxBuffer = skyboxPass.BuildBuffer(worldSegment.Skybox, skyboxTriangles,
                    _loadedTextureSheets, _game.GraphicsDevice);
                if (skyboxBuffer != null)
                    shapeBuffers = shapeBuffers.Append(skyboxBuffer).ToArray();
            }
        }

        var activeObjects = _serviceContainer.CreateControllers(worldSegment.TraverseAllChildren());

        var newLevelData = new LevelData(worldSegment, shapeBuffers, activeObjects, RenderPassRegistry);
        _setupColliderBodies.Execute(newLevelData.WorldSegment);
        newLevelData.Initialize();

        LoadedSegments.Add(newLevelData);
    }

    private bool IsSegmentLoaded(WorldSegment worldSegment)
    {
        return LoadedSegments.Any(ld => ld.WorldSegment.GetType() == worldSegment.GetType());
    }

    private void AssignRoomsToPlaceableShapes(WorldSegment segment)
    {
        var placeableShapes = segment.TraverseAllChildren()
            .OfType<PlaceableShape>()
            .Where(p => p.ViewFrom != ViewFrom.None)
            .ToList();

        foreach (var shape in placeableShapes)
        {
            var room = FindRoomContainingPosition(shape.Position);
            shape.Room = room?.LightingGroup;
        }
    }

    private Room FindRoomContainingPosition(Vector3 position)
    {
        foreach (var room in RoomGraph.GetAllRooms())
        {
            if (room.ContainsPoint(position))
                return room;
        }

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
