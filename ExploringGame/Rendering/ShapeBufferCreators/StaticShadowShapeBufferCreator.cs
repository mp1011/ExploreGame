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

class StaticShadowShapeBufferCreator : ShapeBufferCreator
{      
    public StaticShadowShapeBufferCreator(Dictionary<Shape, Triangle[]> shapeTriangles, LoadedTextureSheets loadedTextureSheets, GraphicsDevice graphicsDevice) 
        : base(shapeTriangles, loadedTextureSheets, graphicsDevice, null)
    {
    }

    protected override IEnumerable<ShapeBuffer> CreateShapeBuffers(WorldSegment worldSegment)
    {
        var shadows = worldSegment.TraverseAllChildren().OfType<ShadowVolume>().ToArray();

        yield return CreateShadowBuffer(worldSegment, shadows);       
    }

    private ShapeBuffer CreateShadowBuffer(WorldSegment worldSegment, ShadowVolume[] shadows)
    {
        var triangles = new Dictionary<Shape, Triangle[]>();
        foreach (var shadow in shadows)
            triangles[shadow] = _shapeTriangles[shadow];

        var texture = _textureSheets.Get(TextureSheetKey.Upstairs);
        var buffers = _vertexBufferBuilder.Build(triangles, texture, _graphicsDevice);

        return new ShapeBuffer(worldSegment, buffers.Item1, buffers.Item2, buffers.Item3, TextureSheetKey.Basement, 
            RasterizerState.CullNone, null,
            Type: ShapeBufferType.StaticShadow);
    }

}
