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

        SetLocalSide(Side.Bottom, northYard.GetLocalSide(Side.Bottom));
        SetLocalSideUnanchored(Side.South, frontSidewalk.GetLocalSide(Side.South) - 0.1f);
        SetLocalSideUnanchored(Side.North, northYard.GetLocalSide(Side.North));
        SetLocalSideUnanchored(Side.East, den.EastPart.GetLocalSide(Side.East) + 1.0f);


        var backSidewalk = AddChild(new Box(Theme, TextureKey.Concrete));
        backSidewalk.AdjustShape().From(frontSidewalk);
        backSidewalk.Place().OnSideOuter(Side.East, frontSidewalk);
        backSidewalk.SetLocalSideUnanchored(Side.East, GetLocalSide(Side.East));

        
        AddChild(new Fence(this, Side.North));
        var northGrass = new GrassSurface(this, TerrainSurface.DefaultLawn);
        northGrass.SetLocalSideUnanchored(Side.South, backSidewalk.GetLocalSide(Side.North));
        northGrass.Terrain.SetLocalSideUnanchored(Side.South, backSidewalk.GetLocalSide(Side.North));

        var northWall = new OuterWall(this, Side.South, moulding: Side.East);
        northWall.SetLocalSideUnanchored(Side.West, frontDeck.GetLocalSide(Side.East));

        var northWall_West = new OuterWall(this, Side.South, moulding: Side.West);        
        northWall_West.AdjustShape().From(northWall);
        northWall_West.SetLocalSideUnanchored(Side.East, eastRoof.GetLocalSide(Side.West));
        northWall_West.VertexOffsets.Add(new VertexOffset(Side.Top | Side.East, new Vector3(0, Roof.RoofHeight, 0)));

        var northWall_Mid = new OuterWall(this, Side.South, moulding: Side.None);
        northWall_Mid.AdjustShape().From(northWall);
        northWall_Mid.SetLocalSideUnanchored(Side.West, eastRoof.GetLocalSide(Side.West));
        northWall_Mid.SetLocalSideUnanchored(Side.East, eastRoof.GetLocalSide(Side.East));
        northWall_Mid.VertexOffsets.Add(new VertexOffset(Side.Top | Side.West, new Vector3(0, Roof.RoofHeight, 0)));

        northWall.SetLocalSideUnanchored(Side.West, northWall_Mid.GetLocalSide(Side.East));

        _eastSection.Place().At(this)
            .OnSideInner(Side.North, this)
            .OnSideOuter(Side.East, this);
        _eastSection.AdjustShape()
            .SliceFromWest(0, Measure.Feet(10));
        _eastSection.SetLocalSideUnanchored(Side.South, southFrontYard.GetLocalSide(Side.South));

        _eastSection.AddChild(new Fence(_eastSection, Side.North));
        _eastSection.AddChild(new Fence(_eastSection, Side.East));
        var eastGrass = new GrassSurface(_eastSection, TerrainSurface.DefaultLawn);

        var eastWall = new OuterWall(_eastSection, Side.West, moulding: Side.South);
        eastWall.SetLocalSideUnanchored(Side.North, GetLocalSide(Side.South));
      
        var eastWall2 = new OuterWall(_eastSection, Side.West, moulding: Side.None);
        eastWall2.AdjustShape().From(eastWall);
        eastWall2.SetLocalSideUnanchored(Side.South, denRoof.GetLocalSide(Side.South));
        eastWall2.VertexOffsets.Add(new VertexOffset(Side.Top | Side.South, new Vector3(0, Roof.RoofHeight, 0)));

        eastWall.SetLocalSideUnanchored(Side.North, denRoof.GetLocalSide(Side.South));
        eastWall.VertexOffsets.Add(new VertexOffset(Side.Top | Side.North, new Vector3(0, Roof.RoofHeight, 0)));

        _southSection.Place().At(this);
        _southSection.SetLocalSideUnanchored(Side.East, _eastSection.GetLocalSide(Side.West));
        _southSection.SetLocalSideUnanchored(Side.South, _eastSection.GetLocalSide(Side.South));
        _southSection.SetLocalSideUnanchored(Side.North, bedroom.GetLocalSide(Side.South) + OuterWall.StandardSpacingForGround);
        _southSection.SetLocalSideUnanchored(Side.West, southFrontYard.GetLocalSide(Side.East));

        var southGrass = new GrassSurface(_southSection, TerrainSurface.DefaultLawn);
        
        var southFence = _southSection.AddChild(new Fence(_southSection, Side.South));
        southFence.SetLocalSideUnanchored(Side.East, _eastSection.GetLocalSide(Side.East));

        var southFence2 = _southSection.AddChild(new Fence(_southSection, Side.South));
        southFence2.AdjustShape().From(southFence)
            .AddToSide(Side.West, Measure.Feet(30));

        southFence.SetLocalSideUnanchored(Side.East, southFence2.GetLocalSide(Side.West) - Measure.Feet(6));


        _southSection.AddChild(new Fence(_southSection, Side.West));

        _midSection.Place().At(this);
        _midSection.SetLocalSideUnanchored(Side.East, _eastSection.GetLocalSide(Side.West));
        _midSection.SetLocalSideUnanchored(Side.South, _southSection.GetLocalSide(Side.North));
        _midSection.SetLocalSideUnanchored(Side.West, kitchen.GetLocalSide(Side.East) + OuterWall.StandardSpacingForGround);
        _midSection.SetLocalSideUnanchored(Side.North, kitchen.GetLocalSide(Side.South));

        var midGrass = new GrassSurface(_midSection, TerrainSurface.DefaultLawn);

        // Position deckArea
        _deckArea.Place().At(this);
        _deckArea.SetLocalSideUnanchored(Side.West, _midSection.GetLocalSide(Side.West));
        _deckArea.SetLocalSideUnanchored(Side.East, _eastSection.GetLocalSide(Side.West));
        _deckArea.SetLocalSideUnanchored(Side.South, _midSection.GetLocalSide(Side.North));
        _deckArea.SetLocalSideUnanchored(Side.North, den.GetLocalSide(Side.South) + OuterWall.StandardSpacingForGround * 1.5f);
        eastWall.SetLocalSideUnanchored(Side.South, _deckArea.GetLocalSide(Side.North));

        new OuterWall(_deckArea, Side.West);

        var southEastWall = new OuterWall(_midSection, Side.West);

        var southWall = new OuterWall(_southSection, Side.North, moulding: Side.West);
        southWall.SetLocalSideUnanchored(Side.East, southEastWall.GetLocalSide(Side.East));
     
        var southWall2 = new OuterWall(_southSection, Side.North, moulding: Side.East);
        southWall2.AdjustShape().From(southWall);

        southWall2.SetLocalSideUnanchored(Side.West, eastRoof.GetLocalSide(Side.West));
        southWall.SetLocalSideUnanchored(Side.East, eastRoof.GetLocalSide(Side.West));
        
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
        eastSidewalk.SetLocalSideUnanchored(Side.South, BackDeck.SideStairs.GetLocalSide(Side.North));
        eastGrass.SetLocalSideUnanchored(Side.West, eastSidewalk.GetLocalSide(Side.East));
        eastGrass.Terrain.SetLocalSideUnanchored(Side.West, eastSidewalk.GetLocalSide(Side.East));

        northGrass.SetLocalSideUnanchored(Side.East, eastGrass.GetLocalSide(Side.West));

        midGrass.SetLocalSideUnanchored(Side.East, eastGrass.GetLocalSide(Side.West));
        southGrass.SetLocalSideUnanchored(Side.East, eastGrass.GetLocalSide(Side.West));


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
