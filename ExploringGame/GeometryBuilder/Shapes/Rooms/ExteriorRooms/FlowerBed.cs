using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Texture;
using Microsoft.Xna.Framework.Audio;
using System.Numerics;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms
{
    public class FlowerBed : Room
    {
        private FrontYard _yard;

        public override Theme Theme => new YardTheme();

        public override ILightingGroup LightingGroup => _yard;

        public override Side OmitSides => Side.Top | Side.South | Side.North | Side.West | Side.East;
        public FlowerBed(WorldSegment worldSegment, FrontYard yard, Driveway driveway) : base(worldSegment)
        {
            _yard = yard;
            Size = Vector3.One;
            Height = yard.Height;
            SetLocalSide(Side.North, yard.Deck.GetLocalSide(Side.South));
            SetLocalSide(Side.East, yard.Deck.GetLocalSide(Side.East));
            SetLocalSide(Side.Bottom, yard.GetLocalSide(Side.Bottom));
            SetLocalSideUnanchored(Side.South, driveway.GetLocalSide(Side.North));
            SetLocalSideUnanchored(Side.West, yard.FrontWalkway.WestPart.GetLocalSide(Side.East));
        }
    }
}
