using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;

public class WestRoof : Room
{
    public static readonly float RoofHeight = Measure.Feet(5.0f);
    public static readonly float RoofOverhang = Measure.Feet(1);

    public override Theme Theme { get; } = new RoofTheme();

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public WestRoof(OutsideWorldSegment worldSegment, FrontYard yard) : base(worldSegment)
    {
        FixedAmbientLight = LightIntensity.Bright;

        Theme.SideTextures[Side.Bottom] = new TextureInfo(TextureKey.Concrete, TextureStyle.Tile, new TilingInfo(TileSize: 2.0f));
        Theme.SideTextures[Side.Top] = new TextureInfo(TextureKey.Concrete, TextureStyle.Tile, new TilingInfo(TileSize: 2.0f));

        Size = Vector3.One;       
        VertexOffsets.Add(new VertexOffset(Side.East, new Vector3(0, RoofHeight, 0)));
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return base.BuildInternal(quality);
    }
}
