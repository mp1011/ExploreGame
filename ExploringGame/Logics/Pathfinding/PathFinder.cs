using ExploringGame.Logics.Collision;
using ExploringGame.Services;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using ExploringGame.GeometryBuilder;
using Jitter2.Dynamics;
using ExploringGame.Entities;
using ExploringGame.Extensions;
using System.Collections.Generic;

namespace ExploringGame.Logics.Pathfinding;

public class PathFinder
{
    private readonly Physics _physics;
    private readonly WaypointGraph _waypointGraph;
    private readonly ICollidable _entity;
    private readonly Random _random;
    private readonly PathfinderSamplePoint _samplePoint;
    private const float _maxStuckMS = 500f;
    private TimeSpan _randomWalkDuration;
    private Vector3 _randomWalk;
    private Vector3? _previousDirectionToTarget;
    private float _previousDistanceToTarget;
    private Waypoint _lastWaypointTarget;
    private Vector3 _cachedDirection;
    private TimeSpan _timeSinceLastDirectionComputation = TimeSpan.FromHours(1);
    private TimeSpan _timeSeekingSamplePoint;

    private const int NumSamples = 30;
    private const float MinSampleDistance = 0.5f;
    private const float MaxSampleDistance = 4.0f;
    private static readonly TimeSpan DirectionComputationInterval = TimeSpan.FromMilliseconds(200);
    private static readonly TimeSpan MaxSamplePointSeekTime = TimeSpan.FromSeconds(3);

    public PathFinderTarget PrimaryTarget { get; }

    private PathFinderTarget _currentTarget;
    public PathFinderTarget CurrentTarget 
    { 
        get => _currentTarget;
        set
        {
            if (value.Target != _currentTarget?.Target)
            {
                _currentTarget = value;
                _previousDirectionToTarget = null;
                _previousDistanceToTarget = float.MaxValue;
                _timeSeekingSamplePoint = TimeSpan.Zero;
            }

            if (value.Target is Waypoint w)
                _lastWaypointTarget = w;
        }
    }

    public PathFinder(Physics physics, WaypointGraph waypointGraph, ICollidable entity, Random random, PathFinderTarget primaryTarget)
    {
        _physics = physics;
        _waypointGraph = waypointGraph;
        _entity = entity;
        _random = random;
        PrimaryTarget = primaryTarget ?? throw new ArgumentNullException(nameof(primaryTarget));
        CurrentTarget = PrimaryTarget;
        _samplePoint = new PathfinderSamplePoint(physics, Vector3.Zero);
    }

    public Vector3 GetTargetDirection(GameTime gameTime)
    {
        _timeSinceLastDirectionComputation += gameTime.ElapsedGameTime;

        // Track time seeking sample point and give up if it takes too long
        if (CurrentTarget?.Target is PathfinderSamplePoint)
        {
            _timeSeekingSamplePoint += gameTime.ElapsedGameTime;

            if (_timeSeekingSamplePoint >= MaxSamplePointSeekTime && _lastWaypointTarget != null)
            {
                CurrentTarget = new PathFinderTarget(_lastWaypointTarget);
                _timeSinceLastDirectionComputation = DirectionComputationInterval;
            }
        }

        if (_timeSinceLastDirectionComputation < DirectionComputationInterval)
            return _cachedDirection;

        var t = CurrentTarget.Target;
        _timeSinceLastDirectionComputation = TimeSpan.Zero;
        _cachedDirection = ComputeTargetDirection(gameTime);

        return _cachedDirection;
    }

