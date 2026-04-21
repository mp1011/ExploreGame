using ExploringGame.GeometryBuilder.Shapes.Rooms.BasementRooms;
using ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;
using ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;
using ExploringGame.GeometryBuilder.Shapes.SimpleShapes;
using ExploringGame.GeometryBuilder.Shapes.Skyboxes;
using ExploringGame.GeometryBuilder.Shapes.Structures;
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

        // Find real shapes from UpstairsWorldSegment
        var denEast = FindShapeByTag<Room>(loadedSegments, "DenEast");
        var kitchen = FindShape<Kitchen>(loadedSegments);
        var den = FindShape<Den>(loadedSegments);
        var kidsBedroom = FindShape<KidsBedroom>(loadedSegments);
        var bedroom = FindShape<Bedroom>(loadedSegments);
        var basement = FindShape<Basement>(loadedSegments);
        var basementOffice = FindShape<BasementOffice>(loadedSegments);
        var frontDeck = FindShape<FrontDeck>(loadedSegments);

        _backyard.LoadChildren(frontSidewalk, northYard, frontDeck, den, kitchen, kidsBedroom, bedroom, basement, basementOffice);
    }
}
