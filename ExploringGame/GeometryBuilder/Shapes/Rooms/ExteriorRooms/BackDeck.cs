using ExploringGame.Entities;
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

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;

public class BackDeck : Deck
{
    private readonly Color _deckColor = Color.DarkRed;
    public override ViewFrom ViewFrom => ViewFrom.None;

    public override Theme Theme { get; } = new BackDeckTheme();

    public float StepWidth { get; }
    public float SideStepWidth => Measure.Feet(4);

    public BackDeckSideStairs SideStairs { get; private set; }

    public override ILightingGroup LightingGroup { get; }

    public BackDeck(WorldSegment worldSegment, Room backDeckArea, Den den) : base(worldSegment)
    {
        LightingGroup = backDeckArea.LightingGroup;

        var floor = AddChild(new Box(Theme));
        floor.LocalPosition = backDeckArea.LocalPosition;
        floor.Size = backDeckArea.Size;

        floor.SetWorldSide(Side.Top, den.GetWorldSide(Side.Bottom));
        floor.SetWorldSideUnanchored(Side.Bottom, den.GetWorldSide(Side.Bottom) - Measure.Feet(1));
        floor.AdjustShape().AddToSide(Side.East, SideStepWidth);

        LocalPosition = floor.LocalPosition;
        Size = floor.Size;
        this.SetWorldSide(Side.Bottom, floor.GetWorldSide(Side.Top));
        this.SetWorldSideUnanchored(Side.Top, den.GetWorldSide(Side.Top));

        var southWestPost = CreatePost(_deckColor).Place()
           .OnSideInner(Side.South, this, -PostInset)
           .OnSideInner(Side.West, this, PostInset)
           .Shape();

        var southPost1 = CreatePost(_deckColor).Place()
           .OnSideInner(Side.South, this, -PostInset)
           .OnSideInner(Side.West, this, Measure.Feet(6))
           .Shape();

        var southPost2 = CreatePost(_deckColor).Place()
           .OnSideInner(Side.South, this, -PostInset)
           .OnSideInner(Side.West, this, Measure.Feet(12))
           .Shape();

        var southEastPost = CreatePost(_deckColor).Place()
          .OnSideInner(Side.South, this, -PostInset)
          .OnSideInner(Side.East, this, -PostInset)
          .Shape();

        var northEastPost = CreatePost(_deckColor).Place()
         .OnSideInner(Side.North, this, PostInset)
         .OnSideInner(Side.East, this, -PostInset)
         .Shape();

        CreateRailing(southWestPost, southPost1, Color.Brown);
        CreateRailing(southPost2, southEastPost, Color.Brown);
        CreateRailing(southEastPost, northEastPost, Color.Brown);

        StepWidth = southPost2.GetWorldSide(Side.West) - southPost1.GetWorldSide(Side.East);

        var stairs = AddChild(new BackDeckStairs(backDeckArea, this));
        stairs.Place().OnFloor(backDeckArea)
            .OnSideOuter(Side.South, this);
        stairs.SetWorldSide(Side.West, southPost1.GetWorldSide(Side.East));

        var sideStairs = AddChild(new BackDeckSideStairs(backDeckArea, this));
        sideStairs.Place().OnFloor(backDeckArea)
            .OnSideOuter(Side.North, this)
            .OnSideInner(Side.East, this);

        SideStairs = sideStairs;

        var light = new OutdoorLight(this, StateKey.BackDeckLightOn);
        light.Place().AtParent().OnSideInner(Side.West).OnSideInner(Side.Top);
        light.LocalZ -= Measure.Feet(2);

        var lightSwitch = new LightSwitch(den, Side.South, StateKey.BackDeckLightOn);
        lightSwitch.ControlledObjects.Add(light.Bulb);

        lightSwitch.Place().AtParent()
            .AtStandardSwitchHeight()
            .OnSideInner(Side.South);
        lightSwitch.SetWorldSide(Side.West, den.GetWorldSide(Side.West) + Measure.Feet(1));


        var underDeckShadow = AddChild(new ShadowVolume());
        underDeckShadow.AdjustShape().From(this);

        underDeckShadow.SetWorldSide(Side.Bottom, backDeckArea.GetWorldSide(Side.Bottom));
        underDeckShadow.SetWorldSideUnanchored(Side.Top, floor.GetWorldSide(Side.Bottom));
    }
}

public class BackDeckStairs : Stairs
{
    public static readonly float StepDepth = Measure.Feet(1);
    public override ViewFrom ViewFrom => ViewFrom.None;
    public override Theme Theme => TopFloor.Theme;

    public override ILightingGroup LightingGroup => TopFloor.LightingGroup;

    public BackDeckStairs(Room bottomFloor, BackDeck topFloor)
        : base(topFloor.WorldSegment, new Vector2(topFloor.StepWidth, StepDepth), bottomFloor, topFloor,
            topFloor.StepWidth,
            StepDepth * 4)
    {
    }   
    protected override Side StartSide => Side.South;

    protected override StairStep CreateStep()
    {
        return new StairStep(Theme.TextureSheetKey, Theme.GetTexture(TextureKey.Wood));
    }
}

public class BackDeckSideStairs : Stairs
{
    public static readonly float StepDepth = Measure.Feet(1);
    public override ViewFrom ViewFrom => ViewFrom.None;
    public override Theme Theme => TopFloor.Theme;
    public override ILightingGroup LightingGroup => TopFloor.LightingGroup;


    public BackDeckSideStairs(Room bottomFloor, BackDeck topFloor)
        : base(topFloor.WorldSegment, new Vector2(topFloor.SideStepWidth, StepDepth), bottomFloor, topFloor,
            topFloor.SideStepWidth,
            StepDepth * 4)
    {
    }

    protected override Side StartSide => Side.North;

    protected override StairStep CreateStep()
    {
        return new StairStep(Theme.TextureSheetKey, Theme.GetTexture(TextureKey.Wood));
    }
}

