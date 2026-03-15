using ExploringGame.Entities;
using Microsoft.Xna.Framework;
using ExploringGame.LevelControl;
using ExploringGame.Logics.Pathfinding;
using ExploringGame.Services;
using ExploringGame.Extensions;
using System;
using System.Linq;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GameDebug;

namespace ExploringGame.Logics.Controllers.LightSpiritPhases;

public class HalfPresencePhaseHandler : IPhaseHandler
{
    private readonly LightSpirit _lightSpirit;
    private readonly Player _player;
    private readonly Physics _physics;
    private readonly LoadedLevelData _loadedLevelData;
    private readonly Random _random;
    private WaypointGraph _waypointGraph;
    private PathFinder _pathFinder;
    private const float MovementSpeed = 2.5f;
    private Logics.EntityMover _entityMover;
    private bool _debugPause = false;
    public PathFinder PathFinder => _pathFinder;

    public HalfPresencePhaseHandler(LightSpirit lightSpirit, Player player, Physics physics, LoadedLevelData loadedLevelData, Random random)
    {
        _lightSpirit = lightSpirit;
        _random = random;
        _player = player;
        _physics = physics;
        _loadedLevelData = loadedLevelData;
    }

    //temporary
    MovingEntityDebugger _med;

    public void OnEnter()
    {
        _waypointGraph = _loadedLevelData.WaypointGraph;
        _pathFinder = new PathFinder(_physics, _waypointGraph, _lightSpirit, _random, new PathFinderTarget(_player));
        _entityMover = new Logics.EntityMover(_lightSpirit, _physics, ignoreY: false);
        _entityMover.Initialize();
        _entityMover.CollisionResponder.AddResponse(new DoorOpenCollisionResponse());
        _entityMover.Motion.Acceleration = MovementSpeed;
        _entityMover.Motion.Gravity = 0f;

        _med = new MovingEntityDebugger(_lightSpirit, _pathFinder);
    }

    public void Update(GameTime gameTime)
    {
        if (_debugPause)
            return;

        _med.Update(gameTime);

        // Get direction from pathfinder
        var direction = _pathFinder.GetTargetDirection(gameTime);
        if (float.IsNaN(direction.X) || float.IsNaN(direction.Y) || float.IsNaN(direction.Z))
            return;

        direction.Normalize();
        _entityMover.Motion.TargetMotion = direction * MovementSpeed;
        _entityMover.Motion.TargetY = 0f;
        _entityMover.Update(gameTime);

        // Optionally, keep the sphere underground if needed
        if (_lightSpirit.Sphere != null)
        {
            _lightSpirit.Sphere.Position = new Vector3(_lightSpirit.Position.X, -100f, _lightSpirit.Position.Z);
            if (_lightSpirit.Sphere.ColliderBodies != null && _lightSpirit.Sphere.ColliderBodies.Length > 0)
            {
                _lightSpirit.Sphere.ColliderBodies[0].Position = _lightSpirit.Sphere.Position.ToJVector();
            }
        }
    }


    // Collision response to open doors on contact
    private class DoorOpenCollisionResponse : Logics.Collision.ICollisionResponse
    {
        public void OnCollision(Jitter2.Dynamics.RigidBody thisBody, Jitter2.Dynamics.RigidBody otherBody)
        {
            var info = otherBody.CollisionInfo();
            if (info?.Shape is Door door && !door.Open)
                door.Open = true;
        }
    }

    public void DebugUpdate(IPlayerInput playerInput) 
    {
        if (playerInput.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.NumPad7))
        {
            _lightSpirit.Position = new Vector3(-7.92f, 6.1600003f, 13.22f);
            _lightSpirit.ColliderBodies[0].Position = _lightSpirit.Position.ToJVector();
        }

        if (playerInput.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.NumPad8))
        {
            _debugPause = !_debugPause;
            if (_debugPause)
                _lightSpirit.ColliderBodies[0].Velocity = new Jitter2.LinearMath.JVector(0f, 0f, 0f);
        }
    }

    public void OnExit() { }

    public string DebugDescribe() => string.Empty;

    public void ForceNextPhase() => _lightSpirit.Phase = LightSpiritPhase.FullPresence;
}

