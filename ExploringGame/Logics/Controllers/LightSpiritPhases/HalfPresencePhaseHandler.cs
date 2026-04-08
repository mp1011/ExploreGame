using ExploringGame.Entities;
using Microsoft.Xna.Framework;
using ExploringGame.LevelControl;
using ExploringGame.Logics.Pathfinding;
using ExploringGame.Services;
using ExploringGame.Extensions;
using System;
using System.Linq;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GameDebug;
using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.GeometryBuilder.Shapes.Structures;

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
    private TimedAction _lightSwitchCheck;
    private const float LightSwitchActivationDistance = 4.0f;
    private EntityRoomFinder _entityRoomFinder;
    private LightSpiritFlickerEffect _flickerEffect;
    public PathFinder PathFinder => _pathFinder;

    public HalfPresencePhaseHandler(LightSpirit lightSpirit, Player player, Physics physics, LoadedLevelData loadedLevelData, Random random)
    {
        _lightSpirit = lightSpirit;
        _random = random;
        _player = player;
        _physics = physics;
        _loadedLevelData = loadedLevelData;
    }

    public void OnEnter()
    {
        _waypointGraph = _loadedLevelData.WaypointGraph;
        _pathFinder = new PathFinder(_physics, _waypointGraph, _lightSpirit, _random, new PathFinderTarget(_player));
        _entityMover = new Logics.EntityMover(_lightSpirit, _physics, ignoreY: false);
        _entityMover.Initialize();
        _entityMover.CollisionResponder.AddResponse(new DoorOpenCollisionResponse());
        _entityMover.Motion.Acceleration = MovementSpeed;
        _entityMover.Motion.Gravity = 0f;
        _entityRoomFinder = new EntityRoomFinder(_loadedLevelData);

        _lightSwitchCheck = new TimedAction(TimeSpan.FromSeconds(1), CheckForLightSwitches);

        var distanceCalculator = new WaypointDistanceCalculator(_loadedLevelData);
        _flickerEffect = new LightSpiritFlickerEffect(_lightSpirit, _loadedLevelData, distanceCalculator, _random);
    }

    public void Update(GameTime gameTime)
    {
        // Update light flickering effect
        _flickerEffect.Update(gameTime);

        if (_debugPause)
            return;

        // Update the Light Spirit's room
        _entityRoomFinder.UpdateRoom(_lightSpirit);


        // Check for light switches to turn on (once per second, priority for LS)
        bool isTargetingLightSwitch = _pathFinder.CurrentTarget?.Target is LightSwitch;
        if (!isTargetingLightSwitch)
        {
            _lightSwitchCheck.Update(gameTime);
        }

        // If targeting a light switch, check if close enough to turn it on
        if (isTargetingLightSwitch)
        {
            var targetSwitch = _pathFinder.CurrentTarget.Target as LightSwitch;
            var distanceToSwitch = Vector3.Distance(_lightSpirit.Position, targetSwitch.Position);

            if (distanceToSwitch <= Measure.Feet(LightSwitchActivationDistance))
            {
                if (targetSwitch.Controller != null)
                {
                    targetSwitch.Controller.On = true;
                }

                // Reset target back to player
                _pathFinder.CurrentTarget = _pathFinder.PrimaryTarget;
            }
        }

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

    private void CheckForLightSwitches()
    {
        var room = _lightSpirit.Room;
        if (room != null)
        {
            var offLightSwitch = room.TraverseAllChildren()
                .OfType<LightSwitch>()
                .FirstOrDefault(ls => ls.Controller != null && !ls.Controller.On);

            if (offLightSwitch != null)
            {
                _pathFinder.CurrentTarget = new PathFinderTarget(offLightSwitch);
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

    public string DebugDescribe() => $"Target = {_pathFinder.CurrentTarget.Target}";
    public void ForceNextPhase() => _lightSpirit.Phase = LightSpiritPhase.FullPresence;
}

