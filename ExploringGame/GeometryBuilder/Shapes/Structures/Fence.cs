using ExploringGame.Logics.Collision;
using ExploringGame.Services;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.Structures;

public class Fence : Shape, ICollidable
{
    public readonly float Thickness = Measure.Inches(8);
    public readonly float FenceHeight = Measure.Feet(10);

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public CollisionGroup CollisionGroup => CollisionGroup.Environment;

    public CollisionGroup CollidesWithGroups => CollisionGroup.MovingObjects;

    public override Theme Theme => new FenceTheme();

    public Fence(Room parent, Side side)
    {
        parent.AddChild(this);

        this.AdjustShape().From(parent).SetAxis(side.GetAxis(), Thickness);
        Height = FenceHeight;

        this.Place().OnFloor().OnSideOuter(side);

        SetSideUnanchored(side.ClockwiseTurn(), parent.GetSide(side.ClockwiseTurn()));
        SetSideUnanchored(side.CounterClockwiseTurn(), parent.GetSide(side.CounterClockwiseTurn()));
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid();
    }
}
