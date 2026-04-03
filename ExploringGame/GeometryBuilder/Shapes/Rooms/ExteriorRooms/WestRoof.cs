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
    private static readonly float RoofHeight = Measure.Feet(3.0f);
    private static readonly float RoofOverhang = Measure.Feet(1);

    public override Theme Theme { get; } = new RoofTheme();

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public WestRoof(OutsideWorldSegment worldSegment, FrontYard yard) : base(worldSegment)
    {
        FixedAmbientLight = LightIntensity.Bright;

        Theme.SideTextures[Side.Bottom] = new TextureInfo(Color.Red, TextureKey.Concrete, TextureStyle.FillSide, TileSize: 1.0f);
        Theme.SideTextures[Side.Top] = new TextureInfo(Color.Green, TextureKey.Concrete, TextureStyle.FillSide, TileSize: 1.0f);

        //Theme.SideTextures[Side.Bottom] = new TextureInfo(Color.Red, TextureKey.Concrete, TextureStyle.FillSide);

        Height = Measure.Feet(1);
        Depth = yard.Depth;
        Width = Measure.Feet(20);

        this.Place().OnSideOuter(Side.East, yard.Deck, -RoofOverhang)
                    .OnSideOuter(Side.Top, yard)
                    .OnSideInner(Side.North, yard.Deck, -RoofOverhang);

        VertexOffsets.Add(new VertexOffset(Side.East, new Vector3(0, RoofHeight, 0)));
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return base.BuildInternal(quality);
    }
}
