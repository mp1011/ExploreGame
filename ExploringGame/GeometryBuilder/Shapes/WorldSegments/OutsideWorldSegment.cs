using ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;
using ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;
using ExploringGame.GeometryBuilder.Shapes.Skyboxes;
using ExploringGame.GeometryBuilder.Shapes.Structures;
using ExploringGame.Logics;
using ExploringGame.Services;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ExploringGame.GeometryBuilder.Shapes.WorldSegments;

public class OutsideWorldSegment : WorldSegment
{
    public override SkyboxShape Skybox => SkyDome.Instance;

    public override IReadOnlyList<WorldSegmentTransition> Transitions { get; } = new[]
    {
        new WorldSegmentTransition(typeof(UpstairsWorldSegment)),
        new WorldSegmentTransition(typeof(NeighborhoodWorldSegment)),
        new WorldSegmentTransition(typeof(BackyardWorldSegment)),
    };

    private FrontDeck _deck;
    private FrontYard _frontYard;
    private WestRoof _roof;
    private Road _road;

    public OutsideWorldSegment() : base()
    {
        Depth = Measure.Feet(100);
        Width = Measure.Feet(100);
        Height = Measure.Feet(20);
        SetSide(Side.Bottom, UpstairsWorldSegment.FloorY - Measure.Feet(4));

        _deck = new FrontDeck(this);
        _frontYard = new FrontYard(this, _deck);
        _roof = new WestRoof(this, _frontYard);

        _road = new Road(this);
        _road.Tag = "HomeRoad";
        _road.AdjustShape().From(_frontYard);
        _road.Depth = Measure.Feet(32);

        _road.Place().OnFloor(_frontYard);
        _road.Place().OnSideOuter(Side.West, _frontYard);
        _road.AdjustShape().AxisStretch(Axis.Z, 50.0f);
    }

    public override void PositionChildren(IEnumerable<WorldSegment> loadedSegments)
    {
        // Find real shapes from UpstairsWorldSegment
        var livingRoom = FindShape<LivingRoom>(loadedSegments);
        var frontDoor = FindShapeByTag<DoorJunction>(loadedSegments, "FrontDoor");
        var livingRoomWindow = FindShapeByTag<Window>(loadedSegments, "LivingRoomWindow");

        // Position and connect deck based on living room
        _deck.Depth = livingRoom.Depth;
        _deck.Width = Measure.Feet(6);
        _deck.Height = livingRoom.Height + Measure.Feet(5);
        _deck.Place().OnSideInner(Side.Bottom, livingRoom)
                    .OnSideOuter(Side.West, livingRoom, -0.5f)
                    .OnSideInner(Side.South, livingRoom);
        _deck.FixedAmbientLight = LightIntensity.Bright;

        _deck.AddConnectingRoom(frontDoor, Side.East);
        _deck.AddConnectingRoom(livingRoomWindow, Side.East);

        // Load children after positioning is complete
        _deck.LoadChildren();
        _frontYard.LoadChildren();
    }
}
