using ExploringGame.Logics.Collision;
using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.GeometryBuilder.Shapes.Structures;

public class Fence : Shape, ICollidable
{
    public readonly float Thickness = Measure.Inches(8);
    public readonly float PostThickness = Measure.Inches(12);
    public readonly float FenceHeight = Measure.Feet(10);
    public readonly float PostHeight = Measure.Feet(12);
    public readonly float PostSpacing = Measure.Feet(12);


    /// <summary>
    /// Tolerance (in world units) used when snapping post intervals and detecting duplicate posts.
    /// Equivalent to ~2.5 mm, well within floating-point precision for fixed room geometry.
    /// </summary>
    private const float PostPositionTolerance = 0.001f;

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public CollisionGroup CollisionGroup => CollisionGroup.Environment;

    public CollisionGroup CollidesWithGroups => CollisionGroup.MovingObjects;

    public override IColliderMaker ColliderMaker => new BoundingBoxColliderMaker(this);

    public override Theme Theme => new FenceTheme();

    public Fence(Room parent, Side side)
    {
        parent.AddChild(this);

        this.AdjustShape().From(parent).SetAxis(side.GetAxis(), Thickness);
        Height = FenceHeight;

        this.Place().OnFloor().OnSideOuter(side);

        SetLocalSideUnanchored(side.ClockwiseTurn(), parent.GetLocalSide(side.ClockwiseTurn()));
        SetLocalSideUnanchored(side.CounterClockwiseTurn(), parent.GetLocalSide(side.CounterClockwiseTurn()));

        AddPosts(parent, side);
    }

    private void AddPosts(Room parent, Side side)
    {
        var spanAxis = side.GetAxis().Orthogonal();

        float start = spanAxis == Axis.X ? GetLocalSide(Side.West) : GetLocalSide(Side.North);
        float end   = spanAxis == Axis.X ? GetLocalSide(Side.East) : GetLocalSide(Side.South);

        foreach (var spanPos in ComputePostPositions(start, end))
        {
            var postCenter = spanAxis == Axis.X
                ? new Vector3(spanPos, LocalPosition.Y, LocalPosition.Z)
                : new Vector3(LocalPosition.X, LocalPosition.Y, spanPos);

            if (!PostExistsAt(parent, postCenter))
            {
                var post = new FencePost(parent);
                post.Width    = PostThickness;
                post.Depth    = PostThickness;
                post.Height   = PostHeight;
                post.LocalPosition = postCenter;
            }
        }
    }

    private IEnumerable<float> ComputePostPositions(float start, float end)
    {
        yield return start;

        float pos = start + PostSpacing;
        while (pos < end)
        {
            // Only add this intermediate post if the remaining section is at least 6 ft,
            // so the last section is never shorter than 6 ft (one longer section rule).
            if (end - pos >= PostSpacing - PostPositionTolerance)
                yield return pos;
            pos += PostSpacing;
        }

        yield return end;
    }

    private static bool PostExistsAt(Room parent, Vector3 position)
    {
        return parent.WorldSegment
            .TraverseAllChildren()
            .OfType<FencePost>()
            .Any(p => Vector3.DistanceSquared(p.LocalPosition, position) < PostPositionTolerance * PostPositionTolerance);
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid();
    }
}
