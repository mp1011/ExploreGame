using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Logics.Pathfinding;
using ExploringGame.Logics.ShapeControllers;
using ExploringGame.Rendering;
using ExploringGame.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.LevelControl;

public class LevelData
{
    public IActiveObject[] ActiveObjects { get; set; } 

    public ShapeBuffer[] ShapeBuffers { get; set; }
    public Dictionary<Type, ShapeBuffer> StampShapeBuffers { get; } = new();
    public List<ShapeBuffer> StampedShapeBuffers { get; } = new();

    public List<ShapeBuffer> GrassShapeBuffers { get; } = new();

    public bool Initialized { get; private set; }
    public WorldSegment WorldSegment { get; }

    public LevelData(WorldSegment worldSegment, ShapeBuffer[] allShapeBuffers, IActiveObject[] activeObjects)
    {
        WorldSegment = worldSegment;
        ActiveObjects = activeObjects.ToArray();
        Initialized = false;

        // Separate stamp buffers and grass buffers from regular buffers
        var stampBuffers = allShapeBuffers.Where(b => b.Shape is ShapeStamp).ToList();
        GrassShapeBuffers.AddRange(allShapeBuffers.Where(p => p.Type == ShapeBufferType.Grass));

        ShapeBuffers = allShapeBuffers.Where(b => b.Shape is not ShapeStamp && b.Type == ShapeBufferType.Normal).ToArray();
        
        // Index stamp buffers by their type
        foreach (var stampBuffer in stampBuffers)
        {
            StampShapeBuffers[stampBuffer.Shape.GetType()] = stampBuffer;
        }
    }

    public void Initialize()
    {
        if (Initialized)
            return;

        foreach (var obj in ActiveObjects)
            obj.Initialize();

        Initialized = true;        
    }

    public void SetBuffers(ShapeBuffer[] allShapeBuffers, IActiveObject[] activeObjects)
    {
        // Clear existing buffers
        StampShapeBuffers.Clear();
        GrassShapeBuffers.Clear();

        ActiveObjects = activeObjects.ToArray();

        // Separate stamp buffers and grass buffers from regular buffers
        var stampBuffers = allShapeBuffers.Where(b => b.Shape is ShapeStamp).ToList();
        GrassShapeBuffers.AddRange(allShapeBuffers.Where(p => p.Type == ShapeBufferType.Grass));

        ShapeBuffers = allShapeBuffers.Where(b => b.Shape is not ShapeStamp && b.Type == ShapeBufferType.Normal).ToArray();

        // Index stamp buffers by their type
        foreach (var stampBuffer in stampBuffers)
        {
            StampShapeBuffers[stampBuffer.Shape.GetType()] = stampBuffer;
        }
    }

    public void Stop()
    {
        if (!Initialized)
            return;

        foreach (var obj in ActiveObjects)
            obj.Stop();

        Initialized = false;
    }
    
    public void Update(GameTime gameTime)
    {
        foreach (var obj in ActiveObjects)
            obj.Update(gameTime);
    }

    public void AddStampedShape<TStamp>(StampedShape<TStamp> stampedShape, 
        RoomGraph roomGraph = null, 
        RoomLightingCalculator lightingCalculator = null) 
        where TStamp : ShapeStamp
    {
        var stampType = typeof(TStamp);

        if (!StampShapeBuffers.TryGetValue(stampType, out var stampBuffer))
        {
            throw new InvalidOperationException($"No ShapeStamp of type {stampType.Name} found in this WorldSegment");
        }

        // Detect which room the stamped shape is in
        Room room = null;
        if (roomGraph != null && stampedShape is IPlaceableObject placeableShape)
        {
            // Find the room containing this position
            room = FindRoomContainingPosition(stampedShape.Position, roomGraph);
            placeableShape.Room = room;
        }

        // Determine the lighting group for this shape buffer
        Room lightingGroup = room?.LightingGroup;

        // Create a new ShapeBuffer that points to the stamped shape
        // but uses the vertex/index buffers from the stamp
        var stampedBuffer = new ShapeBuffer(
            stampedShape,
            stampBuffer.VertexBuffer,
            stampBuffer.IndexBuffer,
            stampBuffer.TriangleCount,
            stampBuffer.Texture,
            stampBuffer.RasterizerState,
            lightingGroup
        );

        StampedShapeBuffers.Add(stampedBuffer);
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
}
