using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
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
    public LightingGroupShapeBufferCreator(Dictionary<Shape, Triangle[]> shapeTriangles, LoadedTextureSheets loadedTextureSheets, GraphicsDevice graphicsDevice, AnnotatedGraph<RoomLightData> roomLightGraph = null) 
        : base(shapeTriangles, loadedTextureSheets, graphicsDevice, roomLightGraph)
    {
    }

    protected override IEnumerable<ShapeBuffer> CreateShapeBuffers(WorldSegment worldSegment)
    {
        // group first by lighting group
        foreach(var lightingGroup in worldSegment.TraverseAllChildren()
            .GroupBy(p => p.LightingGroup))
        {
            // then by texture key
            foreach(var textureGroup in lightingGroup.GroupBy(p=>p.Theme.TextureSheetKey))
            {
                var textureKey = textureGroup.Key;
                var shapes = textureGroup.Where(p => IsStatic(p)).ToArray();

                if (shapes.Any())
                    yield return CreateShapeBuffer(worldSegment, shapes, textureKey, lightingGroup.Key);
            }
        }
    }

    private bool IsStatic(Shape s)
    {
        return s.ShapeBufferType == ShapeBufferType.Static
            && s is not ShapeStamp
            && s is not StampedShape;
    }

}
