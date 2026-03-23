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
                position: new Vector3(-3.4249997f, 6.4799995f, -0.060000002f),
                size: new Vector3(4.63f, 3.36f, 1.92f));

            basement.BasementStairsDoor.AddConnectingRoom(dummyUpstairsHall, Side.South);

            var garage = AddChild(new Garage(this, basement));
            garage.LoadChildren();
        }
    }
}
