using ExploringGame.Logics.Collision;
using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Services;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms
{
    public class OuterWall : Shape, ICollidable
    {
        public static float WallThickness = 0.1f;

        public override ViewFrom ViewFrom => ViewFrom.Outside;

        public override Theme Theme => new OuterWallTheme();

        public CollisionGroup CollisionGroup => CollisionGroup.Environment;

        public CollisionGroup CollidesWithGroups => CollisionGroup.MovingObjects;

        public override IColliderMaker ColliderMaker => new BoundingBoxColliderMaker(this);

        public OuterWall(Shape ground, Side wallSide)
        {
            ground.AddChild(this);
            Height = ground.Height;
            Width = wallSide.GetAxis() == Axis.X ? WallThickness : ground.Width;
            Depth = wallSide.GetAxis() == Axis.Z ? WallThickness : ground.Depth;

            this.Place().At(ground).OnFloor().OnSideOuter(wallSide);
        }

        protected override Triangle[] BuildInternal(QualityLevel quality)
        {
            return BuildCuboid();
        }
    }
}
