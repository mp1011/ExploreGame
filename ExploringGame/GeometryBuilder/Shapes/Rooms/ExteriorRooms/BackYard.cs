using ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;
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
    private Kitchen _kitchen;

    private Room _kidsBedroomSouthWindow;
    private Room _kidsBedroomEastWindow;

    private Shape _frontSidewalk;
    private Shape _northYard;

    private Room _eastSection;
    private Room _southSection;
    private Room _midSection;
    private Room _deckArea;

    public override Side OmitSides => Side.South | Side.North | Side.East | Side.West | Side.Top;

    public override Theme Theme { get; } = new YardTheme();

    public BackYard(WorldSegment worldSegment) : base(worldSegment)
    {
        Theme.SideTextures[Side.South] = Theme.GetTexture(TextureKey.Siding);

        Depth = Measure.Feet(20);
        Width = Measure.Feet(20);
        Height = Measure.Feet(20);

        FixedAmbientLight = LightIntensity.Bright;

        // Create child sections (positioning will happen in LoadChildren and SetDependencies)
        _eastSection = Copy();
        _eastSection.Tag = "BackyardEast";

        _southSection = Copy();
        _southSection.Tag = "BackyardSouth";

        _midSection = Copy();
        _midSection.Tag = "BackyardMid";

        _deckArea = Copy();
        _deckArea.Tag = "BackDeckArea";
    }

    public void SetDependencies(Shape frontSidewalk, Shape northYard, Shape denWindow, Room bedroomWindow, Kitchen kitchen,
        Room kidsBedroomSouthWindow, Room kidsBedroomEastWindow)
    {
        // ONLY store cross-segment dependencies - NO positioning here
        _frontSidewalk = frontSidewalk;
        _northYard = northYard;
        _denWindow = denWindow;
        _bedroomWindow = bedroomWindow;
        _kitchen = kitchen;
        _kidsBedroomSouthWindow = kidsBedroomSouthWindow;
        _kidsBedroomEastWindow = kidsBedroomEastWindow;
    }

    private void LoadChildrenX()
    { 
        // Position THIS BackYard shape based on cross-segment dependencies
        this.Place().At(_frontSidewalk)
                    .OnSideOuter(Side.East, _frontSidewalk)
                    .OnSideInner(Side.North, _northYard);

        SetSide(Side.Bottom, _northYard.GetSide(Side.Bottom));
        SetSideUnanchored(Side.South, _frontSidewalk.GetSide(Side.South) - 0.6f);
        SetSideUnanchored(Side.North, _northYard.GetSide(Side.North));
        SetSideUnanchored(Side.East, _denWindow.GetSide(Side.East) + 1.0f);

        // Add children to main BackYard
        AddChild(new Fence(this, Side.North));       
        new GrassSurface(this, TerrainSurface.DefaultLawn);
        new OuterWall(this, Side.South);

        // Position and add children to eastSection (relative positioning first)
        _eastSection.Place().At(this)
            .OnSideInner(Side.North, this)
            .OnSideOuter(Side.East, this);
        _eastSection.AdjustShape()
            .SliceFromWest(0, Measure.Feet(10));

        // Cross-segment positioning (using dependencies set by SetDependencies)
        _eastSection.SetSideUnanchored(Side.South, _bedroomWindow.GetSide(Side.South) + MainYardDepth);

        _eastSection.AddChild(new Fence(_eastSection, Side.North));
        _eastSection.AddChild(new Fence(_eastSection, Side.East));
        new GrassSurface(_eastSection, TerrainSurface.DefaultLawn);

        var eastWall = new OuterWall(_eastSection, Side.West);
        eastWall.SetSideUnanchored(Side.North, GetSide(Side.South));
        eastWall.SetSideUnanchored(Side.South, _denWindow.GetSide(Side.South));

        // Position and add children to southSection
        _southSection.Place().At(this);
        _southSection.SetSideUnanchored(Side.East, _eastSection.GetSide(Side.West));
        _southSection.SetSideUnanchored(Side.South, _eastSection.GetSide(Side.South));

        // Cross-segment positioning
        _southSection.SetSideUnanchored(Side.North, _bedroomWindow.GetSide(Side.South) + 0.5f);
        _southSection.SetSideUnanchored(Side.West, _bedroomWindow.GetSide(Side.West));

        new GrassSurface(_southSection, TerrainSurface.DefaultLawn);
        var southFence = _southSection.AddChild(new Fence(_southSection, Side.South));
        southFence.SetSideUnanchored(Side.East, _eastSection.GetSide(Side.East));
        _southSection.AddChild(new Fence(_southSection, Side.West));

        // Position and add children to midSection
        _midSection.Place().At(this);
        _midSection.SetSideUnanchored(Side.East, _eastSection.GetSide(Side.West));
        _midSection.SetSideUnanchored(Side.South, _southSection.GetSide(Side.North));

        // Cross-segment positioning
        _midSection.SetSideUnanchored(Side.West, _kitchen.GetSide(Side.East) + 0.0f);
        _midSection.SetSideUnanchored(Side.North, _kitchen.GetSide(Side.South));

        new GrassSurface(_midSection, TerrainSurface.DefaultLawn);

        // Position deckArea
        _deckArea.SetSideUnanchored(Side.West, _midSection.GetSide(Side.West));
        _deckArea.SetSideUnanchored(Side.East, _eastSection.GetSide(Side.West));
        _deckArea.SetSideUnanchored(Side.South, _southSection.GetSide(Side.North));

        // Cross-segment positioning
        _deckArea.SetSideUnanchored(Side.North, _denWindow.GetSide(Side.South));

        // Add walls
        var southEastWall = new OuterWall(_midSection, Side.West);
        southEastWall.SetSideUnanchored(Side.North, _deckArea.GetSide(Side.North));

        var southWall = new OuterWall(_southSection, Side.North);
        southWall.SetSideUnanchored(Side.East, _kitchen.GetSide(Side.East));

        var deckNorthWall = new OuterWall(_deckArea, Side.North);

        // Add room connections to windows
        _midSection.AddConnectingRoom(_kidsBedroomEastWindow, Side.West);
        _southSection.AddConnectingRoom(_kidsBedroomSouthWindow, Side.North);
    }
}
