using ExploringGame.Logics.Collision;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

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

        AddPosts(parent, side);
    }

    private void AddPosts(Room parent, Side side)
    {
        var spanAxis = side.GetAxis().Orthogonal();

        float start = spanAxis == Axis.X ? GetSide(Side.West) : GetSide(Side.North);
        float end   = spanAxis == Axis.X ? GetSide(Side.East) : GetSide(Side.South);

        foreach (var spanPos in ComputePostPositions(start, end))
        {
            var postCenter = spanAxis == Axis.X
                ? new Vector3(spanPos, Position.Y, Position.Z)
                : new Vector3(Position.X, Position.Y, spanPos);

            if (!PostExistsAt(parent, postCenter))
            {
                var post = new FencePost(parent);
                post.Width    = Thickness;
                post.Depth    = Thickness;
                post.Height   = FenceHeight;
                post.Position = postCenter;
            }
        }
    }

    private static IEnumerable<float> ComputePostPositions(float start, float end)
    {
        float postSpacing = Measure.Feet(6);

        yield return start;

        float pos = start + postSpacing;
        while (pos < end)
        {
            // Only add this intermediate post if the remaining section is at least 6 ft,
            // so the last section is never shorter than 6 ft (one longer section rule).
            if (end - pos >= postSpacing - 0.001f)
                yield return pos;
            pos += postSpacing;
        }

        yield return end;
    }

    private static bool PostExistsAt(Room parent, Vector3 position)
    {
        return parent.WorldSegment
            .TraverseAllChildren()
            .OfType<FencePost>()
            .Any(p => Vector3.DistanceSquared(p.Position, position) < 0.0001f);
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid();
    }
}
