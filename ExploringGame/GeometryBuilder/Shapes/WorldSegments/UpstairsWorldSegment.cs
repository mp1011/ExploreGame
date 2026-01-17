using ExploringGame.Config;
using ExploringGame.GeometryBuilder.Shapes.Rooms;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

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

        var bedroom = new Bedroom(this, upstairsHall);
        bedroom.LoadChildren();

        var kidsBedroom = new KidsBedroom(this, upstairsHall);
        kidsBedroom.LoadChildren();

        var spareRoom = new SpareRoom(this, upstairsHall);
        spareRoom.LoadChildren();

        upstairsHall.LoadChildren();

        Transitions = new[] { new WorldSegmentTransition<BasementWorldSegment>(basement.Stairs, Side.North) };
    }
}
