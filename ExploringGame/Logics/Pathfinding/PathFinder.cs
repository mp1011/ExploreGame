using ExploringGame.Logics.Collision;
using ExploringGame.Services;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace ExploringGame.Logics.Pathfinding;

public class PathFinder
{
    private readonly Physics _physics;
    private readonly WaypointGraph _waypointGraph;
    private readonly ICollidable _entity;
    private const float _maxStuckMS = 500f;
    private float _randomWalkDuration;
    private Vector3 _randomWalk;

    public PathFinderTarget PrimaryTarget { get; set; }
    public PathFinderTarget CurrentTarget { get; set; }

    public PathFinder(Physics physics, WaypointGraph waypointGraph, ICollidable entity)
    {
        _physics = physics;
        _waypointGraph = waypointGraph;
        _entity = entity;
    }

    public Vector3 GetTargetDirection(GameTime gameTime)
    {
        if (_physics.HasLineOfSight(_entity, PrimaryTarget.Target))
            CurrentTarget = PrimaryTarget;

        if(_randomWalkDuration > 0)
        {
            _randomWalkDuration -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            return _randomWalk;
        }

        CurrentTarget.Update(gameTime, _entity);

        if (CurrentTarget.StuckTime > _maxStuckMS)
        {
            _randomWalkDuration = 1000f;
            var rng = new Random();
            _randomWalk = Vector3.Normalize(new Vector3(
                (float)(rng.NextDouble() * 2 - 1),
                0,
                (float)(rng.NextDouble() * 2 - 1)));

            CurrentTarget.ResetStuckTime();
        }

        if (!_physics.HasLineOfSight(_entity, CurrentTarget.Target) || HasReachedTarget())
            CurrentTarget = FindPathToTarget();
                       
        GameDebug.Debug.Watch1 = $"Current Target: {CurrentTarget}";
        return Vector3.Normalize(CurrentTarget.Target.Position - _entity.Position);        
    }

    private bool HasReachedTarget()
    {
        return (_entity.Position - CurrentTarget.Target.Position).Length() < 2.0f;
    }

    private PathFinderTarget FindPathToTarget()
    {
        var startWaypoint = _waypointGraph.FindNearestWaypoint(_entity.Position);
        var goalWaypoint = _waypointGraph.FindNearestWaypoint(PrimaryTarget.Target.Position);
        var currentPath = _waypointGraph.FindPath(startWaypoint, goalWaypoint);

        var newTarget = currentPath.TakeWhile(p => _physics.HasLineOfSight(_entity, p)).LastOrDefault() ?? CurrentTarget.Target;
        return new PathFinderTarget(newTarget);
    }
}
