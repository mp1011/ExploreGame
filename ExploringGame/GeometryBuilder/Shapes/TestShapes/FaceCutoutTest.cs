using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.Services;
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

        SideColors[Side.North] = Color.Gray;
        SideColors[Side.South] = Color.Pink;
        SideColors[Side.West] = Color.Red;
        SideColors[Side.East] = Color.Green;

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
        shape = new RemoveSurfaceRegion().Execute(shape, Side.South, _cutoutPlacement);
        shape = new RemoveSurfaceRegion().Execute(shape, Side.North, _cutoutPlacement);
        shape = new RemoveSurfaceRegion().Execute(shape, Side.West, _cutoutPlacement);
        shape = new RemoveSurfaceRegion().Execute(shape, Side.East, _cutoutPlacement);

        return shape;
    }
}
