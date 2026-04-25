using ExploringGame.GeometryBuilder.Shapes.Structures;
using ExploringGame.Logics.Collision;
using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms
{
    public class OuterWall : Shape, ICollidable
    {
        public static float WallThickness = 0.1f;
        public static float StandardSpacingForGround = 0.5f;

        private readonly Side _wallSide;
        private readonly Side _moulding;
        private readonly Room _parentRoom;
        private bool _addedSiding = false;
        public override ViewFrom ViewFrom => ViewFrom.Outside;
        public List<VertexOffset> VertexOffsets { get; } = new();


        public override Theme Theme => new OuterWallTheme();

        public CollisionGroup CollisionGroup => CollisionGroup.Environment;

        public CollisionGroup CollidesWithGroups => CollisionGroup.MovingObjects;

        public override IColliderMaker ColliderMaker => ColliderMakers.Room(this);

        public Side WallSide => _wallSide;

        public OuterWall(Room room, Side wallSide, Side? moulding = null)
        {
            _parentRoom = room;
            _wallSide = wallSide;

            if (moulding == null)
                _moulding = _wallSide.ClockwiseTurn() | _wallSide.CounterClockwiseTurn();
            else
                _moulding = moulding.Value;

            room.AddChild(this);
            Height = _parentRoom.Height;
            Width = _wallSide.GetAxis() == Axis.X ? WallThickness : _parentRoom.Width;
            Depth = _wallSide.GetAxis() == Axis.Z ? WallThickness : _parentRoom.Depth;

            this.Place().At(_parentRoom).OnFloor().OnSideOuter(_wallSide);  
        }

        protected override void BeforeBuild()
        {
            if (_addedSiding)
                return;

            if (_moulding != Side.None)
            {
                new Moulding(this, _wallSide.Opposite(), _moulding,
                    color: Color.White,
                    size: Measure.Feet(1),
                    thickness: Measure.Inches(2),
                    inner: true);
            }

            _addedSiding = true;
        }

        protected override Triangle[] BuildInternal(QualityLevel quality)
        {
            var shape = BuildCuboid();

            foreach (var vertexOffset in VertexOffsets)
                shape = new VertexOffsetter().Execute(this, shape, vertexOffset);

            shape = new SideRemover().Execute(shape, _wallSide);

            // Apply cutouts for windows/doors from room connections
            // Simple approach: just subtract each cutout shape from the triangles
            foreach (var connection in _parentRoom.RoomConnections.Where(c => c.Side == _wallSide))
            {
                var cutoutShape = connection.GetOtherRoom(_parentRoom);
                shape = new RemoveSurfaceRegion().SubtractShape(shape, cutoutShape);
            }

            return shape;          
        }



    }
}
