using ExploringGame.Config;
using ExploringGame.GeometryBuilder.Shapes.Rooms;

namespace ExploringGame.GeometryBuilder.Shapes.WorldSegments;

class UpstairsWorldSegment : WorldSegment
{
    public override WorldSegmentTransition[] Transitions { get; }

    public UpstairsWorldSegment(TransitionShapesRegistrar transitionShapesRegistrar)
    {
        var upstairsHall = AddChild(new UpstairsHall(this, transitionShapesRegistrar));      
        var basement = AddChild(new Basement(this, null, upstairsHall));
        transitionShapesRegistrar.RecallPositionAndSize(basement);
        basement.LoadChildren();
       
        Transitions = new[] { new WorldSegmentTransition<BasementWorldSegment>(basement.Stairs, Side.North) };
    }
}
