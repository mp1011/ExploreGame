using ExploringGame.GeometryBuilder.Shapes.Rooms;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.WorldSegments
{
    public class BasementWorldSegment : WorldSegment
    {
        public BasementWorldSegment() : base()
        {
            var dummyUpstairs = AddChild(new Room(this, new BasementRoomTheme()));
            var office = AddChild(new BasementOffice(this));
            var basement = AddChild(new Basement(this, office, dummyUpstairs));

            dummyUpstairs.Position = basement.Position;
            dummyUpstairs.Height = Measure.Feet(8);
            dummyUpstairs.Width = 10f;
            dummyUpstairs.Depth = 10f;
            dummyUpstairs.SetSide(Side.Bottom, basement.GetSide(Side.Top));
            dummyUpstairs.SetSide(Side.North, basement.GetSide(Side.South));

            dummyUpstairs.LoadChildren();
            office.LoadChildren();
            basement.LoadChildren();

            dummyUpstairs.X = basement.X;
            dummyUpstairs.SetSide(Side.North, basement.GetSide(Side.South));
        }
    }
}
