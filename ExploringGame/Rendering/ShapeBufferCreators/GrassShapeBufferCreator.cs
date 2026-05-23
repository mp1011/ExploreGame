using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Logics.Pathfinding;
using ExploringGame.Texture;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Rendering.ShapeBufferCreators;

class GrassShapeBufferCreator : ShapeBufferCreator
{   
    
    private readonly GrassVertexBufferBuilder _grassVertexBufferBuilder = new GrassVertexBufferBuilder();

    public GrassShapeBufferCreator(Dictionary<Shape, Triangle[]> shapeTriangles, LoadedTextureSheets loadedTextureSheets, GraphicsDevice graphicsDevice, AnnotatedGraph<RoomLightData> roomLightGraph = null) 
        : base(shapeTriangles, loadedTextureSheets, graphicsDevice, roomLightGraph)
    {
    }

    protected override IEnumerable<ShapeBuffer> CreateShapeBuffers(WorldSegment worldSegment)
    {
        var grassSurfaces = worldSegment.TraverseAllChildren().OfType<GrassSurface>().ToArray();

        foreach(var group in grassSurfaces.GroupBy(p=>p.LightingGroup))
        {
            yield return CreateGrassShapeBuffer(worldSegment, group.Key, group.ToArray());
        }
       
    }

    private ShapeBuffer CreateGrassShapeBuffer(WorldSegment worldSegment, ILightingGroup lightingGroup, GrassSurface[] grassSurfaces)
    {
        var grassTriangles = new Dictionary<Shape, Triangle[]>();
        foreach (var grassSurface in grassSurfaces)
            grassTriangles[grassSurface] = _shapeTriangles[grassSurface];

        var grassTexture = _textureSheets.Get(TextureSheetKey.Outdoors);
        var buffers = _grassVertexBufferBuilder.Build(grassTriangles, grassTexture, _graphicsDevice);
        var lights = GetLights(lightingGroup);

        // Use worldSegment as the shape, Outdoors texture, and CullNone rasterizer state
        return new ShapeBuffer(worldSegment, buffers.Item1, buffers.Item2, buffers.Item3, TextureSheetKey.Outdoors, RasterizerState.CullNone, null,
            LightData: lights,
            Type: ShapeBufferType.Grass);
    }
}
