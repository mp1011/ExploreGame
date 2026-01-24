using ExploringGame.GeometryBuilder.Shapes.Rooms;
using System;
using System.Linq;

namespace ExploringGame.GeometryBuilder.Shapes.WorldSegments;

public class UpstairsWorldSegment : WorldSegment
{
    public override WorldSegmentTransition[] Transitions { get; }

    public UpstairsWorldSegment()
    {
        Depth = Measure.Feet(50);
        Width = Measure.Feet(50);
        Height = Measure.Feet(10);
        SetSide(Side.Bottom, Measure.Feet(8));

        var upstairsHall = new UpstairsHall(this);
        //var kitchen = new Kitchen(this, upstairsHall);
        //var livingRoom = new LivingRoom(this, upstairsHall, kitchen);
        //var bedroom = new Bedroom(this, upstairsHall);
        //var bathroom = new Bathroom(this, upstairsHall);
        //var kidsBedroom = new KidsBedroom(this, upstairsHall);
        //var spareRoom = new SpareRoom(this, upstairsHall);
        //var den = new Den(this, livingRoom);
        //var halfBath = new HalfBathroom(this, den);


        //upstairsHall.Position = basement.Position;
        //upstairsHall.Height = Measure.Feet(8);
        //upstairsHall.Width = 10f;
        //upstairsHall.Depth = 10f;
        //upstairsHall.SetSide(Side.Bottom, basement.GetSide(Side.Top) + Measure.Inches(5));
        //upstairsHall.SetSide(Side.North, basement.GetSide(Side.South));

        //upstairsHall.X = basement.X;
        //upstairsHall.SetSide(Side.North, basement.GetSide(Side.South));
        //upstairsHall.SetSideUnanchored(Side.South, basement.GetSide(Side.South) + Measure.Feet(3));
        //upstairsHall.SetSideUnanchored(Side.West, basement.GetSide(Side.West) + Measure.Feet(5));
        //upstairsHall.SetSideUnanchored(Side.East, basement.GetSide(Side.East) - Measure.Feet(3));

        //upstairsHall.LoadChildren();
        //livingRoom.LoadChildren();
        //bedroom.LoadChildren();
        //bathroom.LoadChildren();
        //kidsBedroom.LoadChildren();
        //spareRoom.LoadChildren();
        //kitchen.LoadChildren();
        //livingRoom.LoadChildren();
        //den.LoadChildren();
        //halfBath.LoadChildren();

        Transitions = Array.Empty<WorldSegmentTransition>();

       // Transitions = new[] { new WorldSegmentTransition<BasementWorldSegment>(basement.Stairs, Side.North) };
    }

}
