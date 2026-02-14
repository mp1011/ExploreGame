using ExploringGame.Entities;
using ExploringGame.GeometryBuilder.Shapes.Decals;
using ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;
using ExploringGame.Services;
using Microsoft.Xna.Framework;
using System;

namespace ExploringGame.GeometryBuilder.Shapes.WorldSegments;

public class UpstairsWorldSegment : WorldSegment
{
    public static readonly float FloorY = Measure.Feet(10);

    public override WorldSegmentTransition[] Transitions { get; }

    public UpstairsWorldSegment(BasementWorldSegment basement)
    {
        Depth = Measure.Feet(53);
        Width = Measure.Feet(50);
        Height = Measure.Feet(10);
        SetSide(Side.Bottom, FloorY);

        var upstairsHall = basement?.FindChild<UpstairsHall>() ?? new UpstairsHall(this);
        var kitchen = new Kitchen(this, upstairsHall);
        var livingRoom = new LivingRoom(this, upstairsHall, kitchen);
        var bedroom = new Bedroom(this, upstairsHall);
        var bathroom = new Bathroom(this, upstairsHall);
        var kidsBedroom = new KidsBedroom(this, upstairsHall, bedroom);
        var spareRoom = new SpareRoom(this, upstairsHall, bedroom);
        var den = new Den(this, livingRoom);
        var halfBath = new HalfBathroom(this, den);
      
        livingRoom.SetSideUnanchored(Side.East, den.GetSide(Side.West) - 1.0f);

        spareRoom.SetSide(Side.North, livingRoom.GetSide(Side.South) + 0.5f);


        upstairsHall.SetSideUnanchored(Side.West, spareRoom.GetSide(Side.East) + 0.5f);
        upstairsHall.LoadChildren();

        upstairsHall.NorthHall.SetSideUnanchored(Side.North, livingRoom.GetSide(Side.South));
        upstairsHall.SouthHall.SetSideUnanchored(Side.South, bedroom.GetSide(Side.North) - 0.5f);
        
        livingRoom.LoadChildren();
        bedroom.LoadChildren();
        bathroom.LoadChildren();
        kidsBedroom.LoadChildren();
        spareRoom.LoadChildren();
        kitchen.LoadChildren();
        den.LoadChildren();
        halfBath.LoadChildren();

        Transitions = Array.Empty<WorldSegmentTransition>();

        AddChild(new WallDecalStamp());

        // Add the Light Spirit
        var lightSpirit = new LightSpirit();
        lightSpirit.Position = new Vector3(0, -100, 0); // Start underground
        AddChild(lightSpirit);

        // Transitions = new[] { new WorldSegmentTransition<BasementWorldSegment>(basement.Stairs, Side.North) };
    }

}
