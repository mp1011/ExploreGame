using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System;

namespace ExploringGame.GeometryBuilder.Shapes.TestShapes;

class FaceCutoutTest : Shape
{
    private SurfaceIndent[] _interiors;
    private Placement2D _cutoutPlacement;

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public FaceCutoutTest()
    {
        Width = 1.0f;
        Height = 1.0f;
        Depth = 1.0f;

        SideTextures[Side.North] = new TextureInfo(Color.Gray);
        SideTextures[Side.South] = new TextureInfo(Color.Pink);
        SideTextures[Side.West] = new TextureInfo(Color.Red);
        SideTextures[Side.East] = new TextureInfo(Color.Green);

        _cutoutPlacement = new Placement2D(Left: 0.3f, Right: 0.1f, Bottom: 0.1f, Top: 0.1f);

        _interiors = new SurfaceIndent[] {
            new SurfaceIndent(this, Side.South, _cutoutPlacement, 0.2f),
            new SurfaceIndent(this, Side.North, _cutoutPlacement, 0.2f),
            new SurfaceIndent(this, Side.West, _cutoutPlacement, 0.2f),
            new SurfaceIndent(this, Side.East, _cutoutPlacement, 0.2f),
        };
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        var shape = BuildCuboid();
        shape = new RemoveSurfaceRegion().Execute(shape, Side.South, _cutoutPlacement, ViewFrom);
        shape = new RemoveSurfaceRegion().Execute(shape, Side.North, _cutoutPlacement, ViewFrom);
        shape = new RemoveSurfaceRegion().Execute(shape, Side.West, _cutoutPlacement, ViewFrom);
        shape = new RemoveSurfaceRegion().Execute(shape, Side.East, _cutoutPlacement, ViewFrom);

        return shape;
    }
}
