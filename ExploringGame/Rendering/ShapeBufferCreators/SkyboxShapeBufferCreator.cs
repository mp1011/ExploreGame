using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics.Pathfinding;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Rendering.ShapeBufferCreators;

internal class SkyboxShapeBufferCreator : ShapeBufferCreator
{
    public SkyboxShapeBufferCreator(LoadedTextureSheets loadedTextureSheets, GraphicsDevice graphicsDevice) 
        : base(new(), loadedTextureSheets, graphicsDevice, null)
    {
    }

    protected override IEnumerable<ShapeBuffer> CreateShapeBuffers(WorldSegment worldSegment)
    {
        // todo - redundancy, we only need to make a skybox once
        if(worldSegment.Skybox != null)
            yield return CreateSkyboxBuffer(worldSegment.Skybox);
    }

    private ShapeBuffer CreateSkyboxBuffer(SkyboxShape skybox)
    {
        var skyboxTriangles = skybox.Build((QualityLevel)8);

        var skyboxDepthStencilState = new DepthStencilState
        {
            DepthBufferEnable = true,
            DepthBufferWriteEnable = false,
            DepthBufferFunction = CompareFunction.LessEqual
        };

        var buffers = _vertexBufferBuilder.Build(skyboxTriangles, _textureSheets.Get(skybox.Theme.TextureSheetKey), _graphicsDevice);
        return new ShapeBuffer(skybox, buffers.Item1, buffers.Item2, buffers.Item3, skybox.Theme.TextureSheetKey, skybox.RasterizerState,
            LightingGroup: skybox,
            DepthStencilState: skyboxDepthStencilState,
            Type: ShapeBufferType.Skybox);                      
    }
}
