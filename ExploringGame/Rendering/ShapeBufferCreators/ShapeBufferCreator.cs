using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Logics.Pathfinding;
using ExploringGame.Texture;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Rendering.ShapeBufferCreators;

abstract class ShapeBufferCreator
{
    protected readonly LoadedTextureSheets _textureSheets;
    protected GraphicsDevice _graphicsDevice;
    protected Dictionary<Shape, Triangle[]> _shapeTriangles;
    protected AnnotatedGraph<RoomLightData> _roomLightGraph;

    protected readonly VertexBufferBuilder _vertexBufferBuilder = new VertexBufferBuilder();
    // private readonly GrassVertexBufferBuilder _grassVertexBufferBuilder = new GrassVertexBufferBuilder();

    public ShapeBufferCreator(Dictionary<Shape, Triangle[]> shapeTriangles,
        LoadedTextureSheets loadedTextureSheets, GraphicsDevice graphicsDevice,
        AnnotatedGraph<RoomLightData> roomLightGraph = null)
    {
        _textureSheets = loadedTextureSheets;
        _graphicsDevice = graphicsDevice;
        _shapeTriangles = shapeTriangles;
        _roomLightGraph = roomLightGraph;
    }


    public ShapeBuffer[] Execute(WorldSegment worldSegment)
    {
        return CreateShapeBuffers(worldSegment)
            .Where(p=>p.TriangleCount > 0)
            .ToArray();
    }

    protected RoomLightData GetLights(ILightingGroup group)
    {
        return _roomLightGraph.Get(group);
    }

    protected abstract IEnumerable<ShapeBuffer> CreateShapeBuffers(WorldSegment worldSegment);

 
}
