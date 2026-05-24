using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Logics.Pathfinding;
using ExploringGame.Texture;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Rendering.ShapeBufferCreators;

internal class MasterShapeBufferCreator 
{
    private readonly LoadedTextureSheets _textureSheets;
    private GraphicsDevice _graphicsDevice;
    private Dictionary<Shape, Triangle[]> _shapeTriangles;
    private AnnotatedGraph<RoomLightData> _roomLightGraph;

    public MasterShapeBufferCreator(Dictionary<Shape, Triangle[]> shapeTriangles,
        LoadedTextureSheets loadedTextureSheets, GraphicsDevice graphicsDevice,
        AnnotatedGraph<RoomLightData> roomLightGraph)
    {
        _textureSheets = loadedTextureSheets;
        _graphicsDevice = graphicsDevice;
        _shapeTriangles = shapeTriangles;
        _roomLightGraph = roomLightGraph;
    }

    private IEnumerable<ShapeBufferCreator> Creators
    {
        get
        {
            yield return new LightingGroupShapeBufferCreator(_shapeTriangles, _textureSheets, _graphicsDevice, _roomLightGraph);
            yield return new ActiveObjectShapeBufferCreator(_shapeTriangles, _textureSheets, _graphicsDevice, _roomLightGraph);
            yield return new GrassShapeBufferCreator(_shapeTriangles, _textureSheets, _graphicsDevice, _roomLightGraph);
            yield return new GlassShapeBufferCreator(_shapeTriangles, _textureSheets, _graphicsDevice, _roomLightGraph);
            yield return new SkyboxShapeBufferCreator(_textureSheets, _graphicsDevice);
            yield return new StaticShadowShapeBufferCreator(_shapeTriangles, _textureSheets, _graphicsDevice);
            // still todo, stamped shapes
        }
    }

    public ShapeBuffer[] Execute(WorldSegment worldSegment)
    {
        return Creators
            .SelectMany(c => c.Execute(worldSegment))
            .ToArray();
    }
}
