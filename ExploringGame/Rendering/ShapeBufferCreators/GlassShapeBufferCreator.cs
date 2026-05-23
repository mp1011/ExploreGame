using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.SimpleShapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Logics.Pathfinding;
using ExploringGame.Texture;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Rendering.ShapeBufferCreators;

class GlassShapeBufferCreator : ShapeBufferCreator
{      
    public GlassShapeBufferCreator(Dictionary<Shape, Triangle[]> shapeTriangles, LoadedTextureSheets loadedTextureSheets, GraphicsDevice graphicsDevice, AnnotatedGraph<RoomLightData> roomLightGraph = null) 
        : base(shapeTriangles, loadedTextureSheets, graphicsDevice, roomLightGraph)
    {
    }

    protected override IEnumerable<ShapeBuffer> CreateShapeBuffers(WorldSegment worldSegment)
    {
        var glassPanes = worldSegment.TraverseAllChildren().OfType<GlassPane>().ToArray();

        foreach(var group in glassPanes.GroupBy(p=>p.LightingGroup))
        {
            yield return CreateGlassPaneBuffer(worldSegment, group.Key, group.ToArray());
        }
       
    }

    private ShapeBuffer CreateGlassPaneBuffer(WorldSegment worldSegment, ILightingGroup lightingGroup, GlassPane[] glassPanes)
    {
        var triangles = new Dictionary<Shape, Triangle[]>();
        foreach (var glassPane in glassPanes)
            triangles[glassPane] = _shapeTriangles[glassPane];

        var texture = _textureSheets.Get(TextureSheetKey.Upstairs);
        var buffers = _vertexBufferBuilder.Build(triangles, texture, _graphicsDevice);

        var lights = GetLights(lightingGroup);

        // Use worldSegment as the shape, Outdoors texture, and CullNone rasterizer state
        return new ShapeBuffer(worldSegment, buffers.Item1, buffers.Item2, buffers.Item3, TextureSheetKey.Basement, RasterizerState.CullNone, null,
            LightData: lights,
            Type: ShapeBufferType.Glass);
    }

}
