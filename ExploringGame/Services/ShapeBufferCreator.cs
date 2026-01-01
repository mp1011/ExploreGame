using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.Rendering;
using ExploringGame.Texture;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Services;

internal class ShapeBufferCreator
{
    private readonly VertexBufferBuilder _vertexBufferBuilder = new VertexBufferBuilder();

    public ShapeBuffer[] Execute(Dictionary<Shape, Triangle[]> shapeTriangles,
        TextureSheet textureSheet,
        GraphicsDevice graphicsDevice)
    {
        List<ShapeBuffer> shapeBuffers = new();

        foreach(var key in shapeTriangles.Keys)
        {
            if(key is SimpleRoom || key is Box)
            {
                var d = new  Dictionary<Shape, Triangle[]>();
                d[key] = shapeTriangles[key];
                var buffers = _vertexBufferBuilder.Build(d, textureSheet, graphicsDevice);
                shapeBuffers.Add(new ShapeBuffer(key, buffers.Item1, buffers.Item2, buffers.Item3));
            }
        }
        return shapeBuffers.ToArray();

    }
}
