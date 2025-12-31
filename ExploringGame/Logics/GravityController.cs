using ExploringGame.Entities;
using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder;
using ExploringGame.Logics.Collision;
using ExploringGame.Motion;

namespace ExploringGame.Logics;

public class GravityController
{
    //temp
    private const float EntityHeight = 1.5f;

    private const float GravitySpeed = 0.05f;
    private const float GravityAccel = 0.004f;

    private IWithPosition _entity;
    private EntityCollider _collider;
    private EntityMover _gravityMover;

    public AcceleratedMotion Motion => _gravityMover.Motion;

    public GravityController(IWithPosition entity, EntityCollider collider, EntityMover gravityMover)
    {
        _entity = entity;
        _collider = collider;
        _gravityMover = gravityMover;
    }

    public void Update()
    {

        if (_collider.CurrentRoom == null)
            return;

        var floor = _collider.CurrentRoom.GetSide(Side.Bottom);

        if (_entity.Position.Y > floor + EntityHeight)
        {
            _gravityMover.Motion.TargetY = -GravitySpeed;
            _gravityMover.Motion.Acceleration = GravityAccel;
        }
            
        if (_entity.Position.Y < floor + EntityHeight)
        {
            _entity.Position = _entity.Position.SetY(floor + EntityHeight);
            _gravityMover.Motion.CurrentY = 0;
            _gravityMover.Motion.TargetY = 0;
        }
    }

    public bool CanJump()
    {
        if (_collider.CurrentRoom == null)
            return false;

        var floor = _collider.CurrentRoom.GetSide(Side.Bottom);
        return _entity.Position.Y <= floor + EntityHeight;
    }
}
