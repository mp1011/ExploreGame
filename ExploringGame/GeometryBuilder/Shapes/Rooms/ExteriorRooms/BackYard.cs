using ExploringGame.GeometryBuilder.Shapes.Structures;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Services;
using ExploringGame.Texture;
using System.Linq;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;

public class BackYard : Room
{
    private static readonly float MainYardDepth = Measure.Feet(40);
    private Shape _denWindow;
    private Room _bedroomWindow;
    private Room _kitchenWindow;
    private Room _kidsBedroomSouthWindow;
    private Room _kidsBedroomEastWindow;

    private Shape _frontSidewalk;
    private Shape _northYard;

    public override Side OmitSides => Side.South | Side.North | Side.East | Side.West | Side.Top;

    public override Theme Theme { get; } = new YardTheme();

    public BackYard(WorldSegment worldSegment) : base(worldSegment)
    {
        Theme.SideTextures[Side.South] = Theme.GetTexture(TextureKey.Siding);

        Depth = Measure.Feet(20);
        Width = Measure.Feet(20);
        Height = Measure.Feet(20);

        FixedAmbientLight = LightIntensity.Bright;
    }

    public void SetDependencies(Shape frontSidewalk, Shape northYard, Shape denWindow, Room bedroomWindow, Room kitchenWindow, 
        Room kidsBedroomSouthWindow, Room kidsBedroomEastWindow)
    {
        _frontSidewalk = frontSidewalk;
        _northYard = northYard;
        _denWindow = denWindow;
        _bedroomWindow = bedroomWindow;
        _kitchenWindow = kitchenWindow;
        _kidsBedroomSouthWindow = kidsBedroomSouthWindow;
        _kidsBedroomEastWindow = kidsBedroomEastWindow;

        // Position this BackYard based on OutsideWorldSegment shapes
        this.Place().At(_frontSidewalk)
                    .OnSideOuter(Side.East, _frontSidewalk)
                    .OnSideInner(Side.North, _northYard);

        SetSide(Side.Bottom, _northYard.GetSide(Side.Bottom));
        SetSideUnanchored(Side.South, _frontSidewalk.GetSide(Side.South) - 0.6f);
        SetSideUnanchored(Side.North, _northYard.GetSide(Side.North));

        // Position based on UpstairsWorldSegment shapes
        SetSideUnanchored(Side.East, _denWindow.GetSide(Side.East) + 1.0f);

        // Position the child sections that were created in LoadChildren
        var eastSection = TraverseAllChildren().FirstOrDefault(c => c.Tag == "BackyardEast") as Room;
        var southSection = TraverseAllChildren().FirstOrDefault(c => c.Tag == "BackyardSouth") as Room;
        var midSection = TraverseAllChildren().FirstOrDefault(c => c.Tag == "BackyardMid") as Room;
        var deckArea = TraverseAllChildren().FirstOrDefault(c => c.Tag == "BackDeckArea") as Room;

        if (eastSection != null)
        {
            eastSection.SetSideUnanchored(Side.South, _bedroomWindow.GetSide(Side.South) + MainYardDepth);

            var eastWall = TraverseAllChildren().OfType<OuterWall>().FirstOrDefault(w => w.Parent == eastSection && w.WallSide == Side.West);
            if (eastWall != null)
            {
                eastWall.SetSideUnanchored(Side.South, _denWindow.GetSide(Side.South));
            }
        }

        if (southSection != null)
        {
            southSection.SetSideUnanchored(Side.North, _bedroomWindow.GetSide(Side.South) + 0.5f);
            southSection.SetSideUnanchored(Side.West, _bedroomWindow.GetSide(Side.West));

            var southWall = TraverseAllChildren().OfType<OuterWall>().FirstOrDefault(w => w.Parent == southSection && w.WallSide == Side.North);
            if (southWall != null)
            {
                southWall.SetSideUnanchored(Side.East, _kitchenWindow.GetSide(Side.East));
            }
        }

        if (midSection != null)
        {
            midSection.SetSideUnanchored(Side.West, _kitchenWindow.GetSide(Side.East) + 0.0f);
            midSection.SetSideUnanchored(Side.North, _kitchenWindow.GetSide(Side.South));

            // Connect to windows
            midSection.AddConnectingRoom(_kidsBedroomEastWindow, Side.West);
        }

        if (deckArea != null)
        {
            deckArea.SetSideUnanchored(Side.North, _denWindow.GetSide(Side.South));
        }

        if (southSection != null)
        {
            southSection.AddConnectingRoom(_kidsBedroomSouthWindow, Side.North);
        }
    }

    public override void LoadChildren()
    {
        AddChild(new Fence(this, Side.North));       
        new GrassSurface(this, TerrainSurface.DefaultLawn);

        var eastSection = Copy();
        eastSection.Tag = "BackyardEast";
        eastSection.Place().At(this)
            .OnSideInner(Side.North, this)
            .OnSideOuter(Side.East, this);
        eastSection.AdjustShape()
            .SliceFromWest(0, Measure.Feet(10));
        // Position will be set later in SetUpstairsShapes
        eastSection.AddChild(new Fence(eastSection, Side.North));
        eastSection.AddChild(new Fence(eastSection, Side.East));
        new GrassSurface(eastSection, TerrainSurface.DefaultLawn);

        new OuterWall(this, Side.South);
        var eastWall = new OuterWall(eastSection, Side.West);
        eastWall.SetSideUnanchored(Side.North, GetSide(Side.South));
        // eastWall South side will be set later in SetUpstairsShapes

        var southSection = Copy();
        southSection.Tag = "BackyardSouth";
        southSection.Place().At(this);
        // Position will be set later in SetUpstairsShapes
        southSection.SetSideUnanchored(Side.East, eastSection.GetSide(Side.West));
        southSection.SetSideUnanchored(Side.South, eastSection.GetSide(Side.South));        
        new GrassSurface(southSection, TerrainSurface.DefaultLawn);
        var southFence = southSection.AddChild(new Fence(southSection, Side.South));
        southFence.SetSideUnanchored(Side.East, eastSection.GetSide(Side.East));
        southSection.AddChild(new Fence(southSection, Side.West));

        var midSection = Copy();
        midSection.Tag = "BackyardMid";
        midSection.Place().At(this);
        // Position will be set later in SetUpstairsShapes
        midSection.SetSideUnanchored(Side.East, eastSection.GetSide(Side.West));
        midSection.SetSideUnanchored(Side.South, southSection.GetSide(Side.North));
        new GrassSurface(midSection, TerrainSurface.DefaultLawn);

        var deckArea = Copy();
        deckArea.Tag = "BackDeckArea";
        // Position will be set later in SetUpstairsShapes
        deckArea.SetSideUnanchored(Side.West, midSection.GetSide(Side.West));
        deckArea.SetSideUnanchored(Side.East, eastSection.GetSide(Side.West));
        deckArea.SetSideUnanchored(Side.South, southSection.GetSide(Side.North));

        var southEastWall = new OuterWall(midSection, Side.West);
        southEastWall.SetSideUnanchored(Side.North, deckArea.GetSide(Side.North));
        var southWall = new OuterWall(southSection, Side.North);
        // southWall East side will be set later in SetUpstairsShapes

        var deckNorthWall = new OuterWall(deckArea, Side.North);

        // Connections to windows will be set later in SetUpstairsShapes
    }
}
