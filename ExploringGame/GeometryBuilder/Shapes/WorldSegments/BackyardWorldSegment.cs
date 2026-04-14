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
    public override Vector3 DefaultPlayerStart => TraverseAllChildren().OfType<BackYard>().Single().Position;
    public override IReadOnlyList<WorldSegmentTransition> Transitions { get; } = new[]
    {
        new WorldSegmentTransition(typeof(UpstairsWorldSegment)),
        new WorldSegmentTransition(typeof(OutsideWorldSegment)),
        new WorldSegmentTransition(typeof(NeighborhoodWorldSegment)),

    };

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
        var bedroomSouthWindow = FindShapeByTag<Window>(loadedSegments, "BedroomSouthWindow");
        var kitchen = FindShape<Kitchen>(loadedSegments);
        var kidsBedroomSouthWindow = FindShapeByTag<Window>(loadedSegments, "KidBedroomSouthWindow");
        var kidBedroomEastWindow = FindShapeByTag<Window>(loadedSegments, "KidBedroomEastWindow");

        // Set all cross-segment dependencies
        _backyard.SetDependencies(frontSidewalk, northYard, denEast, bedroomSouthWindow, kitchen, kidsBedroomSouthWindow, kidBedroomEastWindow);

        // Load children after all positioning is complete
        //_backyard.LoadChildren();
    }
}
