using ExploringGame.Config;
using ExploringGame.GeometryBuilder.Shapes.Rooms;
using System.Linq;

namespace ExploringGame.GeometryBuilder.Shapes.WorldSegments;

class UpstairsWorldSegment : WorldSegment
{
    public override WorldSegmentTransition[] Transitions { get; }

    public UpstairsWorldSegment(TransitionShapesRegistrar transitionShapesRegistrar, BasementWorldSegment basementWorldSegment)
    {
        var upstairsHall = basementWorldSegment.Children.OfType<UpstairsHall>().First();
        var basement = basementWorldSegment.Children.OfType<Basement>().First();

        upstairsHall.LoadChildren();
        var bedroom = new Bedroom(this, upstairsHall);
        bedroom.LoadChildren();

        var kidsBedroom = new KidsBedroom(this, upstairsHall);
        kidsBedroom.LoadChildren();

        var spareRoom = new SpareRoom(this, upstairsHall);
        spareRoom.LoadChildren();

        Transitions = new[] { new WorldSegmentTransition<BasementWorldSegment>(basement.Stairs, Side.North) };
    }

}
