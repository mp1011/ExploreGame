using ExploringGame.Entities;
using ExploringGame.Motion;

namespace ExploringGame.Logics;

public class EntityMover
{
    public AcceleratedMotion Motion { get; }
    private IWithPosition _entity;

    public EntityMover(AcceleratedMotion motion, IWithPosition entity)
    {
        Motion = motion;
        _entity = entity;
    }

    public void Update()
    {
        Motion.Update();        
        _entity.Position += Motion.CurrentMotion;
    }
}
