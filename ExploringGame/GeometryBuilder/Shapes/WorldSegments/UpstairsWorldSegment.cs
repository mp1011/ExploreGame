using ExploringGame.Entities;
using ExploringGame.GeometryBuilder.Shapes.Decals;
using ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;
using ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;
using ExploringGame.GeometryBuilder.Shapes.SimpleShapes;
using ExploringGame.GeometryBuilder.Shapes.Structures;
using ExploringGame.LevelControl;
using ExploringGame.Logics.Pathfinding;
using ExploringGame.Services;
using ExploringGame.Story.PlotPoints;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.GeometryBuilder.Shapes.WorldSegments;

public class UpstairsWorldSegment : WorldSegment
{
    public static readonly float FloorY = Measure.Feet(10);

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
        SetLocalSide(Side.Bottom, FloorY);

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
        _livingRoom.SetLocalSideUnanchored(Side.East, _den.GetLocalSide(Side.West) - 1.0f);

        _spareRoom.SetLocalSide(Side.North, _livingRoom.GetLocalSide(Side.South) + 0.5f);

        _upstairsHall.SetLocalSideUnanchored(Side.West, _spareRoom.GetLocalSide(Side.East) + 0.5f);
        _upstairsHall.LoadChildren(FindShapeByTag<DoorJunction>(loadedSegments, "BasementStairsDoor"));

        _upstairsHall.NorthHall.SetLocalSideUnanchored(Side.North, _livingRoom.GetLocalSide(Side.South));
        _upstairsHall.SouthHall.SetLocalSideUnanchored(Side.South, _bedroom.GetLocalSide(Side.North) - 0.5f);

        _livingRoom.LoadChildren(FindShape<FrontDeck>(loadedSegments));
        _bedroom.LoadChildren();
        _bathroom.LoadChildren();
        _kidsBedroom.LoadChildren();
        _spareRoom.LoadChildren();
        _kitchen.LoadChildren(_bathroom);
        _den.LoadChildren();
        _halfBath.LoadChildren();

        var bedroomDoor = TraverseAllChildren().OfType<Door>().Where(p => p.StateKey == StateKey.BedroomDoorOpen).Single().Parent;

        var bedroomDoorBlocker = _upstairsHall.AddChild(new Blocker(bedroomDoor) { Tag = "BedroomDoorBlocker" });
        bedroomDoorBlocker.AdjustShape().From(bedroomDoor)
            .AxisStretch(Axis.X, 1.0f)
            .AxisStretch(Axis.Z, 1.0f);

        new BlockerCreator().ExecuteForDoors(this, StateKey.KidsBedroomDoorOpen, StateKey.BathroomDoorOpen, StateKey.DenDoorsOpen, StateKey.SpareRoomDoorOpen, StateKey.LinenClosetDoorOpen);
        new BlockerCreator().ExecuteForSwitches(this, StateKey.HallLightOn, StateKey.KitchenLightOn, StateKey.LivingRoomLightOn, StateKey.RightBedroomLightOn);


        var childPuppet = AddChild(new Puppet(this, 0.5f));
        childPuppet.Tag = "Child";
        childPuppet.WorldY = -10;
        childPuppet.Active = false;
    }
}
