using ExploringGame.GeometryBuilder.Shapes.Rooms.BasementRooms;
using ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;
using Jitter2.Dynamics.Constraints;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace ExploringGame.GeometryBuilder.Shapes.WorldSegments
{
    public class BasementWorldSegment : WorldSegment
    {

        public override Vector3 DefaultPlayerStart => new Vector3(7.4f, 1.4f, -7.0f);

        public override IReadOnlyList<Type> AnchorShapeTypes => new[] { typeof(UpstairsHall) };
        public override IReadOnlyList<WorldSegmentTransition> Transitions { get; } = new[]
        {
            new WorldSegmentTransition(typeof(UpstairsWorldSegment))
        };

        public BasementWorldSegment() : base()
        {
            Depth = Measure.Feet(53);
            Width = Measure.Feet(50);
            Height = Measure.Feet(10);
            SetSide(Side.Bottom, 0f);

            var office = AddChild(new BasementOffice(this));
            var basement = AddChild(new Basement(this, office));

            office.LoadChildren();
            basement.LoadChildren();

            var dummyUpstairsHall = new PlaceholderShape<UpstairsHall>(this,
                position: new Vector3(-2.3899999f, 6.48f, 0f),
                size: new Vector3(6.7f, 3.36f, 1.92f));

            basement.BasementStairsDoor.AddConnectingRoom(dummyUpstairsHall, Side.South, 0.5f);

            var garage = AddChild(new Garage(this, basement));
            garage.LoadChildren();
        }
    }
}
