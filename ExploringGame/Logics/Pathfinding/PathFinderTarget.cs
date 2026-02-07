using ExploringGame.Extensions;
using ExploringGame.Logics.Collision;
using Microsoft.Xna.Framework;

namespace ExploringGame.Logics.Pathfinding;

public class PathFinderTarget
{
    public ICollidable Target { get; }

    private float _lastDistance = float.MaxValue;
    public float StuckTime { get; private set; }

    public PathFinderTarget(ICollidable target)
    {
        Target = target;

    }

    public void Update(GameTime gameTime, ICollidable entity)
    {
        var distance = entity.Position.DistanceTo(Target.Position);

        if(distance >= _lastDistance)
            StuckTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

        _lastDistance = distance;
    }

    public void ResetStuckTime()
    {
        StuckTime = 0;
    }

    public override string ToString() => Target.ToString();
}
