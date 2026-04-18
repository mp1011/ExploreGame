using ExploringGame.Entities;
using ExploringGame.GameDebug;
using ExploringGame.GeometryBuilder.Shapes.Rooms.BasementRooms;
using ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;
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
        Size = Vector3.One;
        FixedAmbientLight = LightIntensity.Bright;
        Deck = deck;
    }

    public void LoadChildren(Garage garage, Bedroom bedRoom, SpareRoom spareRoom)
    {
        Depth = Deck.Depth + Measure.Feet(10);
        Width = Measure.Feet(40);
        Height = Deck.Height + Measure.Feet(4);

        SetSide(Side.Bottom, Deck.GetSide(Side.Bottom) - Measure.Feet(4));
        SetSide(Side.South, Deck.WestPart.GetSide(Side.South));
        SetSide(Side.East, Deck.GetSide(Side.West));

        AddConnectingRoom(Deck, Side.East);

        var deckStairs = Deck.AddChild(new FrontDeckStairs(this, Deck));
        deckStairs.SetSide(Side.Bottom, GetSide(Side.Bottom));
        deckStairs.SetSide(Side.North, Deck.WestPart.GetSide(Side.South));
        deckStairs.SetSide(Side.East, Deck.WestPart.GetSide(Side.East));

        var northPart = Copy(width: Width + Deck.Width + Measure.Feet(10), depth: Measure.Feet(6));
        northPart.Tag = "FrontYardNorth";
        AddConnectingRoom(northPart, Side.North, HAlign.Left, 0f);

        var sidewalk = AddChild(new Box(Theme, TextureKey.Concrete));
        sidewalk.Tag = "Sidewalk";
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

        var walkway = AddChild(new Box(Theme, TextureKey.Concrete));
        walkway.Height = Measure.Inches(2);
        walkway.Depth = Measure.Feet(10);       
        walkway.SetSide(Side.Top, GetSide(Side.Bottom));

        walkway.Place().OnSideOuter(Side.South, this)
                       .OnSideOuter(Side.West, Deck);            
        walkway.SetSideUnanchored(Side.West, GetSide(Side.West));
        walkway.SetSideUnanchored(Side.South, Deck.GetSide(Side.South));

        var driveway = AddChild(new Driveway(this.WorldSegment, this, garage));
        AddChild(new GarageDoor(WorldSegment, garage, driveway, -4.0f));
        AddChild(new GarageDoor(WorldSegment, garage, driveway, 2.0f));


        var westWall = new OuterWall(driveway, Side.East);
        westWall.SetSideUnanchored(Side.Top, GetSide(Side.Top));
        westWall.SetSideUnanchored(Side.North, Deck.GetSide(Side.South));

        new Window(bedRoom, Side.West, Measure.Feet(4), Measure.Feet(4), otherRoom: driveway);
        new Window(spareRoom, Side.West, Measure.Feet(4), Measure.Feet(4), otherRoom: driveway);


        // Add grass surfaces following terrain variation
        var terrainMain  = new TerrainSurface(this, TerrainSurface.DefaultLawn);
        var terrainNorth = new TerrainSurface(northPart, TerrainSurface.DefaultLawn);
        new GrassSurface(this, terrainMain);
        new GrassSurface(northPart, terrainNorth);


        DebugShapeLogger.LogShape("Yard north", northPart);
    }
}
