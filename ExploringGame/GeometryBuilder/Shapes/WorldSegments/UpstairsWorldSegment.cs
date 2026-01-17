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

        //var dummyRoom = new Room(this, new Theme(Color.Purple));
        //dummyRoom.Position = upstairsHall.Position;
        //dummyRoom.Size = upstairsHall.Size;
        //dummyRoom.Depth = 2.0f;
        //dummyRoom.Place().OnSideOuter(Side.West, upstairsHall);
        //upstairsHall.AddConnectingRoom(new RoomConnection(upstairsHall, dummyRoom, Side.West));
       
        Transitions = new[] { new WorldSegmentTransition<BasementWorldSegment>(basement.Stairs, Side.North) };
    }
}
