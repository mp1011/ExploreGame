using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder;
using ExploringGame.Rendering;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ExploringGame.Tests.TextureCoordinateTests;

public class SlantedTiledSurfaceTests
{
    [Fact]
    public void SlantedSurface_WithTiledTexture_HasCorrectTextureCoordinates()
    {
        // Arrange - Create a slanted surface that should hold exactly 4 tiled textures (2x2 grid)
        var tileSize = 1.0f;
        var slantAmount = 1.0f;  // Amount to raise the north edge to create a slant

        // Calculate surface dimensions so that AFTER slanting, the surface area is 2x2
        var surfaceWidth = 2.0f * tileSize;  // 2 tiles wide (not affected by slanting)
        // For depth: we want sqrt(surfaceDepth^2 + slantAmount^2) = 2.0 * tileSize
        // So: surfaceDepth = sqrt((2.0 * tileSize)^2 - slantAmount^2)
        var targetSlantedDepth = 2.0f * tileSize;
        var surfaceDepth = (float)System.Math.Sqrt(targetSlantedDepth * targetSlantedDepth - slantAmount * slantAmount);

        var shape = new SlantedTestSurface
        {
            Position = new Vector3(0, 0, 0),
            Size = new Vector3(surfaceWidth, 0.1f, surfaceDepth),
            MainTexture = new TextureInfo(
                Key: TextureKey.Wood,
                Style: TextureStyle.Tile,
                TilingInfo: new TilingInfo(TileSize: tileSize)
            ),
            SlantAmount = slantAmount
        };

        // Act - Build the shape to get triangles
        var trianglesByShape = shape.Build(QualityLevel.Basic);
        var triangles = trianglesByShape[shape];

        // Filter to only top surface triangles (the slanted surface we're testing)
        var topTriangles = triangles.Where(t => t.Side == Side.Top).ToArray();

        // We should have exactly 8 triangles (2x2 tiles, 2 triangles per tile, 2 sides = 2*2*2 = 8)
        Assert.Equal(8, topTriangles.Length);

        // Create a mock TextureSheet to calculate texture coordinates
        var mockTextureSheet = new MockTextureSheet();
        var vertexBufferBuilder = new VertexBufferBuilder();

        // Get corner vertices for texture coordinate calculation
        var cornerVertices = topTriangles.GetCornerVertices(Side.Top);
        var planeInfo = TilingPlaneHelper.ComputePlaneInfo(topTriangles, cornerVertices);

        // Assert - Check texture coordinates for each triangle
        // All texture coordinates should be either 0.0 or 1.0
        foreach (var triangle in topTriangles)
        {
            foreach (var vertex in triangle.Vertices)
            {
                var textureCoords = vertexBufferBuilder.CalcTextureCoordinates(
                    shape,
                    Side.Top,
                    mockTextureSheet,
                    triangle,
                    vertex,
                    cornerVertices,
                    planeInfo
                );

                // For tiled textures, all coordinates should be either 0.0 or 1.0
                Assert.True(
                    IsAlmost(textureCoords.X, 0.0f) || IsAlmost(textureCoords.X, 1.0f),
                    $"Expected U coordinate to be 0.0 or 1.0, but got {textureCoords.X} at vertex {vertex}"
                );

                Assert.True(
                    IsAlmost(textureCoords.Y, 0.0f) || IsAlmost(textureCoords.Y, 1.0f),
                    $"Expected V coordinate to be 0.0 or 1.0, but got {textureCoords.Y} at vertex {vertex}"
                );
            }
        }
    }

    private bool IsAtTileBoundary(float position, float origin, float tileSize, float totalSize)
    {
        var relativePos = position - origin;
        var numTilesFromOrigin = relativePos / tileSize;

        // Check if we're at an integer multiple of tile size (within tolerance)
        var fracPart = System.Math.Abs(numTilesFromOrigin - System.Math.Round(numTilesFromOrigin));
        return fracPart < 0.01f;
    }

    private bool IsAlmost(float a, float b, float tolerance = 0.01f)
    {
        return System.Math.Abs(a - b) < tolerance;
    }

    /// <summary>
    /// A simple test shape that represents a single slanted surface (top face only)
    /// </summary>
    private class SlantedTestSurface : Shape
    {
        private Theme _theme = new Theme();
        public override Theme Theme => _theme;
        public override ViewFrom ViewFrom => ViewFrom.Outside;

        public float SlantAmount { get; set; }

        protected override Triangle[] BuildInternal(QualityLevel quality)
        {
            // Build only the top surface
            var triangles = TriangleMaker.BuildCuboid(this)
                .Where(t => t.Side == Side.Top)
                .ToArray();

            // Apply vertex offset to slant the surface
            if (SlantAmount != 0)
            {
                var vertexOffsetter = new VertexOffsetter();
                var offset = new VertexOffset(Side.North, new Vector3(0, SlantAmount, 0));
                triangles = vertexOffsetter.Execute(this, triangles, offset);
            }

            // Manually apply tiling split for test purposes
            // (normally this would happen in AdjustTrianglesForDisplay but only if quality > Basic)
            var splitter = new SplitTrianglesForTiling();
            triangles = splitter.Execute(this, triangles);

            return triangles;
        }
    }

    private class MockTextureSheet : TextureSheet
    {
        public override TextureSheetKey Key => TextureSheetKey.Basement;

        // Mock texture sheet size - texture covers the entire sheet for easier debugging
        private const int SheetSize = 1024;

        public MockTextureSheet() : base()
        {
            // Make the texture cover the entire sheet (0, 0) to (1024, 1024)
            // This makes 1.0 on the texture equal to 1.0 on the sheet
            TextureLocations[TextureKey.Wood] = new Rectangle(0, 0, SheetSize, SheetSize);
        }

        public override Vector2 TexturePosition(TextureKey key, Vector2 position)
        {
            // For testing purposes, just return the position as-is
            // Since the texture covers the whole sheet, this makes debugging easier:
            // position (1.0, 1.0) will map to sheet coordinate (1.0, 1.0)
            return position;
        }
    }
}
