using ExploringGame.GeometryBuilder.Shapes.Rooms;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.WorldSegments
{
    public class BasementWorldSegment : WorldSegment
    {
        public BasementWorldSegment() : base()
        {
            var office = AddChild(new BasementOffice(this));
            var basement = AddChild(new Basement(this, office));
             basement.AddConnectingRoom(new RoomConnection(basement, office.Exit, Side.East, 0.5f), adjustPlacement: false);

            var dummyUpstairs = AddChild(new Room(this, new BasementRoomTheme()));
            dummyUpstairs.Position = basement.Position;
            dummyUpstairs.Height = office.Height;
            dummyUpstairs.Width = 10f;
            dummyUpstairs.Depth = 10f;

            dummyUpstairs.SetSide(Side.Bottom, basement.GetSide(Side.Top));
            dummyUpstairs.SetSide(Side.North, basement.GetSide(Side.South));

        }
    }
}
