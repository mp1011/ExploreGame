using ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;
using ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;
using ExploringGame.GeometryBuilder.Shapes.SimpleShapes;
using ExploringGame.GeometryBuilder.Shapes.Skyboxes;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ExploringGame.GeometryBuilder.Shapes.WorldSegments;

public class BackyardWorldSegment : WorldSegment
{
    public override SkyboxShape Skybox => SkyDome.Instance;

    public override IReadOnlyList<WorldSegmentTransition> Transitions { get; } = new[]
    {
        new WorldSegmentTransition(typeof(UpstairsWorldSegment)),
        new WorldSegmentTransition(typeof(OutsideWorldSegment)),
    };

    public BackyardWorldSegment() : base()
    {
        var frontSidewalk = new PlaceholderShape<Box>(this, "Sidewalk",
            position: new Vector3(-11.540001f, 2.9200003f, -16.079998f),
            size: new Vector3(7.6799994f, 0.08f, 2.8799999f));

        var northYard = new PlaceholderShape<Room>(this, "FrontYardNorth",
            position: new Vector3(-16.34f, 6.7200003f, -18.96f),
            size: new Vector3(17.279999f, 7.68f, 2.8799999f));

        var den = new PlaceholderShape<Room>(this, "DenEast",
            position: new Vector3(13.2f, 6.48f, -2.8799992f),
            size: new Vector3(2.3999999f, 3.36f, 2.3999999f));

        var kidsBedroom = new PlaceholderShape<KidsBedroom>(this,
            position: new Vector3(-0.7100003f, 6.48f, 9.839999f),
            size: new Vector3(5.7599998f, 3.36f, 5.7599998f));

        var bedroom = new PlaceholderShape<Bedroom>(this,
            position: new Vector3(-7.92f, 6.48f, 9.839999f),
            size: new Vector3(8.16f, 3.36f, 5.7599998f));

        var kitchen = new PlaceholderShape<Kitchen>(this,
            position: new Vector3(0.89000034f, 6.4799995f, -1.8299997f),
            size: new Vector3(4f, 3.36f, 5.4599996f));


        var backyard = new BackYard(this, frontSidewalk, northYard, den, bedroom, kitchen);


        backyard.LoadChildren();
    }
}
