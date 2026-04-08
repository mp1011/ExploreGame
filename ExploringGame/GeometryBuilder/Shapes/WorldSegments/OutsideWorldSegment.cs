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
    };

    public OutsideWorldSegment() : base()
    {
        Depth = Measure.Feet(100);
        Width = Measure.Feet(100);
        Height = Measure.Feet(20);
        SetSide(Side.Bottom, UpstairsWorldSegment.FloorY - Measure.Feet(4));

        var deck = new FrontDeck(this);

        var livingRoom = new PlaceholderShape<LivingRoom>(this,
            position: new Vector3(-4.58f, 6.48f, -9.839999f),
            size: new Vector3(14.84f, 3.36f, 10.559999f));

        var frontDoor = new PlaceholderShape<DoorJunction>(this, "FrontDoor",
           position: new Vector3(-12.25f, 6.48f, -5.37f),
           size: new Vector3(0.49999923f, 3.36f, 1.22f));

        var livingRoomWindow = new PlaceholderShape<Window>(this, "LivingRoomWindow",
            position: new Vector3(-12.25f, 6.7200003f, -9.359999f),
            size: new Vector3(0.49999923f, 1.92f, 2.8799999f));

        deck.Depth = livingRoom.Depth;
        deck.Width = Measure.Feet(6);
        deck.Height = livingRoom.Height + Measure.Feet(5);
        deck.Place().OnSideInner(Side.Bottom, livingRoom)
                    .OnSideOuter(Side.West, livingRoom, -0.5f)
                    .OnSideInner(Side.South, livingRoom);
        deck.FixedAmbientLight = LightIntensity.Bright;

        deck.AddConnectingRoom(frontDoor, Side.East);
        deck.AddConnectingRoom(livingRoomWindow, Side.East);

        var frontYard = new FrontYard(this, deck);
        var roof = new WestRoof(this, frontYard);

        deck.LoadChildren();
        frontYard.LoadChildren();
        roof.LoadChildren();

        var road = new Road(this);
        road.Tag = "HomeRoad";
        road.AdjustShape().From(frontYard);
        road.Depth = Measure.Feet(32);

        road.Place().OnFloor(frontYard);
        road.Place().OnSideOuter(Side.West, frontYard);
        road.AdjustShape().AxisStretch(Axis.Z, 50.0f);
    }
}
