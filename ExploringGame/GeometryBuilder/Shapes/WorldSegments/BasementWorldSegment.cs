using ExploringGame.GeometryBuilder.Shapes.Rooms.BasementRooms;
using ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;
using ExploringGame.LevelControl;
using ExploringGame.Services;
using Jitter2.Dynamics.Constraints;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace ExploringGame.GeometryBuilder.Shapes.WorldSegments
{
    public class BasementWorldSegment : WorldSegment
    {
        public static Vector3 DefaultPlayerStart => new Vector3(7.4f, 1.4f, -7.0f);

        private Basement _basement;
        private BasementOffice _office;
        private Garage _garage;

        public BasementWorldSegment() : base()
        {
            Depth = Measure.Feet(53);
            Width = Measure.Feet(50);
            Height = Measure.Feet(10);
            SetLocalSide(Side.Bottom, 0f);

            _office = AddChild(new BasementOffice(this));
            _basement = AddChild(new Basement(this, _office));
            _garage = AddChild(new Garage(this, _basement));
        }

        public override void PositionChildren(IEnumerable<WorldSegment> loadedSegments)
        {
            // Find the real UpstairsHall from UpstairsWorldSegment
            var upstairsHall = FindShape<UpstairsHall>(loadedSegments);
            _basement.SetDependencies(upstairsHall);

            // Load children after all positioning is complete
            _office.LoadChildren();
            _basement.LoadChildren();
            _garage.LoadChildren();

            new BlockerCreator().ExecuteForDoors(this, StateKey.BasementStairsDoorOpen);
        }
    }
}
