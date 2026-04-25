using ExploringGame.Entities;
using ExploringGame.GameDebug;
using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.Rooms.BasementRooms;
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

    public static Vector3 DefaultPlayerStart => new Vector3(-21, 5, -9);

    private FrontDeck _deck;
    private FrontYard _frontYard;
    private Roof _westRoof, _eastRoof, _denRoof1, _denRoof2;
    private Road _road, _sideRoad;

    public OutsideWorldSegment() : base()
    {
        Depth = Measure.Feet(100);
        Width = Measure.Feet(100);
        Height = Measure.Feet(20);
        SetSide(Side.Bottom, UpstairsWorldSegment.FloorY - Measure.Feet(4));

        _deck = new FrontDeck(this);
        _frontYard = new FrontYard(this, _deck);
        _westRoof = new Roof(this, Side.East) { Tag = "WestRoof" };
        _eastRoof = new Roof(this, Side.West) { Tag = "EastRoof" };
        _denRoof1 = new Roof(this, Side.South) { Tag = "DenRoofNorth" };
        _denRoof2 = new Roof(this, Side.North) { Tag = "DenRoofSouth" };

        _road = new Road(this) { Tag = "HomeRoad" };
        _sideRoad = new Road(this) { Tag = "SideRoad" };

    }

    public override void PositionChildren(IEnumerable<WorldSegment> loadedSegments)
    {
        // Find real shapes from UpstairsWorldSegment
        var livingRoom = FindShape<LivingRoom>(loadedSegments);
        var garage = FindShape<Garage>(loadedSegments);
        var bedroom = FindShape<Bedroom>(loadedSegments);
        var spareRoom = FindShape<SpareRoom>(loadedSegments);
        var den = FindShape<Den>(loadedSegments);

        // Position and connect deck based on living room
        _deck.Depth = livingRoom.Depth;
        _deck.Width = Measure.Feet(6);
        _deck.Height = livingRoom.Height + Measure.Inches(6);
        _deck.Place().OnSideInner(Side.Bottom, livingRoom)
                    .OnSideOuter(Side.West, livingRoom, -0.5f)
                    .OnSideInner(Side.South, livingRoom);
        _deck.FixedAmbientLight = LightIntensity.Bright;

        // Load children after positioning is complete
        _deck.LoadChildren(livingRoom);
        _frontYard.LoadChildren(garage, bedroom, spareRoom);

        _road.AdjustShape().From(_frontYard);
        _road.Width = Measure.Feet(32);
        _road.Place().OnFloor(_frontYard);
        _road.Place().OnSideOuter(Side.West, _frontYard);
        _road.AdjustShape().AxisStretch(Axis.Z, 200.0f);

        _sideRoad.Depth = Measure.Feet(32);
        _sideRoad.Height = _road.Height;
        _sideRoad.AdjustShape().AxisStretch(Axis.X, 200.0f);

        _sideRoad.Place().OnSideOuter(Side.East, _road).OnFloor(_frontYard);
        _sideRoad.Z = _frontYard.GetSide(Side.South) + Measure.Feet(110);

        _westRoof.Depth = _frontYard.Depth;
        _eastRoof.Depth = _frontYard.Depth;

        _westRoof.Width = Measure.Feet(17.5f);
        _eastRoof.Width = _westRoof.Width;

        _westRoof.Place().OnSideOuter(Side.East, _frontYard.Deck, -Roof.RoofOverhang)
                    .OnSideOuter(Side.Top, _frontYard, -Measure.Inches(6))
                    .OnSideInner(Side.North, _frontYard.Deck, -Roof.RoofOverhang);

        _westRoof.SetSideUnanchored(Side.South, garage.GetSide(Side.South) + Roof.RoofOverhang);

        _eastRoof.Depth = _westRoof.Depth;
        _eastRoof.Place().At(_westRoof).OnSideOuter(Side.East, _westRoof);
       

        _denRoof1.AdjustShape().From(den);
        _denRoof1.Height = Measure.Feet(1);
        _denRoof1.Depth = den.Depth / 2f;
        _denRoof1.Place().OnSideOuter(Side.Top, den)
            .OnSideInner(Side.North, den, -Measure.Feet(1.2f));

        _denRoof1.Place().AlignSideWith(Side.East, den.EastPart, Measure.Feet(3));

        _denRoof2.AdjustShape().From(_denRoof1);
        _denRoof2.Place().OnSideInner(Side.South, den, Measure.Feet(2));
        _denRoof2.SetSideUnanchored(Side.North, _denRoof1.GetSide(Side.South));

        _denRoof1.VertexOffsets.Add(new VertexOffset(Side.South | Side.West, new Vector3(-8.5f, 0f, 0f)));
        _denRoof2.VertexOffsets.Add(new VertexOffset(Side.North | Side.West, new Vector3(-8.5f, 0f, 0f)));


        var sideSection = _frontYard.Copy();
        sideSection.Place().OnSideOuter(Side.South, _frontYard.SouthSection)
            .OnSideInner(Side.West, _frontYard.SouthSection);

        sideSection.SetSideUnanchored(Side.South, _sideRoad.GetSide(Side.North));
        sideSection.SetSideUnanchored(Side.East, _sideRoad.GetSide(Side.East));


        var sideSection2 = sideSection.Copy();
        sideSection2.Place().OnSideOuter(Side.South, _sideRoad);

        new Fence(sideSection2, Side.South);

        new GrassSurface(sideSection, TerrainSurface.DefaultLawn);
        new GrassSurface(sideSection2, TerrainSurface.DefaultLawn);


        DebugShapeLogger.LogShape("OutsideWorldSegment PositionChildren end", _frontYard);
    }
}
