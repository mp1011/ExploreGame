using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;

public class Road : Room
{
    public override Side OmitSides => Side.North | Side.South | Side.East | Side.West | Side.Top;

    public override Theme Theme => new RoadTheme();

    public Road(WorldSegment worldSegment) : base(worldSegment)
    {
    }
}
