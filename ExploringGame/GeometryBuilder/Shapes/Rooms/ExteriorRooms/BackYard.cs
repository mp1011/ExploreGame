using ExploringGame.GeometryBuilder.Shapes.Structures;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Services;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;

public class BackYard : Room
{
    public override Side OmitSides => Side.North | Side.East | Side.West | Side.Top;

    public override Theme Theme { get; } = new YardTheme();

    public BackYard(WorldSegment worldSegment, Shape frontSidewalk, Shape northYard, Shape den) : base(worldSegment)
    {
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
        var fence = AddChild(new Fence(this, Side.North));
    }
}
