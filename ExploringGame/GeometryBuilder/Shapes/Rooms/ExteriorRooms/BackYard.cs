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

    public void LoadChildren(Shape frontSidewalk, Shape northYard, Den den, Kitchen kitchen)
    {
        this.Place().At(frontSidewalk)
                   .OnSideOuter(Side.East, frontSidewalk)
                   .OnSideInner(Side.North, northYard);

        SetSide(Side.Bottom, northYard.GetSide(Side.Bottom));
        SetSideUnanchored(Side.South, frontSidewalk.GetSide(Side.South) - 0.6f);
        SetSideUnanchored(Side.North, northYard.GetSide(Side.North));
        SetSideUnanchored(Side.East, den.EastPart.GetSide(Side.East) + 1.0f);
    }

    public void LoadChildrenX(Shape frontSidewalk, Shape northYard, Kitchen kitchen)
    { 
        // Position THIS BackYard shape based on cross-segment dependencies
        this.Place().At(frontSidewalk)
                    .OnSideOuter(Side.East, frontSidewalk)
                    .OnSideInner(Side.North, northYard);

        SetSide(Side.Bottom, northYard.GetSide(Side.Bottom));
        SetSideUnanchored(Side.South, frontSidewalk.GetSide(Side.South) - 0.6f);
        SetSideUnanchored(Side.North, northYard.GetSide(Side.North));
       // SetSideUnanchored(Side.East, denWindow.GetSide(Side.East) + 1.0f);

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
       // _eastSection.SetSideUnanchored(Side.South, bedroomWindow.GetSide(Side.South) + MainYardDepth);

        _eastSection.AddChild(new Fence(_eastSection, Side.North));
        _eastSection.AddChild(new Fence(_eastSection, Side.East));
        new GrassSurface(_eastSection, TerrainSurface.DefaultLawn);

        var eastWall = new OuterWall(_eastSection, Side.West);
        eastWall.SetSideUnanchored(Side.North, GetSide(Side.South));
       // eastWall.SetSideUnanchored(Side.South, denWindow.GetSide(Side.South));

        // Position and add children to southSection
        _southSection.Place().At(this);
        _southSection.SetSideUnanchored(Side.East, _eastSection.GetSide(Side.West));
        _southSection.SetSideUnanchored(Side.South, _eastSection.GetSide(Side.South));

        // Cross-segment positioning
      //  _southSection.SetSideUnanchored(Side.North, bedroomWindow.GetSide(Side.South) + 0.5f);
      //  _southSection.SetSideUnanchored(Side.West, bedroomWindow.GetSide(Side.West));

        new GrassSurface(_southSection, TerrainSurface.DefaultLawn);
        var southFence = _southSection.AddChild(new Fence(_southSection, Side.South));
        southFence.SetSideUnanchored(Side.East, _eastSection.GetSide(Side.East));
        _southSection.AddChild(new Fence(_southSection, Side.West));

        // Position and add children to midSection
        _midSection.Place().At(this);
        _midSection.SetSideUnanchored(Side.East, _eastSection.GetSide(Side.West));
        _midSection.SetSideUnanchored(Side.South, _southSection.GetSide(Side.North));

        // Cross-segment positioning
        _midSection.SetSideUnanchored(Side.West, kitchen.GetSide(Side.East) + 0.0f);
        _midSection.SetSideUnanchored(Side.North, kitchen.GetSide(Side.South));

        new GrassSurface(_midSection, TerrainSurface.DefaultLawn);

        // Position deckArea
        _deckArea.SetSideUnanchored(Side.West, _midSection.GetSide(Side.West));
        _deckArea.SetSideUnanchored(Side.East, _eastSection.GetSide(Side.West));
        _deckArea.SetSideUnanchored(Side.South, _southSection.GetSide(Side.North));

        // Cross-segment positioning
       // _deckArea.SetSideUnanchored(Side.North, denWindow.GetSide(Side.South));

        // Add walls
      ///  var southEastWall = new OuterWall(_midSection, Side.West);
      //  southEastWall.SetSideUnanchored(Side.North, _deckArea.GetSide(Side.North));

        var southWall = new OuterWall(_southSection, Side.North);
        southWall.SetSideUnanchored(Side.East, kitchen.GetSide(Side.East));

      //  new OuterWall(_deckArea, Side.North);
    }
}
