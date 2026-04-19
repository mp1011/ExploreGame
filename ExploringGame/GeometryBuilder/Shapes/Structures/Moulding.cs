using ExploringGame.GeometryBuilder.Shapes.SimpleShapes;
using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System;

namespace ExploringGame.GeometryBuilder.Shapes.Structures;

/// <summary>
/// Adds decorative moulding (trim) around the edges of a parent shape's face.
/// </summary>
public class Moulding : Shape
{
    public override ViewFrom ViewFrom => ViewFrom.None;
    public override IColliderMaker ColliderMaker => null;

    private readonly Side _parentSide;
    private readonly Side _mouldingSides;
    private readonly Color _color;
    private readonly float _size;
    private readonly float _thickness;
    private Theme _theme;

    public override Theme Theme => _theme;

    /// <summary>
    /// Creates moulding trim around specified edges of a parent shape's face.
    /// </summary>
    /// <param name="parent">The shape to apply moulding to</param>
    /// <param name="parentSide">Which side of the parent shape to apply moulding (e.g., Side.North)</param>
    /// <param name="mouldingSides">Which edges get moulding pieces (flags: Side.West | Side.East, or Side.North | Side.South | Side.Top | Side.Bottom)</param>
    /// <param name="color">Color/shade of the moulding</param>
    /// <param name="size">Width or height of each moulding piece (e.g., Measure.Inches(3))</param>
    /// <param name="thickness">How far the moulding extends from the parent surface (e.g., Measure.Inches(0.5))</param>
    public Moulding(Shape parent, Side parentSide, Side mouldingSides, Color color, float size, float thickness)
    {
        _parentSide = parentSide;
        _mouldingSides = mouldingSides;
        _color = color;
        _size = size;
        _thickness = thickness;

        Position = parent.Position;
        Size = parent.Size;

        parent.AddChild(this);

        // Create theme for moulding
        _theme = new Theme(TextureSheetKey.Upstairs, TextureKey.Plain, color);
      
        CreateMouldingPieces();
    }

    private void CreateMouldingPieces()
    {
        // Decompose the moulding sides flags into individual sides
        var sides = _mouldingSides.Decompose();

        foreach (var side in sides)
        {
            CreateMouldingPiece(side);
        }
    }

    private void CreateMouldingPiece(Side mouldingSide)
    {
        var piece = AddChild(new Box(Theme));

        // Determine the axis and dimensions based on parent side and moulding side
        var parentAxis = _parentSide.GetAxis();
        var mouldingAxis = mouldingSide.GetAxis();

        // Get the two axes that define the parent's face plane
        var (uAxis, vAxis) = _parentSide.GetAxisUV();

        // moulding is always along the Y Axis
        vAxis = Axis.Y;

        var thicknessAxis = Axis.All & ~uAxis & ~vAxis;

        float uSize, vSize;

        if(mouldingAxis == Axis.Y)
        {
            vSize = _size;
            uSize = Parent.GetAxisSize(uAxis) + (_size * 2);
        }
        else if(parentAxis != mouldingAxis)
        {
            vSize = Parent.GetAxisSize(vAxis);
            uSize = _size;
        }
        else
        {
            vSize = _size;
            uSize = Parent.GetAxisSize(uAxis);
        }

        piece.AdjustShape()
            .SetAxis(uAxis, uSize)
            .SetAxis(vAxis, vSize)
            .SetAxis(thicknessAxis, _thickness);

        piece.Place()
            .At(Parent)
            .OnSideOuter(mouldingSide)
            .OnSideOuter(_parentSide, offset: 0f);
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return Array.Empty<Triangle>();
    }
}
