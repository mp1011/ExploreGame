using ExploringGame.GeometryBuilder.Shapes.Rooms.BasementRooms;
using ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;
using ExploringGame.GeometryBuilder.Shapes.SimpleShapes;
using ExploringGame.Logics;
using ExploringGame.Services;
using ExploringGame.Texture;
using System;
using System.Numerics;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;

public class FrontWalkway : Room
{
    private FrontYard _yard;

    public Shape WestPart { get; private set; }
    public override ViewFrom ViewFrom => ViewFrom.None;

    public override Theme Theme { get; }


    public override ILightingGroup LightingGroup => _yard;

    public FrontWalkway(FrontYard yard) : base(yard.WorldSegment)
    {
        _yard = yard;
        Size = Vector3.One;
        Theme = yard.Theme;
    }

    public void LoadChildren(FrontYard yard, Driveway driveway)
    {
        var walkway = AddChild(new Box(Theme, TextureKey.Concrete));
        walkway.Height = Measure.Feet(1);
        walkway.Depth = Measure.Feet(10);
        walkway.Width = Measure.Feet(25);
        walkway.SetLocalSide(Side.Top, yard.GetLocalSide(Side.Bottom));
        walkway.Place().OnSideOuter(Side.South, yard)
                       .OnSideOuter(Side.West, yard.Deck);
        walkway.SetLocalSideUnanchored(Side.South, yard.Deck.GetLocalSide(Side.South));
        walkway.SetLocalSideUnanchored(Side.East, yard.Deck.WestPart.GetLocalSide(Side.West));

        var walkway2 = AddChild(new Box(Theme, TextureKey.Concrete));
        walkway2.Height = Measure.Feet(2);
        walkway2.Depth = Measure.Feet(10);
        walkway2.Width = Measure.Feet(5);
        walkway2.Place().OnSideOuter(Side.West, walkway)
            .OnSideInner(Side.North, walkway);
        walkway2.SetLocalSide(Side.Top, walkway.GetLocalSide(Side.Top) - Measure.Feet(1));
        walkway2.SetLocalSideUnanchored(Side.South, driveway.GetLocalSide(Side.North));

        var walkwaySide = AddChild(new Box(Theme, TextureKey.Brick));
        walkwaySide.Width = walkway.Width;
        walkwaySide.Depth = Measure.Feet(1);
        walkwaySide.Height = Measure.Feet(6);
        walkwaySide.Place().At(walkway)
            .OnSideInner(Side.North, walkway);
        walkwaySide.SetLocalSide(Side.Top, walkway.GetLocalSide(Side.Top) + Measure.Inches(4));
        walkwaySide.SetLocalSideUnanchored(Side.West, walkway2.GetLocalSide(Side.West) - Measure.Inches(6));

        var walkwaySide2 = AddChild(new Box(Theme, TextureKey.Brick));
        walkwaySide2.Width = Measure.Feet(1);
        walkwaySide2.Depth = Measure.Feet(6);
        walkwaySide2.Height = Measure.Feet(6);
        walkwaySide2.Place().At(walkway2)
            .OnSideInner(Side.West, walkwaySide)
            .OnSideInner(Side.North, walkwaySide)
            .AlignSideWith(Side.Top, walkwaySide);
        walkwaySide2.SetLocalSideUnanchored(Side.South, walkway2.GetLocalSide(Side.South));

        var walkwaySideSouth = AddChild(new Box(Theme, TextureKey.Brick));
        walkwaySideSouth.Width = walkway.Width;
        walkwaySideSouth.Depth = Measure.Feet(1);
        walkwaySideSouth.Height = Measure.Feet(6);
        walkwaySideSouth.Place().At(walkway)
            .OnSideInner(Side.South, walkway)
            .AlignSideWith(Side.Top, walkwaySide);
        walkwaySideSouth.SetLocalSideUnanchored(Side.West, walkway.GetLocalSide(Side.West) - Measure.Inches(6));

        var walkwaySide3 = AddChild(new Box(Theme, TextureKey.Brick));
        walkwaySide3.Width = Measure.Feet(1);
        walkwaySide3.Depth = Measure.Feet(6);
        walkwaySide3.Height = Measure.Feet(6);
        walkwaySide3.Place().At(walkway2)
            .OnSideInner(Side.East, walkway2)
            .OnSideInner(Side.North, walkway2)
            .AlignSideWith(Side.Top, walkwaySide);
        walkwaySide3.SetLocalSideUnanchored(Side.South, walkway2.GetLocalSide(Side.South));
        walkwaySide3.SetLocalSideUnanchored(Side.North, walkwaySideSouth.GetLocalSide(Side.South));

        SetLocalSide(Side.West, walkway2.GetLocalSide(Side.West));
        SetLocalSide(Side.North, walkway.GetLocalSide(Side.North));

        SetLocalSideUnanchored(Side.East, walkway.GetLocalSide(Side.East));
        SetLocalSideUnanchored(Side.South, walkway2.GetLocalSide(Side.South));

        WestPart = walkway2;
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return Array.Empty<Triangle>();
    }
}
