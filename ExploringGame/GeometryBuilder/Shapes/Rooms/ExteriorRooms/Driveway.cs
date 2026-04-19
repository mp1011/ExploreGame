using ExploringGame.GeometryBuilder.Shapes.Rooms.BasementRooms;
using ExploringGame.GeometryBuilder.Shapes.SimpleShapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;

public class Driveway : Room
{
    public override Theme Theme => new RoadTheme();

    public override Side OmitSides => Side.Top | Side.South | Side.North | Side.East | Side.West;

    public Driveway(WorldSegment worldSegment, Garage garage, FrontYard yard) : base(worldSegment)
    {
        FixedAmbientLight = LightIntensity.Bright;

        Depth = garage.Depth;
        Height = 0.5f;
        Width = 1.0f;

        SetSideUnanchored(Side.Top, yard.GetSide(Side.Top));

        this.Place().OnSideInner(Side.East, yard.Deck)
                    .OnSideInner(Side.North, garage)
                    .OnFloor(garage);

        SetSideUnanchored(Side.West, yard.GetSide(Side.West));

        var yOffset = yard.GetSide(Side.Bottom) - GetSide(Side.Bottom);
        VertexOffsets.Add(new VertexOffset(Side.West, new Vector3(0f, yOffset, 0f)));
    }

    public void LoadChildren(FrontYard yard, Garage garage)
    {
        var retainingWallNorth = AddChild(new Box(yard.Theme, TextureKey.Brick));
        retainingWallNorth.Width = Width;
        retainingWallNorth.Depth = Measure.Feet(1);
        retainingWallNorth.Height = Measure.Feet(6);
        retainingWallNorth.Place().At(this)
            .OnSideInner(Side.North, this)
            .OnSideInner(Side.West, this);
        retainingWallNorth.SetSide(Side.Top, yard.GetSide(Side.Bottom) + Measure.Inches(4));
        retainingWallNorth.SetSideUnanchored(Side.East, yard.FrontWalkway.WestPart.GetSide(Side.West) + Measure.Inches(4));

        var retainingWallNorth2 = AddChild(new Box(yard.Theme, TextureKey.Brick));
        retainingWallNorth2.Width = Width;
        retainingWallNorth2.Depth = Measure.Feet(1);
        retainingWallNorth2.Height = Measure.Feet(6);
        retainingWallNorth2.Place().At(this)
            .OnSideInner(Side.North, this)
            .OnSideInner(Side.East, this);
        retainingWallNorth2.SetSide(Side.Top, yard.GetSide(Side.Bottom) + Measure.Inches(4));
        retainingWallNorth2.SetSideUnanchored(Side.West, yard.FrontWalkway.WestPart.GetSide(Side.East) - Measure.Feet(1));

        var retainingWallSouth = AddChild(new Box(yard.Theme, TextureKey.Brick));
        retainingWallSouth.Width = Width;
        retainingWallSouth.Depth = Measure.Feet(1);
        retainingWallSouth.Height = Measure.Feet(6);
        retainingWallSouth.Place().At(this)
            .OnSideInner(Side.South, this)
            .OnSideInner(Side.West, this);
        retainingWallSouth.SetSide(Side.Top, yard.GetSide(Side.Bottom) + Measure.Inches(4));
        retainingWallSouth.SetSideUnanchored(Side.East, GetSide(Side.East));
    }
}
