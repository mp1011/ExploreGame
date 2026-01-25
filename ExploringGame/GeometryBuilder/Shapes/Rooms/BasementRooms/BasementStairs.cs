using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.BasementRooms;

public class BasementStairs : Stairs, ICutoutShape
{
    public override Theme Theme => new Theme(TextureKey.Ceiling);

    protected override Side StartSide => Side.North;

    public Side ParentCutoutSide => Side.Top;

    protected override Side OmitSides => Side.South | Side.North;

    public Shape CutoutTarget => BottomFloor;

    public BasementStairs(WorldSegment worldSegment, Shape bottomFloor, Shape topFloor) 
        : base(worldSegment, new Vector2(x: Measure.Feet(3), y: Measure.Inches(10)), bottomFloor, topFloor, 
            width: Measure.Feet(3), depth: Measure.Feet(8))
    {

        var offset = topFloor.GetSide(Side.Top) - bottomFloor.GetSide(Side.Top);
        VertexOffsets.Add(new VertexOffset(Side.NorthWest, new Vector3(0, -offset, 0)));
        VertexOffsets.Add(new VertexOffset(Side.NorthEast, new Vector3(0, -offset, 0)));
    }

    protected override StairStep CreateStep()
    {
        return new StairStep { MainTexture = new TextureInfo(TextureKey.Ceiling) };
    }

    public Triangle[] Build()
    {
        return BuildCuboid();
    }
}
