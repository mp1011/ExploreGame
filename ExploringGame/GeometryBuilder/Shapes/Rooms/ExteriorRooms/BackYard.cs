using ExploringGame.GeometryBuilder.Shapes.Structures;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Services;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;

public class BackYard : Room
{
    private static readonly float MainYardDepth = Measure.Feet(40);
    private readonly Shape _den;
    private readonly Shape _bedroom;
    private readonly Shape _kitchen;

    public override Side OmitSides => Side.South | Side.North | Side.East | Side.West | Side.Top;

    public override Theme Theme { get; } = new YardTheme();

    public BackYard(WorldSegment worldSegment, Shape frontSidewalk, Shape northYard, Shape den, Shape bedroom, Shape kitchen) : base(worldSegment)
    {
        _den = den;
        _bedroom = bedroom;
        _kitchen = kitchen;
        Theme.SideTextures[Side.South] = Theme.GetTexture(TextureKey.Siding);

        Depth = Measure.Feet(20);
        Width = Measure.Feet(20);
        Height = Measure.Feet(20);

        FixedAmbientLight = LightIntensity.Bright;

        this.Place().At(frontSidewalk)
                    .OnSideOuter(Side.East, frontSidewalk)
                    .OnSideInner(Side.North, northYard);

        SetSide(Side.Bottom, northYard.GetSide(Side.Bottom));
        SetSideUnanchored(Side.South, frontSidewalk.GetSide(Side.South) - 0.6f);
        SetSideUnanchored(Side.North, northYard.GetSide(Side.North));

        SetSideUnanchored(Side.East, den.GetSide(Side.East) + 1.0f);
    }

    public override void LoadChildren()
    {
        AddChild(new Fence(this, Side.North));       
        new GrassSurface(this, TerrainSurface.DefaultLawn);

        var eastSection = Copy();
        eastSection.Place().At(this)
            .OnSideInner(Side.North, this)
            .OnSideOuter(Side.East, this);
        eastSection.AdjustShape()
            .SliceFromWest(0, Measure.Feet(10));
        eastSection.SetSideUnanchored(Side.South, _bedroom.GetSide(Side.South) + MainYardDepth);
        eastSection.AddChild(new Fence(eastSection, Side.North));
        eastSection.AddChild(new Fence(eastSection, Side.East));
        new GrassSurface(eastSection, TerrainSurface.DefaultLawn);

        new OuterWall(this, Side.South);
        var eastWall = new OuterWall(eastSection, Side.West);
        eastWall.SetSideUnanchored(Side.North, GetSide(Side.South));
        eastWall.SetSideUnanchored(Side.South, _den.GetSide(Side.South));

        var southSection = Copy();
        southSection.Place().At(this);
        southSection.SetSideUnanchored(Side.North, _bedroom.GetSide(Side.South) + 0.5f);
        southSection.SetSideUnanchored(Side.West, _bedroom.GetSide(Side.West));
        southSection.SetSideUnanchored(Side.East, eastSection.GetSide(Side.West));
        southSection.SetSideUnanchored(Side.South, eastSection.GetSide(Side.South));        
        new GrassSurface(southSection, TerrainSurface.DefaultLawn);
        var southFence = southSection.AddChild(new Fence(southSection, Side.South));
        southFence.SetSideUnanchored(Side.East, eastSection.GetSide(Side.East));
        southSection.AddChild(new Fence(southSection, Side.West));

        var midSection = Copy();
        midSection.Place().At(this);
        midSection.SetSideUnanchored(Side.West, _kitchen.GetSide(Side.East) + 0.5f);
        midSection.SetSideUnanchored(Side.East, eastSection.GetSide(Side.West));
        midSection.SetSideUnanchored(Side.South, southSection.GetSide(Side.North));
        midSection.SetSideUnanchored(Side.North, _kitchen.GetSide(Side.South));
        new GrassSurface(midSection, TerrainSurface.DefaultLawn);


    }
}
