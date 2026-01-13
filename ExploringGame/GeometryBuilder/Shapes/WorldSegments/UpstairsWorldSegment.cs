using ExploringGame.Config;
using ExploringGame.GeometryBuilder.Shapes.Rooms;
using ExploringGame.Texture;
using System;

namespace ExploringGame.GeometryBuilder.Shapes.WorldSegments;

class UpstairsWorldSegment : WorldSegment
{
    public override WorldSegmentTransition[] Transitions { get; } = Array.Empty<WorldSegmentTransition>();

    public UpstairsWorldSegment(TransitionShapesRegistrar transitionShapesRegistrar)
    {
        var upstairsHall = AddChild(new UpstairsHall(this, transitionShapesRegistrar));      
        var basement = AddChild(new Basement(this, null, upstairsHall));
        basement.LoadChildren();
       
       // Transitions = new[] { new WorldSegmentTransition<BasementWorldSegment>(basement.Stairs, Side.North) };
    }
}
