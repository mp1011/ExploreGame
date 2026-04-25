using ExploringGame.GeometryBuilder.Shapes.Rooms.BasementRooms;
using ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;
using ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;
using ExploringGame.GeometryBuilder.Shapes.SimpleShapes;
using ExploringGame.GeometryBuilder.Shapes.Skyboxes;
using ExploringGame.GeometryBuilder.Shapes.Structures;
using ExploringGame.LevelControl;
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

        var southFrontYard = FindShapeByTag<Room>(loadedSegments, "SouthFrontYard");

        _backyard.LoadChildren(frontSidewalk, northYard, frontDeck, den, kitchen, kidsBedroom, bedroom, basement, basementOffice, eastRoof, denRoof, southFrontYard);

        var slidingDoorJunction = _backyard.DeckArea.AddConnectingRoomWithJunction(new SlidingDoorJunction(_backyard.DeckArea, Side.North, HAlign.Right, StateKey.DeckSlidingDoorOpen),
            den, Side.North, HAlign.Left, 1.0f, adjustPlacement: false);

        slidingDoorJunction.SetSide(Side.Top, den.GetSide(Side.Top) - Measure.Feet(1));
        slidingDoorJunction.SetSideUnanchored(Side.Bottom, _backyard.BackDeck.GetSide(Side.Bottom));

    }
}
