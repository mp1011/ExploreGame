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

class PointLightShapeBufferCreator : ShapeBufferCreator
{
    private const int NeighborDepth = 3;

    public PointLightShapeBufferCreator(Dictionary<Shape, Triangle[]> shapeTriangles, LoadedTextureSheets loadedTextureSheets, GraphicsDevice graphicsDevice, AnnotatedGraph<RoomLightData> roomLightGraph = null) 
        : base(shapeTriangles, loadedTextureSheets, graphicsDevice, roomLightGraph)
    {
    }

    protected override IEnumerable<ShapeBuffer> CreateShapeBuffers(WorldSegment worldSegment)
    {
        foreach (var lightingGroup in worldSegment.TraverseAllChildren()
            .GroupBy(p => p.LightingGroup))
        {
            if (lightingGroup.Key is Room r) // todo...fix this
            {
                var neighbors = GetSelfAndNeighbors(r);
                var lights = neighbors.SelectMany(p => p.TraverseAllChildren()).OfType<ILightSource>().ToArray();

                var shapes = r.TraverseAllChildren().ToArray();
                yield return CreateShapeBuffer(worldSegment, shapes, lightingGroup.Key, lights);
            }
        }
    }

    private ShapeBuffer CreateShapeBuffer(Shape shape, Shape[] children, ILightingGroup lightingGroup, ILightSource[] lights)
    {
        var worldSegmentTriangles = new Dictionary<Shape, Triangle[]>();
        foreach (var child in children)
            worldSegmentTriangles[child] = _shapeTriangles[child];

        var buffers = _vertexBufferBuilder.Build(worldSegmentTriangles, textureSheet: null, graphicsDevice: _graphicsDevice);
        return new ShapeBuffer(shape, buffers.Item1, buffers.Item2, buffers.Item3, TextureSheetKey.Default, shape.RasterizerState, lightingGroup,
            PointLights: lights,
            Type: ShapeBufferType.PointLight);
    }


    private IEnumerable<Room> GetSelfAndNeighbors(Room room)
    {
        List<Room> result = new();
        GetSelfAndNeighbors(room, NeighborDepth, result);
        return result;
    }

    private void GetSelfAndNeighbors(Room room, int depth, List<Room> result)
    {
        if (result.Contains(room))
            return;

        result.Add(room);

        if (depth > 0)
        {
            var neighbors = room.RoomConnections.Select(p => p.GetOtherRoom(room)).ToArray();
            foreach (var neighbor in neighbors)
                GetSelfAndNeighbors(neighbor, depth - 1, result);
        }
    }
}
