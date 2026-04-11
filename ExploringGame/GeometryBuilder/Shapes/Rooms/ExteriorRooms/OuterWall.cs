using ExploringGame.Logics.Collision;
using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Services;
using ExploringGame.Texture;
using System.Linq;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms
{
    public class OuterWall : Shape, ICollidable
    {
        public static float WallThickness = 0.1f;

        private readonly Side _wallSide;
        private readonly Room _parentRoom;

        public override ViewFrom ViewFrom => ViewFrom.Outside;

        public override Theme Theme => new OuterWallTheme();

        public CollisionGroup CollisionGroup => CollisionGroup.Environment;

        public CollisionGroup CollidesWithGroups => CollisionGroup.MovingObjects;

        public override IColliderMaker ColliderMaker => new BoundingBoxColliderMaker(this);

        public OuterWall(Room room, Side wallSide)
        {
            _parentRoom = room;
            _wallSide = wallSide;

            room.AddChild(this);
            Height = room.Height;
            Width = wallSide.GetAxis() == Axis.X ? WallThickness : room.Width;
            Depth = wallSide.GetAxis() == Axis.Z ? WallThickness : room.Depth;

            this.Place().At(room).OnFloor().OnSideOuter(wallSide);
        }

        protected override Triangle[] BuildInternal(QualityLevel quality)
        {
            var shape = BuildCuboid();

            shape = new SideRemover().Execute(shape, _wallSide);

            // Apply cutouts for windows/doors from room connections
            var connections = _parentRoom.RoomConnections.Where(c => c.Side == _wallSide);
            foreach (var connection in connections)
            {
                shape = new RemoveSurfaceRegion().Execute(shape, connection.Side,
                    connection.CalcCutoutPlacement(shape), ViewFrom);
            }

            return shape;
        }
    }
}
