using ExploringGame.GeometryBuilder.Shapes.Rooms.BasementRooms;
using ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.WorldSegments
{
    public class BasementWorldSegment : WorldSegment
    {

        public override WorldSegmentTransition[] Transitions { get; }

        public override Vector3 DefaultPlayerStart => new Vector3(7.4f, 1.4f, -7.0f);

        public BasementWorldSegment(UpstairsWorldSegment upstairsWorldSegment) : base()
        {
            Depth = Measure.Feet(53);
            Width = Measure.Feet(50);
            Height = Measure.Feet(10);
            SetSide(Side.Bottom, 0f);

            var office = AddChild(new BasementOffice(this));
            var basement = AddChild(new Basement(this, office));

            office.LoadChildren();
            basement.LoadChildren();

            var upstairsHall = upstairsWorldSegment?.FindChild<UpstairsHall>() ?? new UpstairsHall(this);
            upstairsHall.SetSide(Side.Bottom, UpstairsWorldSegment.FloorY);
            basement.BasementStairsDoor.AddConnectingRoom(upstairsHall, Side.South, 0.5f);
         
            Transitions = new[] { new WorldSegmentTransition<UpstairsWorldSegment>(basement.Stairs, Side.South) };
        }
    }
}
