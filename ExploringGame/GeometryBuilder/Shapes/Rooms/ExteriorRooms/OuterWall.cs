using ExploringGame.Logics.Collision;
using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Services;
using ExploringGame.Texture;
using System.Collections.Generic;
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

        public Side WallSide => _wallSide;

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
            foreach (var connection in _parentRoom.RoomConnections.Where(c => c.Side == _wallSide))
            {
                var cutoutPlacement = RoomConnection.CalcCutoutPlacement(shape, _wallSide.Opposite(), this, connection.GetOtherRoom(_parentRoom));
                shape = new RemoveSurfaceRegion().Execute(shape, connection.Side.Opposite(), cutoutPlacement, ViewFrom);
            }

            return shape;
        }



    }
}
