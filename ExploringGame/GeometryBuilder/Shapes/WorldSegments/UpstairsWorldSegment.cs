using ExploringGame.Entities;
using ExploringGame.GeometryBuilder.Shapes.Decals;
using ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;
using ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;
using ExploringGame.GeometryBuilder.Shapes.Structures;
using ExploringGame.LevelControl;
using ExploringGame.Logics.Pathfinding;
using ExploringGame.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.GeometryBuilder.Shapes.WorldSegments;

public class UpstairsWorldSegment : WorldSegment
{

    public override Vector3 DefaultPlayerStart => TraverseAllChildren().OfType<LivingRoom>().Single().Position;
    public static readonly float FloorY = Measure.Feet(10);
    public override IReadOnlyList<WorldSegmentTransition> Transitions { get; } = new[]
    {
        new WorldSegmentTransition(typeof(BasementWorldSegment)),
        new WorldSegmentTransition(typeof(OutsideWorldSegment)),
        new WorldSegmentTransition(typeof(BackyardWorldSegment)),
        new WorldSegmentTransition(typeof(NeighborhoodWorldSegment)),
    };

    private Kitchen _kitchen;
    private Bedroom _bedroom;
    private KidsBedroom _kidsBedroom;
    private Den _den;
    private LivingRoom _livingRoom;
    private UpstairsHall _upstairsHall;
    private Bathroom _bathroom;
    private SpareRoom _spareRoom;
    private HalfBathroom _halfBath;
    
    public UpstairsWorldSegment()
    {
        Depth = Measure.Feet(53);
        Width = Measure.Feet(50);
        Height = Measure.Feet(10);
        SetSide(Side.Bottom, FloorY);

        _upstairsHall = new UpstairsHall(this);

        // Create rooms without cross-segment dependencies
        _kitchen = new Kitchen(this, _upstairsHall);
        _livingRoom = new LivingRoom(this, _upstairsHall, _kitchen);
        _bedroom = new Bedroom(this, _upstairsHall);
        _bathroom = new Bathroom(this, _upstairsHall);
        _kidsBedroom = new KidsBedroom(this, _upstairsHall, _bedroom);
        _spareRoom = new SpareRoom(this, _upstairsHall, _bedroom);
        _den = new Den(this, _livingRoom);
        _halfBath = new HalfBathroom(this, _den);
    }

    public override void PositionChildren(IEnumerable<WorldSegment> loadedSegments)
    {        
        _livingRoom.SetSideUnanchored(Side.East, _den.GetSide(Side.West) - 1.0f);

        _spareRoom.SetSide(Side.North, _livingRoom.GetSide(Side.South) + 0.5f);

        _upstairsHall.SetSideUnanchored(Side.West, _spareRoom.GetSide(Side.East) + 0.5f);
        _upstairsHall.LoadChildren(FindShapeByTag<DoorJunction>(loadedSegments, "BasementStairsDoor"));

        _upstairsHall.NorthHall.SetSideUnanchored(Side.North, _livingRoom.GetSide(Side.South));
        _upstairsHall.SouthHall.SetSideUnanchored(Side.South, _bedroom.GetSide(Side.North) - 0.5f);

        _livingRoom.LoadChildren(FindShape<FrontDeck>(loadedSegments));
        _bedroom.LoadChildren();
        _bathroom.LoadChildren();
        _kidsBedroom.LoadChildren();
        _spareRoom.LoadChildren();
        _kitchen.LoadChildren();
        _den.LoadChildren();
        _halfBath.LoadChildren();

        AddChild(new WallDecalStamp());

        // Add the Light Spirit
        var lightSpirit = new LightSpirit();
        lightSpirit.Position = new Vector3(0, -100, 0); // Start underground
        AddChild(lightSpirit);

        var deck = FindShape<FrontDeck>(loadedSegments);
        _livingRoom.AddConnectingRoomWithJunction(
            new DoorJunction(_livingRoom, Side.West, HAlign.Left, DoorDirection.Pull, StateKey.FrontDoorOpen),
            other: deck,
            side: Side.West,
            align: HAlign.Left,
            offset: 0.2f,
            adjustPlacement: false);
    }
}
