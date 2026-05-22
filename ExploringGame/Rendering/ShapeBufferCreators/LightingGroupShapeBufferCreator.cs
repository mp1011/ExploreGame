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

/// <summary>
/// Creates shape buffers for each lighting group
/// </summary>
class LightingGroupShapeBufferCreator : ShapeBufferCreator
{
    public LightingGroupShapeBufferCreator(Dictionary<Shape, Triangle[]> shapeTriangles, LoadedTextureSheets loadedTextureSheets, GraphicsDevice graphicsDevice, 
        AnnotatedGraph<RoomLightData> roomLightGraph) 
        : base(shapeTriangles, loadedTextureSheets, graphicsDevice, roomLightGraph)
    {
    }

    protected override IEnumerable<ShapeBuffer> CreateShapeBuffers(WorldSegment worldSegment)
    {
        // group first by lighting group
        foreach(var lightingGroup in worldSegment.TraverseAllChildren()
            .GroupBy(p => p.LightingGroup))
        {
            var lights = GetLights(lightingGroup.Key);

            // then by texture key
            foreach(var textureGroup in lightingGroup.GroupBy(p=>p.Theme.TextureSheetKey))
            {
                var textureKey = textureGroup.Key;
                var shapes = textureGroup.Where(p => IsStatic(p)).ToArray();

                if (shapes.Any())
                    yield return CreateShapeBuffer(worldSegment, shapes, textureKey, lightingGroup.Key, lights);
            }
        }
    }

    protected ShapeBuffer CreateShapeBuffer(
       Shape shape,
       Shape[] children,
       TextureSheetKey key,
       ILightingGroup lightingGroup,
       RoomLightData lightData)
    {
        var worldSegmentTriangles = new Dictionary<Shape, Triangle[]>();
        foreach (var child in children)
            worldSegmentTriangles[child] = _shapeTriangles[child];

        var buffers = _vertexBufferBuilder.Build(worldSegmentTriangles, _textureSheets.Get(key), _graphicsDevice);
        return new ShapeBuffer(shape, buffers.Item1, buffers.Item2, buffers.Item3, key, shape.RasterizerState, lightingGroup, 
            Type: ShapeBufferType.Normal, 
            LightData: lightData);
    }

    private bool IsStatic(Shape s)
    {
        return s.ShapeBufferType == ShapeBufferType.Static
            && s is not ShapeStamp
            && s is not StampedShape;
    }

}
