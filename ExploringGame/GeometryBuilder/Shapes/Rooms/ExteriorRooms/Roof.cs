using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;

public class Roof : Room
{
    public static readonly float RoofHeight = Measure.Feet(7.0f);
    public static readonly float RoofOverhang = Measure.Feet(1);

    public override Theme Theme { get; } = new RoofTheme();

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public Roof(OutsideWorldSegment worldSegment, Side raiseSide) : base(worldSegment)
    {
        FixedAmbientLight = LightIntensity.Bright;

        Theme.SideTextures[Side.Bottom] = new TextureInfo(TextureKey.Concrete, TextureStyle.Tile, new TilingInfo(TileSize: 2.0f));
        Theme.SideTextures[Side.Top] = new TextureInfo(TextureKey.Concrete, TextureStyle.Tile, new TilingInfo(TileSize: 2.0f));

        Size = Vector3.One;
        Height = Measure.Feet(1);
        VertexOffsets.Add(new VertexOffset(raiseSide, new Vector3(0, RoofHeight, 0)));
    }
}