    private Vector3 ComputeTargetDirection(GameTime gameTime)
    {
        // Step 1: Does Entity have a Line of Sight to Primary Target?
        if (_physics.HasLineOfSight(_entity, PrimaryTarget.Target))
        {
            CurrentTarget = PrimaryTarget;
            return Vector3.Normalize(PrimaryTarget.Target.Position - _entity.Position);
        }

        // Step 2: Has Entity reached its Current Target?
        if (HasReachedTarget())
        {
            // if we were following a sample point, switch back to the waypoint we were originally trying to reach
            var recomputedTarget = CurrentTarget.Target is PathfinderSamplePoint
                ? new PathFinderTarget(_lastWaypointTarget) 
                : PickNextTargetAfterCurrent();

            CurrentTarget = recomputedTarget;
            return Vector3.Normalize(CurrentTarget.Target.Position - _entity.Position);
        }

        // Step 3: Does Entity have a Line of Sight to its Current Target?
        if (_physics.HasLineOfSight(_entity, CurrentTarget.Target))
        {
            return Vector3.Normalize(CurrentTarget.Target.Position - _entity.Position);
        }

        // Step 3.5: Detect if stuck on obstacle and need to find alternate route
        if (IsStuckOnObstacle())
        {
            // Jump to Step 6: Pick a valid Random Sample Point
            var obstacleAvoidanceSamplePoint = FindValidSamplePoint();
            if (obstacleAvoidanceSamplePoint.HasValue)
            {
                _samplePoint.Position = obstacleAvoidanceSamplePoint.Value;
                CurrentTarget = new PathFinderTarget(_samplePoint);
                return Vector3.Normalize(CurrentTarget.Target.Position - _entity.Position);
            }
        }

        // Step 4: Is the Random Walk Timer > 0?
        if (_randomWalkDuration > TimeSpan.Zero)
        {
            _randomWalkDuration -= gameTime.ElapsedGameTime;
            return _randomWalk;
        }

        // Step 5: Try to compute a new path. Take only if new path doesn't include the current target
        var path = FindPathToTarget();
        if (path.Count == 0)
            return Vector3.Zero;

        if(!path.Contains(CurrentTarget.Target))
        {
            CurrentTarget = new PathFinderTarget(path.First());
            return Vector3.Normalize(CurrentTarget.Target.Position - _entity.Position);
        }

        // Step 6: Pick a valid Random Sample Point
        var samplePoint = FindValidSamplePoint();
        if (samplePoint.HasValue)
        {
            _samplePoint.Position = samplePoint.Value;
            CurrentTarget = new PathFinderTarget(_samplePoint);
            return Vector3.Normalize(CurrentTarget.Target.Position - _entity.Position);
        }

        // Step 7: Set the Random Walk timer
        _randomWalkDuration = TimeSpan.FromMilliseconds(_random.NextDouble() * 1500 + 500); // 500-2000ms
        var directionToTarget = Vector3.Normalize(CurrentTarget.Target.Position - _entity.Position);
        var angle = (float)(_random.NextDouble() * Math.PI / 2 - Math.PI / 4); // +- 45 degrees
        _randomWalk = RotateVector2D(directionToTarget, angle);

        return _randomWalk;
    }

    private bool HasReachedTarget()
    {
        var directionToTarget = CurrentTarget.Target.Position - _entity.Position;
        var directionToTargetXZ = new Vector3(directionToTarget.X, 0, directionToTarget.Z);
        var currentDistance = directionToTargetXZ.Length();
        var threshold = Measure.Feet(1);

        // Check if within threshold
        if (currentDistance < threshold)
        {
            _previousDirectionToTarget = directionToTarget;
            return true;
        }

        // Check if we overshot by comparing angles
        // If the angle between previous direction and current direction is > 90 degrees,
        // we've passed the target
        if (_previousDirectionToTarget.HasValue && currentDistance < Measure.Feet(5))
        {
            var previousDirXZ = new Vector3(_previousDirectionToTarget.Value.X, 0, _previousDirectionToTarget.Value.Z);
            var previousDir = Vector3.Normalize(previousDirXZ);
            var currentDir = Vector3.Normalize(directionToTargetXZ);
            var dotProduct = Vector3.Dot(previousDir, currentDir);

            // Dot product < 0 means angle > 90 degrees (we've passed the target)
            if (dotProduct < 0)
            {
                _previousDirectionToTarget = directionToTarget;
                return true;
            }
        }

        _previousDirectionToTarget = directionToTarget;
        _previousDistanceToTarget = currentDistance;
        return false;
    }

    private List<Waypoint> FindPathToTarget()
    {
        var startWaypoint = _waypointGraph.FindNearestWaypoint(_entity.Position);
        var goalWaypoint = _waypointGraph.FindNearestWaypoint(PrimaryTarget.Target.Position);

        if (startWaypoint == null || goalWaypoint == null)
            return new();

        var path = _waypointGraph.FindPath(startWaypoint, goalWaypoint);
        return path ?? new();
    }

