using ExploringGame.Config;
using ExploringGame.GeometryBuilder.Shapes.Rooms;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.WorldSegments
{
    public class BasementWorldSegment : WorldSegment
    {

        public override WorldSegmentTransition[] Transitions { get; }

        public BasementWorldSegment(TransitionShapesRegistrar transitionShapesRegistrar) : base()
        {
            var upstairsHall = AddChild(new UpstairsHall(this));
            var office = AddChild(new BasementOffice(this));
            var basement = AddChild(new Basement(this, office, upstairsHall));

            upstairsHall.Position = basement.Position;
            upstairsHall.Height = Measure.Feet(8);
            upstairsHall.Width = 10f;
            upstairsHall.Depth = 10f;
            upstairsHall.SetSide(Side.Bottom, basement.GetSide(Side.Top) + Measure.Inches(5));
            upstairsHall.SetSide(Side.North, basement.GetSide(Side.South));

            upstairsHall.LoadChildren();
            office.LoadChildren();
            basement.LoadChildren();

            upstairsHall.X = basement.X;
            upstairsHall.SetSide(Side.North, basement.GetSide(Side.South));
            upstairsHall.SetSideUnanchored(Side.South, basement.GetSide(Side.South) + Measure.Feet(3));
            upstairsHall.SetSideUnanchored(Side.West, basement.GetSide(Side.West) + Measure.Feet(5));
            upstairsHall.SetSideUnanchored(Side.East, basement.GetSide(Side.East) - Measure.Feet(3));


            Transitions = new[] { new WorldSegmentTransition<UpstairsWorldSegment>(basement.Stairs, Side.South) };

            transitionShapesRegistrar.Set(basement.Stairs);
            transitionShapesRegistrar.Set(basement);
            transitionShapesRegistrar.Set(upstairsHall);
        }
    }
}
