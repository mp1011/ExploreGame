using ExploringGame.GeometryBuilder.Shapes.Rooms.BasementRooms;
using ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace ExploringGame.GeometryBuilder.Shapes.WorldSegments
{
    public class BasementWorldSegment : WorldSegment
    {

        public override Vector3 DefaultPlayerStart => new Vector3(7.4f, 1.4f, -7.0f);

     //   public override IReadOnlyList<Type> AnchorShapeTypes => new[] { typeof(UpstairsHall) };
        //public override IReadOnlyList<WorldSegmentTransition> Transitions { get; } = new[]
        //{
        //    new WorldSegmentTransition(typeof(UpstairsWorldSegment))
        //};

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

            var upstairsHall = new UpstairsHall(this);

            // hard-code upstairs hall position to match upstairs world segment
            upstairsHall.Size = new Vector3(6.7f,  3.36f,  1.92f);
            upstairsHall.Position = new Vector3(-2.3899999f, 6.48f, 0f);

            basement.BasementStairsDoor.AddConnectingRoom(upstairsHall, Side.South, 0.5f);

            var garage = AddChild(new Garage(this, basement));
            garage.LoadChildren();
        }
    }
}