    private PathFinderTarget PickNextTargetAfterCurrent()
    {
        var path = FindPathToTarget();

        var currentTargetIndex = path.FindIndex(p => p == CurrentTarget.Target);

        Waypoint newTarget;
        if (currentTargetIndex >= 0)
            newTarget = path.Skip(currentTargetIndex + 1).FirstOrDefault();
        else
            newTarget = path.FirstOrDefault();

        if (newTarget == null)
            return CurrentTarget;
        else
            return new PathFinderTarget(newTarget);
    }

    private Vector3? FindValidSamplePoint()
    {
        Vector3? bestPoint = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < NumSamples; i++)
        {
            // Generate random point between MinSampleDistance and MaxSampleDistance from Entity
            var angle = (float)(_random.NextDouble() * Math.PI * 2);
            var distance = (float)(_random.NextDouble() * (MaxSampleDistance - MinSampleDistance) + MinSampleDistance);

            var samplePoint = _entity.Position + new Vector3(
                (float)Math.Cos(angle) * distance,
                0,
                (float)Math.Sin(angle) * distance);

            // Check if sample point has line of sight to both Entity and Current Target
            _samplePoint.Position = samplePoint;

            if (_physics.HasLineOfSight(_samplePoint, _entity) && 
                _physics.HasLineOfSight(_samplePoint, CurrentTarget.Target))
            {
                var distanceToEntity = Vector3.Distance(_entity.Position, samplePoint);
                if (distanceToEntity < bestDistance)
                {
                    bestDistance = distanceToEntity;
                    bestPoint = samplePoint;
                }
            }
        }

        return bestPoint;
    }

    private bool IsStuckOnObstacle()
    {
        // Check condition 1: LS does not have line of sight to target (primary target)
        if (_physics.HasLineOfSight(_entity, PrimaryTarget.Target))
            return false;

        // Check condition 2: target is NOT a PathfinderSamplePoint
        if (CurrentTarget.Target is PathfinderSamplePoint)
            return false;

        // Check condition 3: there is no waypoint closer to the target than the one the LS is already at
        var currentWaypoint = _waypointGraph.FindNearestWaypoint(_entity.Position);
        if (currentWaypoint == null)
            return false;

        var currentWaypointDistanceToTarget = Vector3.Distance(currentWaypoint.Position, PrimaryTarget.Target.Position);

        // Check all waypoints to see if any are closer to the target
        foreach (var waypoint in _waypointGraph.GetAllWaypoints())
        {
            if (waypoint == currentWaypoint)
                continue;

            var distanceToTarget = Vector3.Distance(waypoint.Position, PrimaryTarget.Target.Position);
            if (distanceToTarget < currentWaypointDistanceToTarget)
            {
                // Found a waypoint closer to target, so not stuck
                return false;
            }
        }

        // All conditions met - we're stuck on an obstacle
        return true;
    }

    private Vector3 RotateVector2D(Vector3 vector, float angle)
    {
        var cos = (float)Math.Cos(angle);
        var sin = (float)Math.Sin(angle);
        return Vector3.Normalize(new Vector3(
            vector.X * cos - vector.Z * sin,
            vector.Y,
            vector.X * sin + vector.Z * cos));
    }

    private class PathfinderSamplePoint : ICollidable
    {
        private Vector3 _position;
        private readonly RigidBody _body;

        public Vector3 Position 
        { 
            get => _position;
            set
            {
                _position = value;
                if (_body != null)
                    _body.Position = value.ToJVector();
            }
        }

        public Vector3 Size => new Vector3(0.2f, 0.2f, 0.2f);
        public Rotation Rotation { get; set; }
        public CollisionGroup CollisionGroup => CollisionGroup.LineOfSightTest;
        public CollisionGroup CollidesWithGroups => CollisionGroup.None;
        public RigidBody[] ColliderBodies { get; }

        public PathfinderSamplePoint(Physics physics, Vector3 position)
        {
            _position = position;
            _body = physics.CreateStaticBody(this, CollisionGroup.LineOfSightTest, CollisionGroup.Environment);
            ColliderBodies = new[] { _body };
            Rotation = null;
        }

        public override string ToString()
        {
            return "SamplePoint";
        }
    }
}
