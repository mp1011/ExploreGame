using ExploringGame.GeometryBuilder.Shapes.Rooms.BasementRooms;
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
    private Room _eastSection;
    private Room _southSection;
    private Room _midSection;
    private Room _deckArea;

    public override Side OmitSides => Side.South | Side.North | Side.East | Side.West | Side.Top;

    public override Theme Theme { get; } = new YardTheme();

    public BackDeck BackDeck { get; private set; }

    public Room DeckArea => _deckArea;

    public BackYard(WorldSegment worldSegment) : base(worldSegment)
    {
        Theme.SideTextures[Side.South] = Theme.GetTexture(TextureKey.Siding);

        Depth = Measure.Feet(20);
        Width = Measure.Feet(20);
        Height = 5.52f;

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

    public void LoadChildren(Shape frontSidewalk, Shape northYard, FrontDeck frontDeck, Den den, Kitchen kitchen, KidsBedroom kidsBedroom, Bedroom bedroom, 
        Basement basement, BasementOffice basementOffice)
    {
        this.Place().At(frontSidewalk)
                    .OnFloor()
                   .OnSideOuter(Side.East, frontSidewalk)
                   .OnSideInner(Side.North, northYard);

        SetSide(Side.Bottom, northYard.GetSide(Side.Bottom));
        SetSideUnanchored(Side.South, frontSidewalk.GetSide(Side.South) - 0.1f);
        SetSideUnanchored(Side.North, northYard.GetSide(Side.North));
        SetSideUnanchored(Side.East, den.EastPart.GetSide(Side.East) + 1.0f);

        AddChild(new Fence(this, Side.North));
        new GrassSurface(this, TerrainSurface.DefaultLawn);
        var northWall = new OuterWall(this, Side.South);
        northWall.SetSideUnanchored(Side.West, frontDeck.GetSide(Side.East));

        _eastSection.Place().At(this)
            .OnSideInner(Side.North, this)
            .OnSideOuter(Side.East, this);
        _eastSection.AdjustShape()
            .SliceFromWest(0, Measure.Feet(10));
        _eastSection.SetSideUnanchored(Side.South, bedroom.GetSide(Side.South) + MainYardDepth);

        _eastSection.AddChild(new Fence(_eastSection, Side.North));
        _eastSection.AddChild(new Fence(_eastSection, Side.East));
        new GrassSurface(_eastSection, TerrainSurface.DefaultLawn);

        var eastWall = new OuterWall(_eastSection, Side.West);
        eastWall.SetSideUnanchored(Side.North, GetSide(Side.South));
      
        _southSection.Place().At(this);
        _southSection.SetSideUnanchored(Side.East, _eastSection.GetSide(Side.West));
        _southSection.SetSideUnanchored(Side.South, _eastSection.GetSide(Side.South));
        _southSection.SetSideUnanchored(Side.North, bedroom.GetSide(Side.South) + OuterWall.StandardSpacingForGround);
        _southSection.SetSideUnanchored(Side.West, bedroom.GetSide(Side.West));

        new GrassSurface(_southSection, TerrainSurface.DefaultLawn);
        var southFence = _southSection.AddChild(new Fence(_southSection, Side.South));
        southFence.SetSideUnanchored(Side.East, _eastSection.GetSide(Side.East));
        _southSection.AddChild(new Fence(_southSection, Side.West));

        _midSection.Place().At(this);
        _midSection.SetSideUnanchored(Side.East, _eastSection.GetSide(Side.West));
        _midSection.SetSideUnanchored(Side.South, _southSection.GetSide(Side.North));
        _midSection.SetSideUnanchored(Side.West, kitchen.GetSide(Side.East) + OuterWall.StandardSpacingForGround);
        _midSection.SetSideUnanchored(Side.North, kitchen.GetSide(Side.South));

        new GrassSurface(_midSection, TerrainSurface.DefaultLawn);

        // Position deckArea
        _deckArea.Place().At(this);
        _deckArea.SetSideUnanchored(Side.West, _midSection.GetSide(Side.West));
        _deckArea.SetSideUnanchored(Side.East, _eastSection.GetSide(Side.West));
        _deckArea.SetSideUnanchored(Side.South, _midSection.GetSide(Side.North));
        _deckArea.SetSideUnanchored(Side.North, den.GetSide(Side.South) + OuterWall.StandardSpacingForGround * 1.5f);
        eastWall.SetSideUnanchored(Side.South, _deckArea.GetSide(Side.North));

        new OuterWall(_deckArea, Side.West);

        var southEastWall = new OuterWall(_midSection, Side.West);

        var southWall = new OuterWall(_southSection, Side.North);
        southWall.SetSideUnanchored(Side.East, southEastWall.GetSide(Side.East));
        southWall.Tag = "SouthWall";

        new OuterWall(_deckArea, Side.North);

        new Window(kitchen, Side.East, Measure.Feet(4), Measure.Feet(4), HAlign.Right, -Measure.Feet(2), otherRoom: _deckArea);
        new Window(den, Side.South, Measure.Feet(4), Measure.Feet(4), HAlign.Left, Measure.Feet(2), otherRoom: _deckArea);
        new Window(kidsBedroom, Side.South, Measure.Feet(3), Measure.Feet(4), otherRoom: _southSection);
        new Window(kidsBedroom, Side.East, Measure.Feet(3), Measure.Feet(4), otherRoom: _midSection);       
        new Window(bedroom, Side.South, Measure.Feet(4), Measure.Feet(4), otherRoom: _southSection);

        new BasementWindow(basement, this, Side.North, HAlign.Right, -0.5f);
        new BasementWindow(basementOffice.EastPart, _eastSection, Side.East, HAlign.Left, 0.0f);

        BackDeck = new BackDeck(WorldSegment, _deckArea, den);

    }

}
