using ExploringGame.GeometryBuilder.Shapes.Rooms;

namespace ExploringGame.GeometryBuilder.Shapes.WorldSegments
{
    public class BasementWorldSegment : WorldSegment
    {
        public BasementWorldSegment() : base()
        {
            var office = AddChild(new BasementOffice(this));
            var basement = AddChild(new Basement(this, office));
             basement.AddConnectingRoom(new RoomConnection(basement, office.Exit, Side.East, 0.5f), adjustPlacement: false);
        }
    }
}
