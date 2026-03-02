using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics.Collision;
using ExploringGame.Logics.Pathfinding;
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

    public List<LevelData> LoadedSegments { get; } = new();
    public RoomGraph RoomGraph { get; private set; }
    public RoomLightingCalculator LightingCalculator => _lightingCalculator;

    public LoadedLevelData(Game game, SetupColliderBodies setupColliderBodies, Physics physics, 
        LoadedTextureSheets loadedTextureSheets, ServiceContainer serviceContainer, 
        RoomLightingCalculator lightingCalculator)
    {
        _game = game;
        _physics = physics;
        _loadedTextureSheets = loadedTextureSheets;
        _serviceContainer = serviceContainer;
        _setupColliderBodies = setupColliderBodies;
        _lightingCalculator = lightingCalculator;
    }

    public void Update(GameTime gameTime)
    {
        foreach(var segment in LoadedSegments)
            segment.Update(gameTime);
    }

    public void LoadSegment(WorldSegment worldSegment)
    {
        List<WorldSegment> addedSegments = new();
        addedSegments.Add(worldSegment);
        _serviceContainer.BindSingleton(worldSegment, worldSegment.GetType());

        foreach (var transition in worldSegment.Transitions)
        {
            var nextSegment = _serviceContainer.Get(transition.WorldSegmentType) as WorldSegment;
            addedSegments.Add(nextSegment);
            _serviceContainer.BindSingleton(nextSegment, nextSegment.GetType());
        }

        BuildRoomGraph(addedSegments);

        // Initialize lighting with the room graph and segments
        _lightingCalculator.SetRoomGraph(RoomGraph);
        _lightingCalculator.AddSegments(addedSegments);

      
        foreach (var addedSegment in addedSegments)
        {
            // Create waypoint graph before building so DebugMarkers are included
            addedSegment.WaypointGraph = new WaypointGraph(addedSegment, RoomGraph);

            var triangles = addedSegment.Build((QualityLevel)8); //todo, quality level

            AssignRoomsToPlaceableShapes(addedSegment);

            var shapeBuffers = new ShapeBufferCreator(triangles, _loadedTextureSheets, _game.GraphicsDevice).Execute();
            var activeObjects = _serviceContainer.CreateControllers(addedSegment.TraverseAllChildren());

            var newLevelData = new LevelData(addedSegment, shapeBuffers, activeObjects);
            _setupColliderBodies.Execute(newLevelData.WorldSegment);
            newLevelData.Initialize();

            LoadedSegments.Add(newLevelData);
        }
    }

    private void BuildRoomGraph(List<WorldSegment> segments)
    {
        if (RoomGraph == null)
        {
            RoomGraph = new RoomGraph();
        }

        foreach (var segment in segments)
        {
            var rooms = segment.TraverseAllChildren().OfType<Room>().ToList();

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
        }
    }

    private void AssignRoomsToPlaceableShapes(WorldSegment segment)
    {
        // Find all PlaceableShapes in the segments
        var placeableShapes = segment.TraverseAllChildren()
            .OfType<PlaceableShape>()
            .ToList();

        foreach (var shape in placeableShapes)
        {
            // Use the same logic as AddStampedShape to find the room containing this shape
            var room = FindRoomContainingPosition(shape.Position, RoomGraph);

            // Use the LightingGroup for consistency (RoomParts point to their parent room)
            shape.Room = room?.LightingGroup;
        }
    }

    private Room FindRoomContainingPosition(Vector3 position, RoomGraph roomGraph)
    {
        // Check each room to see if it contains the position
        foreach (var room in roomGraph.GetAllRooms())
        {
            if (room.ContainsPoint(position))
                return room;
        }

        // If no room contains the point, find the nearest room
        Room nearestRoom = null;
        float nearestDistance = float.MaxValue;

        foreach (var room in roomGraph.GetAllRooms())
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

    public void SwapActive()
    {
        //if (Next == null)
        //    return;

        //if (Current != null)
        //{
        //    Current.Stop();

        //    foreach (var body in Current.WorldSegment.TraverseAllChildren()
        //                                             .Where(p=>p.ColliderBodies != null)
        //                                             .SelectMany(p=>p.ColliderBodies))
        //    {                                                      
        //        _physics.Remove(body);
        //    }
        //}

        //Current = Next;

        //_setupColliderBodies.Execute(Current.WorldSegment);
        //Current.Initialize();

        //Next = null;
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

