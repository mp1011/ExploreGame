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
using System.Linq;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;

public class BackYard : Room
{
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

        // Create child sections (positioning will happen in LoadChildren and SetDependencies)
        _eastSection = Copy(inheritLightingGroup: false);
        _eastSection.Tag = "BackyardEast";

        _southSection = Copy(inheritLightingGroup: false);
        _southSection.Tag = "BackyardSouth";

        _midSection = Copy(inheritLightingGroup: false);
        _midSection.Tag = "BackyardMid";

        _deckArea = Copy(inheritLightingGroup: false);
        _deckArea.Tag = "BackDeckArea";

        AddConnectingRoom(_eastSection, Side.None);
        AddConnectingRoom(_southSection, Side.None);
        AddConnectingRoom(_midSection, Side.None);
        AddConnectingRoom(_deckArea, Side.None);

        _eastSection.AddConnectingRoom(_midSection, Side.None);
        _eastSection.AddConnectingRoom(_deckArea, Side.None);
        _eastSection.AddConnectingRoom(_southSection, Side.None);

    }

    public void LoadChildren(Shape frontSidewalk, Shape northYard, FrontDeck frontDeck, Den den, Kitchen kitchen, KidsBedroom kidsBedroom, Bedroom bedroom, 
        Basement basement, BasementOffice basementOffice, Roof eastRoof, Roof denRoof, Room southFrontYard, HalfBathroom halfBath)
    {
        this.Place().At(frontSidewalk)
                    .OnFloor()
                   .OnSideOuter(Side.East, frontSidewalk)
                   .OnSideInner(Side.North, northYard);

        SetWorldSide(Side.Bottom, northYard.GetWorldSide(Side.Bottom));
        SetWorldSideUnanchored(Side.South, frontSidewalk.GetWorldSide(Side.South) - 0.1f);
        SetWorldSideUnanchored(Side.North, northYard.GetWorldSide(Side.North));
        SetWorldSideUnanchored(Side.East, den.EastPart.GetWorldSide(Side.East) + 1.0f);


        var backSidewalk = AddChild(new Box(Theme, TextureKey.Concrete));
        backSidewalk.AdjustShape().From(frontSidewalk);
        backSidewalk.Place().OnSideOuter(Side.East, frontSidewalk);
        backSidewalk.SetWorldSideUnanchored(Side.East, GetWorldSide(Side.East));

        
        AddChild(new Fence(this, Side.North));
        var northGrass = new GrassSurface(this, TerrainSurface.DefaultLawn);
        northGrass.SetWorldSideUnanchored(Side.South, backSidewalk.GetWorldSide(Side.North));
        northGrass.Terrain.SetWorldSideUnanchored(Side.South, backSidewalk.GetWorldSide(Side.North));

        var northWall = new OuterWall(this, Side.South, moulding: Side.East);
        northWall.SetWorldSideUnanchored(Side.West, frontDeck.GetWorldSide(Side.East));

        var northWall_West = new OuterWall(this, Side.South, moulding: Side.West);        
        northWall_West.AdjustShape().From(northWall);
        northWall_West.SetWorldSideUnanchored(Side.East, eastRoof.GetWorldSide(Side.West));
        northWall_West.VertexOffsets.Add(new VertexOffset(Side.Top | Side.East, new Vector3(0, Roof.RoofHeight, 0)));

        var northWall_Mid = new OuterWall(this, Side.South, moulding: Side.None);
        northWall_Mid.AdjustShape().From(northWall);
        northWall_Mid.SetWorldSideUnanchored(Side.West, eastRoof.GetWorldSide(Side.West));
        northWall_Mid.SetWorldSideUnanchored(Side.East, eastRoof.GetWorldSide(Side.East));
        northWall_Mid.VertexOffsets.Add(new VertexOffset(Side.Top | Side.West, new Vector3(0, Roof.RoofHeight, 0)));

        northWall.SetWorldSideUnanchored(Side.West, northWall_Mid.GetWorldSide(Side.East));

        _eastSection.Place().At(this)
            .OnSideInner(Side.North, this)
            .OnSideOuter(Side.East, this);
        _eastSection.AdjustShape()
            .SliceFromWest(0, Measure.Feet(10));
        _eastSection.SetWorldSideUnanchored(Side.South, southFrontYard.GetWorldSide(Side.South));

        _eastSection.AddChild(new Fence(_eastSection, Side.North));
        _eastSection.AddChild(new Fence(_eastSection, Side.East));
        var eastGrass = new GrassSurface(_eastSection, TerrainSurface.DefaultLawn);

        var eastWall = new OuterWall(_eastSection, Side.West, moulding: Side.South);
        eastWall.SetWorldSideUnanchored(Side.North, GetWorldSide(Side.South));
      
        var eastWall2 = new OuterWall(_eastSection, Side.West, moulding: Side.None);
        eastWall2.AdjustShape().From(eastWall);
        eastWall2.SetWorldSideUnanchored(Side.South, denRoof.GetWorldSide(Side.South));
        eastWall2.VertexOffsets.Add(new VertexOffset(Side.Top | Side.South, new Vector3(0, Roof.RoofHeight, 0)));

        eastWall.SetWorldSideUnanchored(Side.North, denRoof.GetWorldSide(Side.South));
        eastWall.VertexOffsets.Add(new VertexOffset(Side.Top | Side.North, new Vector3(0, Roof.RoofHeight, 0)));

        _southSection.Place().At(this);
        _southSection.SetWorldSideUnanchored(Side.East, _eastSection.GetWorldSide(Side.West));
        _southSection.SetWorldSideUnanchored(Side.South, _eastSection.GetWorldSide(Side.South));
        _southSection.SetWorldSideUnanchored(Side.North, bedroom.GetWorldSide(Side.South) + OuterWall.StandardSpacingForGround);
        _southSection.SetWorldSideUnanchored(Side.West, southFrontYard.GetWorldSide(Side.East));

        var southGrass = new GrassSurface(_southSection, TerrainSurface.DefaultLawn);
        
        var southFence = _southSection.AddChild(new Fence(_southSection, Side.South));
        southFence.SetWorldSideUnanchored(Side.East, _eastSection.GetWorldSide(Side.East));

        var southFence2 = _southSection.AddChild(new Fence(_southSection, Side.South));
        southFence2.AdjustShape().From(southFence)
            .AddToSide(Side.West, Measure.Feet(30));

        southFence.SetWorldSideUnanchored(Side.East, southFence2.GetWorldSide(Side.West) - Measure.Feet(6));


        _southSection.AddChild(new Fence(_southSection, Side.West));

        _midSection.Place().At(this);
        _midSection.SetWorldSideUnanchored(Side.East, _eastSection.GetWorldSide(Side.West));
        _midSection.SetWorldSideUnanchored(Side.South, _southSection.GetWorldSide(Side.North));
        _midSection.SetWorldSideUnanchored(Side.West, kitchen.GetWorldSide(Side.East) + OuterWall.StandardSpacingForGround);
        _midSection.SetWorldSideUnanchored(Side.North, kitchen.GetWorldSide(Side.South));

        var midGrass = new GrassSurface(_midSection, TerrainSurface.DefaultLawn);

        // Position deckArea
        _deckArea.Place().At(this);
        _deckArea.SetWorldSideUnanchored(Side.West, _midSection.GetWorldSide(Side.West));
        _deckArea.SetWorldSideUnanchored(Side.East, _eastSection.GetWorldSide(Side.West));
        _deckArea.SetWorldSideUnanchored(Side.South, _midSection.GetWorldSide(Side.North));
        _deckArea.SetWorldSideUnanchored(Side.North, den.GetWorldSide(Side.South) + OuterWall.StandardSpacingForGround * 1.5f);
        eastWall.SetWorldSideUnanchored(Side.South, _deckArea.GetWorldSide(Side.North));

        new OuterWall(_deckArea, Side.West);

        var southEastWall = new OuterWall(_midSection, Side.West);

        var southWall = new OuterWall(_southSection, Side.North, moulding: Side.West);
        southWall.SetWorldSideUnanchored(Side.East, southEastWall.GetWorldSide(Side.East));
     
        var southWall2 = new OuterWall(_southSection, Side.North, moulding: Side.East);
        southWall2.AdjustShape().From(southWall);

        southWall2.SetWorldSideUnanchored(Side.West, eastRoof.GetWorldSide(Side.West));
        southWall.SetWorldSideUnanchored(Side.East, eastRoof.GetWorldSide(Side.West));
        
        southWall.VertexOffsets.Add(new VertexOffset(Side.Top | Side.East, new Vector3(0, Roof.RoofHeight, 0)));
        southWall2.VertexOffsets.Add(new VertexOffset(Side.Top | Side.West, new Vector3(0, Roof.RoofHeight, 0)));


        southWall2.Tag = "SouthWall";



        new OuterWall(_deckArea, Side.North);

        new Window(kitchen, Side.East, Measure.Feet(4), Measure.Feet(4), HAlign.Right, -Measure.Feet(2), otherRoom: _deckArea, style: WindowStyle.Plain);    
        new Window(den, Side.South, Measure.Feet(4), Measure.Feet(4), HAlign.Left, Measure.Feet(2), otherRoom: _deckArea);
        new Window(kidsBedroom, Side.South, Measure.Feet(3), Measure.Feet(4), otherRoom: _southSection);
        new Window(kidsBedroom, Side.East, Measure.Feet(3), Measure.Feet(4), otherRoom: _midSection);       
        new Window(bedroom, Side.South, Measure.Feet(4), Measure.Feet(4), otherRoom: _southSection);
        new Window(halfBath, Side.East, Measure.Feet(4), Measure.Feet(4), otherRoom: _eastSection, style: WindowStyle.Plain);

        new BasementWindow(basement, this, Side.North, HAlign.Right, -0.5f);
        new BasementWindow(basementOffice.EastPart, _eastSection, Side.East, HAlign.Left, 0.0f);

        BackDeck = new BackDeck(WorldSegment, _deckArea, den);

        BackDeck.AddConnectingRoom(_deckArea, Side.None);

        var eastSidewalk = AddChild(new Box(Theme, TextureKey.Concrete));
        eastSidewalk.AdjustShape().From(backSidewalk).SetAxis(Axis.X, BackDeck.SideStairs.Depth);
        eastSidewalk.Place().OnSideInner(Side.North, backSidewalk)
            .OnSideOuter(Side.East, backSidewalk);
        eastSidewalk.SetWorldSideUnanchored(Side.South, BackDeck.SideStairs.GetWorldSide(Side.North));
        eastGrass.SetWorldSideUnanchored(Side.West, eastSidewalk.GetWorldSide(Side.East));
        eastGrass.Terrain.SetWorldSideUnanchored(Side.West, eastSidewalk.GetWorldSide(Side.East));

        northGrass.SetWorldSideUnanchored(Side.East, eastGrass.GetWorldSide(Side.West));

        midGrass.SetWorldSideUnanchored(Side.East, eastGrass.GetWorldSide(Side.West));
        southGrass.SetWorldSideUnanchored(Side.East, eastGrass.GetWorldSide(Side.West));


        var neighborLight = new NeighborLight(this);
        neighborLight.Place().At(this).OnSideOuter(Side.North);
        neighborLight.LocalY = this.LocalY + Measure.Feet(14);
        neighborLight.LocalX += Measure.Feet(8);
        neighborLight.LocalZ -= Measure.Feet(8);

        var neighborLight2 = new NeighborLight(_eastSection);
        neighborLight2.Place().At(_eastSection).OnSideOuter(Side.East);
        neighborLight2.LocalY = this.LocalY + Measure.Feet(12);
        neighborLight2.LocalX += Measure.Feet(8);
    }

}
