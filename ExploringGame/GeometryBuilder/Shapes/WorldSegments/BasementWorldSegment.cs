using ExploringGame.GeometryBuilder.Shapes.Rooms;
using System;

namespace ExploringGame.GeometryBuilder.Shapes.WorldSegments
{
    public class BasementWorldSegment : WorldSegment
    {

        public override WorldSegmentTransition[] Transitions { get; }

        public BasementWorldSegment() : base()
        {
            Depth = Measure.Feet(50);
            Width = Measure.Feet(50);
            Height = Measure.Feet(10);
            SetSide(Side.Bottom, 0f);

            var office = AddChild(new BasementOffice(this));
            var basement = AddChild(new Basement(this, office));

            office.LoadChildren();
            basement.LoadChildren();
         
          //  Transitions = Array.Empty<WorldSegmentTransition>();
            Transitions = new[] { new WorldSegmentTransition<UpstairsWorldSegment>(basement.Stairs, Side.South) };
        }
    }
}
