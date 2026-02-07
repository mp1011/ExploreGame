using ExploringGame.Entities;
using ExploringGame.Logics.Collision;
using ExploringGame.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace ExploringGame.Logics.Pathfinding;

public class PathFinder
{
    private const int NumOffsetRays = 8;
    private const float OffsetAngle = MathHelper.TwoPi / NumOffsetRays;
    private const float WaypointArrivalThreshold = 2.0f;

    private readonly Physics _physics;
    private readonly WaypointGraph _waypointGraph;
    private List<Waypoint> _currentPath;
    private int _currentWaypointIndex;
    private Waypoint _lastTargetedWaypoint;

    public PathFinder(Physics physics, WaypointGraph waypointGraph)
    {
        _physics = physics;
        _waypointGraph = waypointGraph;
        _currentPath = new List<Waypoint>();
    }

    private void UpdateTargetedMarker(Waypoint newTarget)
    {
        if (_lastTargetedWaypoint != null)
        {
            _lastTargetedWaypoint.IsTargeted = false;
        }

        _lastTargetedWaypoint = newTarget;
        
        if (_lastTargetedWaypoint != null)
        {
            _lastTargetedWaypoint.IsTargeted = true;
        }
    }

    public Vector3 GetTargetDirection(ICollidable entity, Player player)
    {
        // Step 1: Try direct line of sight
        if (_physics.HasLineOfSight(entity, player))
        {
            GameDebug.Debug.Watch1 = "Direct LOS";
            _currentPath.Clear();
            UpdateTargetedMarker(null);
            return Vector3.Normalize(player.Position - entity.Position);
        }

        // Step 2: Try offset raycasts (temporarily disabled)
        //var offsetDirection = TryOffsetRaycasts(entity, player);
        //if (offsetDirection.HasValue)
        //{
        //    _currentPath.Clear();
        //    UpdateTargetedMarker(null);
        //    GameDebug.Debug.Watch1 = "Offset LOS";
        //    return offsetDirection.Value;
        //}

        GameDebug.Debug.Watch1 = "Waypoint Nav";
        // Step 3: Use waypoint navigation
        return NavigateViaWaypoints(entity, player);
    }

    private Vector3? TryOffsetRaycasts(ICollidable entity, Player player)
    {
        var directionToPlayer = player.Position - entity.Position;
        directionToPlayer.Y = 0;
        
        if (directionToPlayer.LengthSquared() < 0.01f)
            return null;

        directionToPlayer.Normalize();
        var baseAngle = (float)Math.Atan2(directionToPlayer.X, directionToPlayer.Z);

        Vector3? bestDirection = null;
        float bestScore = float.MinValue;

        for (int i = 0; i < NumOffsetRays; i++)
        {
            var angle = baseAngle + (i * OffsetAngle);
            var testDirection = new Vector3(
                (float)Math.Sin(angle),
                0,
                (float)Math.Cos(angle)
            );

            // Create a test target position along this direction
            var testTarget = new TestTarget(entity.Position + testDirection * 10.0f);
            var rayResult = _physics.Raycast(entity, testTarget);

            if (rayResult.HitObject != null)
            {
                // Calculate how close this gets us to the player
                var hitPoint = entity.Position + testDirection * rayResult.Lambda;
                var distanceToPlayer = Vector3.Distance(hitPoint, player.Position);
                var score = -distanceToPlayer; // Negative because we want minimum distance

                if (score > bestScore)
                {
                    bestScore = score;
                    bestDirection = testDirection;
                }
            }
        }

        return bestDirection;
    }

    private Vector3 NavigateViaWaypoints(ICollidable entity, Player player)
    {
        // Check if we need to recalculate path
        if (_currentPath.Count == 0 || _currentWaypointIndex >= _currentPath.Count)
        {
            var startWaypoint = _waypointGraph.FindNearestWaypoint(entity.Position);
            var goalWaypoint = _waypointGraph.FindNearestWaypoint(player.Position);

            _currentPath = _waypointGraph.FindPath(startWaypoint, goalWaypoint);
            
            // Always skip the first waypoint - it's the room the entity is already in
            // Start at index 1 (the next room to move towards)
            _currentWaypointIndex = _currentPath.Count > 1 ? 1 : 0;
        }

        if (_currentPath.Count == 0)
        {
            UpdateTargetedMarker(null);
            // No path found, move directly toward player
            var direction = player.Position - entity.Position;
            direction.Y = 0;
            return direction.LengthSquared() > 0.01f ? Vector3.Normalize(direction) : Vector3.Zero;
        }

        // Move toward current waypoint
        var targetWaypoint = _currentPath[_currentWaypointIndex];
        UpdateTargetedMarker(targetWaypoint);
        
        var directionToWaypoint = targetWaypoint.Position - entity.Position;
        directionToWaypoint.Y = 0;

        // Check if we've reached the current waypoint
        if (directionToWaypoint.LengthSquared() < WaypointArrivalThreshold * WaypointArrivalThreshold)
        {
            _currentWaypointIndex++;
            
            // If we've reached the last waypoint, clear the path
            if (_currentWaypointIndex >= _currentPath.Count)
            {
                _currentPath.Clear();
            }
        }

        return directionToWaypoint.LengthSquared() > 0.01f ? Vector3.Normalize(directionToWaypoint) : Vector3.Zero;
    }

    // Helper class for raycast testing
    private class TestTarget : IWithPosition
    {
        public Vector3 Position { get; set; }
        public GeometryBuilder.Rotation Rotation { get; set; }
        public Vector3 Size { get; set; }

        public TestTarget(Vector3 position)
        {
            Position = position;
            Size = Vector3.One;
        }
    }
}
