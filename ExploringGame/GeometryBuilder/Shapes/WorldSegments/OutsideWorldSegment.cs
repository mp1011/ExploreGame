using System.Collections.Generic;

namespace ExploringGame.GeometryBuilder.Shapes.WorldSegments;

public class OutsideWorldSegment : WorldSegment
{
    public override IReadOnlyList<WorldSegmentTransition> Transitions { get; } = new[]
    {
        new WorldSegmentTransition(typeof(UpstairsWorldSegment))
    };

    public OutsideWorldSegment() : base()
    {
        Depth = Measure.Feet(100);
        Width = Measure.Feet(100);
        Height = Measure.Feet(20);
        SetSide(Side.Bottom, UpstairsWorldSegment.FloorY - Measure.Feet(4));
    }
}
