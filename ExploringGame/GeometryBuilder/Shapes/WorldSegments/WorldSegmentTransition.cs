using System;

namespace ExploringGame.GeometryBuilder.Shapes.WorldSegments;

public abstract record WorldSegmentTransition(Type WorldSegmentType, Shape ExitShape, Side ExitSide)
{
}

public record WorldSegmentTransition<TSegment>(Shape ExitShape, Side ExitSide) : WorldSegmentTransition(typeof(TSegment), ExitShape, ExitSide)
    where TSegment : WorldSegment
{ }
