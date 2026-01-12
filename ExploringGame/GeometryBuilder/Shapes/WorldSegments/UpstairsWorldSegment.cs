using ExploringGame.GeometryBuilder.Shapes.Rooms;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.WorldSegments;

class UpstairsWorldSegment : WorldSegment
{
    public override WorldSegmentTransition[] Transitions => new[] { new WorldSegmentTransition<BasementWorldSegment, BasementStairs>(Side.North) };

    public UpstairsWorldSegment()
    {
        var dummyUpstairs = AddChild(new Room(this, new BasementRoomTheme()));        
      //  dummyUpstairs.Position = basement.Position;
        dummyUpstairs.Height = Measure.Feet(8);
        dummyUpstairs.Width = 10f;
        dummyUpstairs.Depth = 10f;
      //  dummyUpstairs.SetSide(Side.Bottom, basement.GetSide(Side.Top));
     //   dummyUpstairs.SetSide(Side.North, basement.GetSide(Side.South));

        dummyUpstairs.LoadChildren();
      
     //   dummyUpstairs.X = basement.X;
     //   dummyUpstairs.SetSide(Side.North, basement.GetSide(Side.South));
    }
}
