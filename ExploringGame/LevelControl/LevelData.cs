using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Logics.ShapeControllers;
using ExploringGame.Rendering;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.LevelControl;

public class LevelData
{
    public IActiveObject[] ActiveObjects { get; } 

    public ShapeBuffer[] ShapeBuffers { get; private set; }
    public Dictionary<Type, ShapeBuffer> StampShapeBuffers { get; } = new();
    public List<ShapeBuffer> StampedShapeBuffers { get; } = new();

    public bool Initialized { get; private set; }
    public WorldSegment WorldSegment { get; }

    public LevelData(WorldSegment worldSegment, ShapeBuffer[] allShapeBuffers, IActiveObject[] activeObjects)
    {
        WorldSegment = worldSegment;
        ActiveObjects = activeObjects.OrderBy(p => p is SegmentTransitionController ? 1 : 0).ToArray();
        Initialized = false;
        
        // Separate stamp buffers from regular buffers
        var stampBuffers = allShapeBuffers.Where(b => b.Shape is ShapeStamp).ToList();
        ShapeBuffers = allShapeBuffers.Where(b => b.Shape is not ShapeStamp).ToArray();
        
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

    public void AddStampedShape<TStamp>(StampedShape<TStamp> stampedShape) 
        where TStamp : ShapeStamp
    {
        var stampType = typeof(TStamp);
        
        if (!StampShapeBuffers.TryGetValue(stampType, out var stampBuffer))
        {
            throw new InvalidOperationException($"No ShapeStamp of type {stampType.Name} found in this WorldSegment");
        }       

        // Create a new ShapeBuffer that points to the stamped shape
        // but uses the vertex/index buffers from the stamp
        var stampedBuffer = new ShapeBuffer(
            stampedShape,
            stampBuffer.VertexBuffer,
            stampBuffer.IndexBuffer,
            stampBuffer.TriangleCount,
            stampBuffer.Texture,
            stampBuffer.RasterizerState
        );

        StampedShapeBuffers.Add(stampedBuffer);
    }
}
