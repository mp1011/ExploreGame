using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;
using ExploringGame.GeometryBuilder.Shapes.SimpleShapes;
using ExploringGame.GeometryBuilder.Shapes.Structures;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Services;
using ExploringGame.Texture;
using ExploringGame.Texture.Themes;
using Microsoft.Xna.Framework;
using System.Linq;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;

public class FrontDeck : Deck
{ 
    public Shape WestPart { get; private set; }
    public override Side OmitSides => Side.North | Side.South | Side.West | Side.Top;
    public override Theme Theme => new FrontPorchTheme();
    public FrontDeck(WorldSegment worldSegment) : base(worldSegment)
    {
    }

    public void LoadChildren(LivingRoom livingRoom)
    {
        WestPart = AddChild(new Box(Theme));
        WestPart.SideTextures[Side.Top] = Theme.GetTexture(TextureKey.Wood);

        WestPart.Width = Width;
        WestPart.Height = Measure.Feet(4);
        WestPart.Depth = Depth - Measure.Feet(6);

        WestPart.Place()
            .OnSideOuter(Side.West, this)
            .OnSideInner(Side.North, this);

        WestPart.SetLocalSide(Side.Top, GetLocalSide(Side.Bottom));

        var northPart = AddChild(new Box(Theme));
        northPart.OmitSides = Side.Top;
        northPart.Height = Height;
        northPart.Width = Width;
        northPart.Depth = Measure.Inches(5);
        northPart.Place().OnSideInner(Side.North)
            .OnSideInner(Side.West);
        northPart.SetLocalSide(Side.Top, WestPart.GetLocalSide(Side.Top));


        var southPart = AddChild(new Box(Theme));
        southPart.SideTextures[Side.Top] = Theme.GetTexture(TextureKey.Wood);
        southPart.Height = Height;
        southPart.Depth = Measure.Feet(2);
        southPart.Width = Width + WestPart.Width;
        southPart.Place().OnSideInner(Side.South)
           .OnSideInner(Side.East);
        southPart.SetLocalSide(Side.Top, WestPart.GetLocalSide(Side.Top));

        // posts
        var northWestPost = CreatePost().Place()
            .OnSideInner(Side.North, WestPart, PostInset)
            .OnSideInner(Side.West, WestPart, PostInset)
            .Shape();

        var westMiddlePost = CreatePost().Place()
            .OnSideInner(Side.West, WestPart, PostInset)
            .Shape();
        westMiddlePost.Z = WestPart.Z;

        var northEastPost = CreatePost().Place()
         .OnSideInner(Side.North, WestPart, PostInset)
         .OnSideInner(Side.East, this, -PostInset)
         .Shape();

        var southWestPost = CreatePost().Place()
          .OnSideInner(Side.South, WestPart, -PostInset)
          .OnSideInner(Side.West, WestPart, PostInset)
          .Shape();

        var southEastPost = CreatePost().Place()
          .OnSideInner(Side.South, WestPart, -PostInset)
          .OnSideInner(Side.East, WestPart, -PostInset)
          .Shape();

        var southPost1 = CreatePost().Place()
            .OnSideInner(Side.South, this, -PostInset)
            .OnSideInner(Side.East, this, -PostInset)
            .Shape();

        var southPost2 = CreatePost().Place()
           .OnSideInner(Side.South, southPart, -PostInset)
           .OnSideInner(Side.West, southPart, +PostInset)
           .Shape();

        // railing
        CreateRailing(northWestPost, westMiddlePost);
        CreateRailing(westMiddlePost, southWestPost);
        CreateRailing(southWestPost, southEastPost);
        CreateRailing(northWestPost, northEastPost);
        CreateRailing(southPost1, southPost2);

        new Window(livingRoom, Side.West, Measure.Feet(6), Measure.Feet(4), HAlign.Right, -Measure.Feet(4), otherRoom: this);

        var frontDoor = livingRoom.AddConnectingRoomWithJunction(
            new DoorJunction(this, Side.West, HAlign.Left, DoorDirection.Pull, StateKey.FrontDoorOpen),
            other: this,
            side: Side.West,
            align: HAlign.Left,
            offset: 1.0f,
            adjustPlacement: false);

        frontDoor.TraverseAllChildren().OfType<Door>().First().Tag = "FrontDoor";
        frontDoor.SetLocalSideUnanchored(Side.Top, livingRoom.GetLocalSide(Side.Top));

        var light = new OutdoorLight(this, StateKey.FrontPorchLightOn);
        light.Place().AtParent().OnSideInner(Side.East).OnSideInner(Side.Top);
        light.Z += Measure.Feet(2);

        var lightSwitch = new LightSwitch(livingRoom, Side.West, StateKey.FrontPorchLightOn);
        lightSwitch.Place()
            .AtParent()
            .AtStandardSwitchHeight()
            .OnSideInner(Side.West);

        lightSwitch.ControlledObjects.Add(light.Bulb);

        lightSwitch.Z += Measure.Feet(4);

    }
}

public class FrontDeckStairs : Stairs
{
    public override ViewFrom ViewFrom => ViewFrom.None;
    public override Theme Theme => TopFloor.Theme;

    public override ILightingGroup LightingGroup => TopFloor.LightingGroup;

    public FrontDeckStairs(FrontYard bottomFloor, FrontDeck topFloor) 
        : base(topFloor.WorldSegment, CalcStepSize(bottomFloor, topFloor), bottomFloor, topFloor, 
            CalcStairsWidth(topFloor),
            CalcStairsDepth(bottomFloor, topFloor))
    {
    }

    private static Vector2 CalcStepSize(FrontYard yard, FrontDeck deck)
    {
        return new Vector2(CalcStairsWidth(deck) / 4.0f, CalcStairsDepth(yard, deck));
    }

    private static float CalcStairsWidth(FrontDeck deck) => deck.WestPart.Width;

    private static float CalcStairsDepth(FrontYard yard, FrontDeck deck) =>
        deck.Depth - deck.WestPart.Depth - Measure.Feet(2);

    protected override Side StartSide => Side.West;

    protected override StairStep CreateStep()
    {
        return new StairStep(Theme.TextureSheetKey, Theme.GetTexture(TextureKey.Wood));
    }
}
