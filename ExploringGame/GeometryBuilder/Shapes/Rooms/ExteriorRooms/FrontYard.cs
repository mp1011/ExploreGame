using ExploringGame.Entities;
using ExploringGame.GeometryBuilder.Shapes.SimpleShapes;
using ExploringGame.GeometryBuilder.Shapes.Structures;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System.Reflection.Metadata;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;

public class FrontYard : Room
{
    public FrontDeck Deck { get; }

    public override Side OmitSides => Side.North | Side.South | Side.East | Side.West | Side.Top;

    public override Theme Theme => new YardTheme();

    public FrontYard(WorldSegment worldSegment, FrontDeck deck) : base(worldSegment)
    {
        Depth = deck.Depth + Measure.Feet(10);
        Width = Measure.Feet(20);
        Height = deck.Height + Measure.Feet(4);

        SetSide(Side.Bottom, deck.GetSide(Side.Bottom) - Measure.Feet(4));
        SetSide(Side.South, deck.GetSide(Side.South) + Measure.Feet(5));
        SetSide(Side.East, deck.GetSide(Side.West));

        AddConnectingRoom(deck, Side.East);

        FixedAmbientLight = LightIntensity.Bright;
        Deck = deck;
    }

    public override void LoadChildren()
    {
        var deckStairs = Deck.AddChild(new FrontDeckStairs(this, Deck));
        deckStairs.SetSide(Side.Bottom, GetSide(Side.Bottom));
        deckStairs.SetSide(Side.North, Deck.WestPart.GetSide(Side.South));
        deckStairs.SetSide(Side.East, Deck.WestPart.GetSide(Side.East));

        var northPart = Copy(width: Width + Deck.Width + Measure.Feet(10), depth: Measure.Feet(6));   
        AddConnectingRoom(northPart, Side.North, HAlign.Left, 0f);

        var sidewalk = AddChild(new Box(Theme, TextureKey.Concrete));
        sidewalk.Height = Measure.Inches(2);
        sidewalk.Width = Deck.Width;
        sidewalk.Depth = Measure.Feet(6);
        sidewalk.Place().OnFloor()
            .OnSideOuter(Side.South, northPart)
            .OnSideOuter(Side.East, this);
        sidewalk.SetSideUnanchored(Side.East, northPart.GetSide(Side.East));

        var houseNorthSide = AddChild(new Box(Theme, TextureKey.Siding));
        houseNorthSide.OmitSides = Side.South;
        houseNorthSide.Width = Measure.Inches(6);
        houseNorthSide.Height = Height;
        houseNorthSide.Depth = Measure.Inches(4);
        houseNorthSide.Place().OnFloor();
        houseNorthSide.SetSide(Side.South, Deck.GetSide(Side.North));       
        houseNorthSide.AdjustShape().SetAxis(Axis.X, Deck.GetSide(Side.East), northPart.GetSide(Side.East));

        var northFence = new Fence(northPart, Side.North);
    }
}
