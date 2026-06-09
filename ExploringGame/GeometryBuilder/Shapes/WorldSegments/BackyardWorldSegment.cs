using ExploringGame.GeometryBuilder.Shapes.Rooms.BasementRooms;
using ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;
using ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;
using ExploringGame.GeometryBuilder.Shapes.SimpleShapes;
using ExploringGame.GeometryBuilder.Shapes.Skyboxes;
using ExploringGame.GeometryBuilder.Shapes.Structures;
using ExploringGame.LevelControl;
using ExploringGame.Services;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.GeometryBuilder.Shapes.WorldSegments;

public class BackyardWorldSegment : WorldSegment
{
    public override SkyboxShape Skybox => SkyDome.Instance;

    private BackYard _backyard;

    public BackyardWorldSegment() : base()
    {
        // Create BackYard without any cross-segment dependencies
        _backyard = new BackYard(this);
    }

    public override void PositionChildren(IEnumerable<WorldSegment> loadedSegments)
    {
        // Find real shapes from OutsideWorldSegment
        var frontSidewalk = FindShapeByTag<Box>(loadedSegments, "Sidewalk");
        var northYard = FindShapeByTag<Room>(loadedSegments, "FrontYardNorth");
        var eastRoof = FindShapeByTag<Roof>(loadedSegments, "EastRoof");
        var denRoof = FindShapeByTag<Roof>(loadedSegments, "DenRoofNorth");

        // Find real shapes from UpstairsWorldSegment
        var denEast = FindShapeByTag<Room>(loadedSegments, "DenEast");
        var kitchen = FindShape<Kitchen>(loadedSegments);
        var den = FindShape<Den>(loadedSegments);
        var kidsBedroom = FindShape<KidsBedroom>(loadedSegments);
        var bedroom = FindShape<Bedroom>(loadedSegments);
        var basement = FindShape<Basement>(loadedSegments);
        var basementOffice = FindShape<BasementOffice>(loadedSegments);
        var frontDeck = FindShape<FrontDeck>(loadedSegments);
        var halfBath = FindShape<HalfBathroom>(loadedSegments);

        var southFrontYard = FindShapeByTag<Room>(loadedSegments, "SouthFrontYard");

        _backyard.LoadChildren(frontSidewalk, northYard, frontDeck, den, kitchen, kidsBedroom, bedroom, basement, basementOffice, eastRoof, denRoof, southFrontYard,
            halfBath);

        var slidingDoorJunction = _backyard.DeckArea.AddConnectingRoomWithJunction(new SlidingDoorJunction(_backyard.DeckArea, Side.North, HAlign.Right, StateKey.DeckSlidingDoorOpen),
            den, Side.North, HAlign.Left, 2.0f, adjustPlacement: false);

        slidingDoorJunction.SetLocalSide(Side.Top, den.GetLocalSide(Side.Top) - Measure.Feet(1));
        slidingDoorJunction.SetLocalSideUnanchored(Side.Bottom, _backyard.BackDeck.GetLocalSide(Side.Bottom));

        _backyard.AddConnectingRoom(FindShape<FrontYard>(loadedSegments), Side.West);

        var neighborHouse1 = AddChild(new NeighborHouse());
        neighborHouse1.Width = Measure.Feet(50);
        neighborHouse1.Depth = Measure.Feet(50);
        neighborHouse1.Height = Measure.Feet(20);
        neighborHouse1.Place().OnFloor(_backyard)
            .OnSideOuter(Side.North, _backyard, -Measure.Feet(10));

        var neighborHouse2 = AddChild(new NeighborHouse());
        neighborHouse2.Width = Measure.Feet(50);
        neighborHouse2.Depth = Measure.Feet(50);
        neighborHouse2.Height = Measure.Feet(20);
        neighborHouse2.Place().OnFloor(_backyard)
            .OnSideOuter(Side.East, _backyard, Measure.Feet(20));


    }
}
