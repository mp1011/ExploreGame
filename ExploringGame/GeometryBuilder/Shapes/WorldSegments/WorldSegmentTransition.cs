using System;

namespace ExploringGame.GeometryBuilder.Shapes.WorldSegments;

public abstract record WorldSegmentTransition(Type WorldSegmentType, Type ShapeType, Side ExitSide)
{
}

public record WorldSegmentTransition<TSegment, TShape>(Side ExitSide) : WorldSegmentTransition(typeof(TSegment), typeof(TShape), ExitSide)
    where TSegment : WorldSegment, new()
    where TShape : Shape
{ }
