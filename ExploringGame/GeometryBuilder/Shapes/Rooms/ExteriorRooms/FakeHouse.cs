using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;

public class FakeHouse : Shape
{
    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public override Theme Theme { get; }

    public FakeHouse(BgNeighborhood parent, Shape ground)
    {
        parent.AddChild(this);
        Theme = new Theme(TextureSheetKey.Outdoors, TextureKey.Siding, Color.White);

        Width = Measure.Feet(50);
        Depth = Measure.Feet(40);
        Height = Measure.Feet(30);

        this.Place().At(ground).OnFloor(ground);
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid();
    }
}
