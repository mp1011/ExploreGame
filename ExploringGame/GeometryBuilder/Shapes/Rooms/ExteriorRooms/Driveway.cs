using ExploringGame.GeometryBuilder.Shapes.Rooms.BasementRooms;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;

public class Driveway : Room
{
    public override Theme Theme => new RoadTheme();

    public override Side OmitSides => Side.Top | Side.South | Side.North | Side.East | Side.West;

    public Driveway(WorldSegment worldSegment, FrontYard yard, Garage garage) : base(worldSegment)
    {
        FixedAmbientLight = LightIntensity.Bright;

        Depth = garage.Depth;
        Height = 0.5f;
        Width = 1.0f;

        SetSideUnanchored(Side.Top, yard.GetSide(Side.Top));

        this.Place().OnSideInner(Side.East, yard.Deck)
                    .OnSideInner(Side.North, garage)
                    .OnFloor(garage);

        SetSideUnanchored(Side.West, yard.GetSide(Side.West));

        var yOffset = yard.GetSide(Side.Bottom) - GetSide(Side.Bottom);
        VertexOffsets.Add(new VertexOffset(Side.West, new Vector3(0f, yOffset, 0f)));
    }
}
