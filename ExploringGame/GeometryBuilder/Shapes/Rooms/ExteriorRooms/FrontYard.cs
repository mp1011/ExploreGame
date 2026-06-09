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

    public FrontWalkway FrontWalkway { get; private set;  }
    public override Side OmitSides => Side.North | Side.South | Side.East | Side.West | Side.Top;


    public Room SouthSection { get; private set; }

    public override Theme Theme => new YardTheme();
    
    public FrontYard(WorldSegment worldSegment, FrontDeck deck) : base(worldSegment)
    {
        Size = Vector3.One;
        Deck = deck;
    }

    public void LoadChildren(Garage garage, Bedroom bedRoom, SpareRoom spareRoom)
    {
        Depth = Deck.Depth + Measure.Feet(0);
        Width = Measure.Feet(40);
        Height = Deck.Height + Measure.Feet(4);

        SetLocalSide(Side.Bottom, Deck.GetLocalSide(Side.Bottom) - Measure.Feet(4));
        SetLocalSide(Side.South, Deck.WestPart.GetLocalSide(Side.South));
        SetLocalSide(Side.East, Deck.GetLocalSide(Side.West));

        AddConnectingRoom(Deck, Side.East);

        var deckStairs = Deck.AddChild(new FrontDeckStairs(this, Deck));
        deckStairs.SetLocalSide(Side.Bottom, GetLocalSide(Side.Bottom));
        deckStairs.SetLocalSide(Side.North, Deck.WestPart.GetLocalSide(Side.South));
        deckStairs.SetLocalSide(Side.East, Deck.WestPart.GetLocalSide(Side.East));

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
        sidewalk.SetLocalSideUnanchored(Side.East, northPart.GetLocalSide(Side.East));


        var northFence = new Fence(northPart, Side.North);

       

        var driveway = AddChild(new Driveway(WorldSegment, garage, this));
        AddChild(new GarageDoor(WorldSegment, garage, driveway, HAlign.Left, 1.0f));
        AddChild(new GarageDoor(WorldSegment, garage, driveway, HAlign.Right, -1.0f));

        var westWall = new OuterWall(driveway, Side.East);
        westWall.SetLocalSideUnanchored(Side.Top, GetLocalSide(Side.Top));
        westWall.SetLocalSideUnanchored(Side.North, Deck.GetLocalSide(Side.South));

        FrontWalkway = new FrontWalkway(this);

        FrontWalkway.LoadChildren(this, driveway);
        driveway.LoadChildren(this, garage);


        new Window(bedRoom, Side.West, Measure.Feet(4), Measure.Feet(4), otherRoom: driveway);
        new Window(spareRoom, Side.West, Measure.Feet(4), Measure.Feet(4), otherRoom: driveway);

        var westOfWalkway = Copy();
        westOfWalkway.Place().OnSideOuter(Side.West, FrontWalkway)
            .OnSideOuter(Side.South, this);
        westOfWalkway.SetLocalSideUnanchored(Side.South, driveway.GetLocalSide(Side.North));
        westOfWalkway.SetLocalSideUnanchored(Side.West, GetLocalSide(Side.West));


        var southSection = Copy(inheritLightingGroup: false);
        southSection.Tag = "SouthFrontYard";
        southSection.Depth = driveway.Depth * 1.6f;
        southSection.Width = driveway.Width;
        southSection.Place().OnSideOuter(Side.South, driveway)
            .OnSideInner(Side.East, driveway);

        AddConnectingRoom(driveway, Side.None);
        driveway.AddConnectingRoom(southSection, Side.None);

        new GrassSurface(southSection, TerrainSurface.DefaultLawn);

        var flowerBed = new FlowerBed(WorldSegment, this, driveway);

        // Add grass surfaces following terrain variation
        var terrainMain = new TerrainSurface(this, TerrainSurface.DefaultLawn);
        var terrainNorth = new TerrainSurface(northPart, TerrainSurface.DefaultLawn);
        var terrainWestOfWalkway = new TerrainSurface(westOfWalkway, TerrainSurface.DefaultLawn);

        new GrassSurface(this, terrainMain);
        new GrassSurface(northPart, terrainNorth);
        new GrassSurface(westOfWalkway, terrainWestOfWalkway);


        SouthSection = southSection;
    }
}
