using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms
{
    public class FrontDeck : Room
    {
        protected override Side OmitSides => Side.Top;

        public override Theme Theme => new ExteriorTheme();
        public FrontDeck(WorldSegment worldSegment) : base(worldSegment)
        {
        }
    }
}
