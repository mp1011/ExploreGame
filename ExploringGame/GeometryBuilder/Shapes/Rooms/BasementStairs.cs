using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms;

public class BasementStairs : Stairs, ICutoutShape
{
    public override Theme Theme => new Theme(TextureKey.Ceiling);

    protected override Side StartSide => Side.North;

    public Side ParentCutoutSide => Side.Top;

    public BasementStairs(WorldSegment worldSegment, Room room) 
        : base(worldSegment, new Vector3(x: Measure.Feet(3), y: Measure.Inches(7), z: Measure.Inches(10)))
    {
        Width = Measure.Feet(3);
        Height = room.Height * 2;
        Depth = Measure.Feet(8);
    }

    protected override Shape CreateStep()
    {
        return new Box(TextureKey.Ceiling);
    }

    public Triangle[] Build()
    {
        return BuildCuboid();
    }
}
