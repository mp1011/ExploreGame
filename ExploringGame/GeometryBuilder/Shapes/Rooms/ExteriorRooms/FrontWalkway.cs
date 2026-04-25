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
    public Shape WestPart { get; private set; }
    public override ViewFrom ViewFrom => ViewFrom.None;

    public override Theme Theme { get; }


    public FrontWalkway(FrontYard yard) : base(yard.WorldSegment)
    {
        Size = Vector3.One;
        FixedAmbientLight = LightIntensity.Bright;
        Theme = yard.Theme;
        yard.AddChild(this);
    }

    public void LoadChildren(FrontYard yard, Driveway driveway)
    {
        var walkway = AddChild(new Box(Theme, TextureKey.Concrete));
        walkway.Height = Measure.Feet(1);
        walkway.Depth = Measure.Feet(10);
        walkway.Width = Measure.Feet(25);
        walkway.SetSide(Side.Top, yard.GetSide(Side.Bottom));
        walkway.Place().OnSideOuter(Side.South, yard)
                       .OnSideOuter(Side.West, yard.Deck);
        walkway.SetSideUnanchored(Side.South, yard.Deck.GetSide(Side.South));
        walkway.SetSideUnanchored(Side.East, yard.Deck.WestPart.GetSide(Side.West));

        var walkway2 = AddChild(new Box(Theme, TextureKey.Concrete));
        walkway2.Height = Measure.Feet(2);
        walkway2.Depth = Measure.Feet(10);
        walkway2.Width = Measure.Feet(5);
        walkway2.Place().OnSideOuter(Side.West, walkway)
            .OnSideInner(Side.North, walkway);
        walkway2.SetSide(Side.Top, walkway.GetSide(Side.Top) - Measure.Feet(1));
        walkway2.SetSideUnanchored(Side.South, driveway.GetSide(Side.North));

        var walkwaySide = AddChild(new Box(Theme, TextureKey.Brick));
        walkwaySide.Width = walkway.Width;
        walkwaySide.Depth = Measure.Feet(1);
        walkwaySide.Height = Measure.Feet(6);
        walkwaySide.Place().At(walkway)
            .OnSideInner(Side.North, walkway);
        walkwaySide.SetSide(Side.Top, walkway.GetSide(Side.Top) + Measure.Inches(4));
        walkwaySide.SetSideUnanchored(Side.West, walkway2.GetSide(Side.West) - Measure.Inches(6));

        var walkwaySide2 = AddChild(new Box(Theme, TextureKey.Brick));
        walkwaySide2.Width = Measure.Feet(1);
        walkwaySide2.Depth = Measure.Feet(6);
        walkwaySide2.Height = Measure.Feet(6);
        walkwaySide2.Place().At(walkway2)
            .OnSideInner(Side.West, walkwaySide)
            .OnSideInner(Side.North, walkwaySide)
            .AlignSideWith(Side.Top, walkwaySide);
        walkwaySide2.SetSideUnanchored(Side.South, walkway2.GetSide(Side.South));

        var walkwaySideSouth = AddChild(new Box(Theme, TextureKey.Brick));
        walkwaySideSouth.Width = walkway.Width;
        walkwaySideSouth.Depth = Measure.Feet(1);
        walkwaySideSouth.Height = Measure.Feet(6);
        walkwaySideSouth.Place().At(walkway)
            .OnSideInner(Side.South, walkway)
            .AlignSideWith(Side.Top, walkwaySide);
        walkwaySideSouth.SetSideUnanchored(Side.West, walkway.GetSide(Side.West) - Measure.Inches(6));

        var walkwaySide3 = AddChild(new Box(Theme, TextureKey.Brick));
        walkwaySide3.Width = Measure.Feet(1);
        walkwaySide3.Depth = Measure.Feet(6);
        walkwaySide3.Height = Measure.Feet(6);
        walkwaySide3.Place().At(walkway2)
            .OnSideInner(Side.East, walkway2)
            .OnSideInner(Side.North, walkway2)
            .AlignSideWith(Side.Top, walkwaySide);
        walkwaySide3.SetSideUnanchored(Side.South, walkway2.GetSide(Side.South));
        walkwaySide3.SetSideUnanchored(Side.North, walkwaySideSouth.GetSide(Side.South));

        SetSide(Side.West, walkway2.GetSide(Side.West));
        SetSide(Side.North, walkway.GetSide(Side.North));

        SetSideUnanchored(Side.East, walkway.GetSide(Side.East));
        SetSideUnanchored(Side.South, walkway2.GetSide(Side.South));

        WestPart = walkway2;
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return Array.Empty<Triangle>();
    }
}
