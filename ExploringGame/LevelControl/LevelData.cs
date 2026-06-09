using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Logics.Pathfinding;
using ExploringGame.Logics.ShapeControllers;
using ExploringGame.Rendering;
using ExploringGame.Services;
using ExploringGame.Story;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.LevelControl;


public static class LevelDataExtensions
{
    public static TShape FindShape<TShape>(this List<LevelData> levelData, string tag = null)
        where TShape : IShape
    {
        return levelData.FindShapes<TShape>(tag).Single(p => p.Tag == tag);

    }

    public static IEnumerable<TShape> FindShapes<TShape>(this List<LevelData> levelData, string tag = null)
        where TShape : IShape
    {
        return levelData.SelectMany(p => p.WorldSegment.TraverseAllChildren())
            .OfType<TShape>();

    }

    public static IEnumerable<T> FindShapes<T>(this List<LevelData> levelData, Func<T,bool> filter)
    {
        return levelData.SelectMany(p => p.WorldSegment.TraverseAllChildren())
            .OfType<T>()
            .Where(filter);
    }

    public static T FindShape<T>(this List<LevelData> levelData, Func<T, bool> filter)
    {
        return levelData.FindShapes<T>(filter).Single();
    }
}
public class LevelData
{
    public IActiveObject[] ActiveObjects { get; set; } 

    public Dictionary<IRenderPass, List<ShapeBuffer>> BuffersByPass { get; } = new();
    public Dictionary<Type, ShapeBuffer> StampShapeBuffers { get; } = new();
    public List<ShapeBuffer> StampedShapeBuffers { get; } = new();

    // Legacy properties for backward compatibility during migration
    public ShapeBuffer[] ShapeBuffers => BuffersByPass.Values.SelectMany(list => list).Where(b => b.Shape is not ShapeStamp && b.Type == ShapeBufferType.Normal).ToArray();
    
    public bool Initialized { get; private set; }
    public WorldSegment WorldSegment { get; }
    private readonly RenderPassRegistry _renderPassRegistry;

    public LevelData(WorldSegment worldSegment, RenderPassRegistry renderPassRegistry = null)
    {
        WorldSegment = worldSegment;
        Initialized = false;
        _renderPassRegistry = renderPassRegistry;
    }

    private void PopulateBuffers(ShapeBuffer[] allShapeBuffers)
    {
        // Clear existing buffers
        BuffersByPass.Clear();
        StampShapeBuffers.Clear();

        // Separate stamp buffers from regular buffers
        var stampBuffers = allShapeBuffers.Where(b => b.Shape is ShapeStamp).ToList();
        var regularBuffers = allShapeBuffers.Where(b => b.Shape is not ShapeStamp).ToArray();

        // Index stamp buffers by their type
        foreach (var stampBuffer in stampBuffers)
        {
            StampShapeBuffers[stampBuffer.Shape.GetType()] = stampBuffer;
        }

        // Group regular buffers by render pass
        if (_renderPassRegistry != null)
        {
            foreach (var buffer in regularBuffers)
            {
                var pass = _renderPassRegistry.EnvironmentPasses.FirstOrDefault(p => p.ShapeBufferType == buffer.Type);
                if (pass != null)
                {
                    if (!BuffersByPass.ContainsKey(pass))
                        BuffersByPass[pass] = new List<ShapeBuffer>();

                    BuffersByPass[pass].Add(buffer);
                }
            }
        }
        else
        {
            // Fallback: if no registry, use legacy Type-based grouping
            // This supports gradual migration
            foreach (var buffer in regularBuffers)
            {
                // Create a dummy "pass" key - we'll use null for now
                IRenderPass legacyPass = null;
                if (!BuffersByPass.ContainsKey(legacyPass))
                    BuffersByPass[legacyPass] = new List<ShapeBuffer>();

                BuffersByPass[legacyPass].Add(buffer);
            }
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
        ActiveObjects = activeObjects.ToArray();
        PopulateBuffers(allShapeBuffers);
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
        IRoom room = null;
        if (roomGraph != null && stampedShape is IPlaceableObject placeableShape)
        {
            // Find the room containing this position
            room = FindRoomContainingPosition(stampedShape.LocalPosition, roomGraph);
            placeableShape.Room = room;
        }

        // Determine the lighting group for this shape buffer
        var lightingGroup = room?.LightingGroup;

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

    private IRoom FindRoomContainingPosition(Vector3 position, RoomGraph roomGraph)
    {
        // Check each room to see if it contains the position
        foreach (var room in roomGraph.GetAllRooms())
        {
            if (room.ContainsPoint(position))
                return room;
        }

        // If no room contains the point, find the nearest room
        IRoom nearestRoom = null;
        float nearestDistance = float.MaxValue;

        foreach (var room in roomGraph.GetAllRooms())
        {
            var distance = Vector3.DistanceSquared(position, room.LocalPosition);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestRoom = room;
            }
        }

        return nearestRoom;
    }
}
