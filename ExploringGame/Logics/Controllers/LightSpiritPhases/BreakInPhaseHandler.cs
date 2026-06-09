using ExploringGame.Entities;
using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Logics.Controllers.LightSpiritPhases;

public class BreakInPhaseHandler : IPhaseHandler
{
    private readonly TimeSpan GatemarkSpawnTime = TimeSpan.FromSeconds(30);

    private readonly Random _random;
    private readonly LightSpirit _lightSpirit;
    private readonly WorldSegment _worldSegment;
    private readonly LoadedLevelData _loadedLevelData;
    private readonly PointLights _pointLights;
    private GateMarkManager _gateMarkManager;
    private TimedAction _gateMarkSpawnAction;
    private GateMark _targetGateMark;
    
    private const float MovementSpeed = 2.0f; // Placeholder speed
    private const float ArrivalThreshold = 0.5f;

    public BreakInPhaseHandler(LightSpirit lightSpirit, WorldSegment worldSegment, 
        LoadedLevelData loadedLevelData, PointLights pointLights, Random random)
    {
        _lightSpirit = lightSpirit;
        _worldSegment = worldSegment;
        _loadedLevelData = loadedLevelData;
        _pointLights = pointLights;
        _random = random;
    }

    public void OnEnter()
    {
        // Initialize GateMark manager
        _gateMarkManager = new GateMarkManager(_worldSegment, _loadedLevelData, _pointLights, _random);
        
        _gateMarkSpawnAction = new TimedAction(GatemarkSpawnTime, () =>
        {
            _gateMarkManager.ActivateRandomGateMark();
            _gateMarkManager.SpawnGateMark();            
        });

        // Disable collision - LS passes through everything
        // (ColliderBodies already exist but we'll position them underground)
        SetUndergroundPosition();
    }

    public void Update(GameTime gameTime)
    {
        // Update gatemark spawning
        _gateMarkSpawnAction.Update(gameTime);

        // Find closest active gatemark if we don't have a target
        if (_targetGateMark == null || !_targetGateMark.IsActive)
        {
            _targetGateMark = _gateMarkManager.GetClosestActiveGateMark(_lightSpirit.LocalPosition);
        }

        // Move toward target gatemark if we have one
        if (_targetGateMark != null)
        {
            var direction = _targetGateMark.LocalPosition - _lightSpirit.LocalPosition;
            var distance = direction.Length();

            if (distance <= ArrivalThreshold)
            {
                // Reached the gatemark!
                var room = _targetGateMark.FindFirstAncestor<GeometryBuilder.Shapes.Room>();

                _gateMarkManager.RemoveGateMark(_targetGateMark);
                _targetGateMark = null;

                // Reposition LS to center of room to avoid being stuck near wall
                if (room != null)
                    _lightSpirit.LocalPosition = room.LocalPosition;

                // Transition to Half-Presence Phase
                _lightSpirit.Phase = LightSpiritPhase.HalfPresence;
            }
            else
            {
                // Move toward gatemark
                direction.Normalize();
                var movement = direction * MovementSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                _lightSpirit.LocalPosition += movement;
                
                // Keep sphere underground (invisible, no collision)
                SetUndergroundPosition();
            }
        }
    }

    public void OnExit()
    {
        // Clean up any remaining gatemarks
        var marks = _gateMarkManager.GateMarks.ToArray();
        foreach (var mark in marks)
        {
            _gateMarkManager.RemoveGateMark(mark);
        }
    }

    public string DebugDescribe()
    {
        if (_gateMarkManager == null)
            return string.Empty;

        var count = _gateMarkManager.GateMarks.Count;
        var mostRecent = _gateMarkManager.GateMarks.LastOrDefault();

        if (mostRecent == null)
            return $"GateMarks: {count}";

        // Get the parent room
        var room = mostRecent.FindFirstAncestor<GeometryBuilder.Shapes.Room>();
        var roomName = room?.Tag ?? room?.ToString() ?? "Unknown";

        return $"GateMarks: {count} | Recent: {roomName} {mostRecent.WallSide}";
    }

    public void ForceNextPhase()
    {
        // If there are any GateMarks, move LS to the nearest one
        GateMark target = null;
        if (_gateMarkManager.GateMarks.Any())
            target = _gateMarkManager.GetClosestActiveGateMark(_lightSpirit.LocalPosition);
        else
        {
            _gateMarkManager.SpawnGateMark();
            target = _gateMarkManager.GateMarks.FirstOrDefault();
        }

        if (target != null)
        {
            _lightSpirit.LocalPosition = target.LocalPosition;
            SetUndergroundPosition();
        }

        _lightSpirit.Phase = LightSpiritPhase.HalfPresence;
    }

    private void SetUndergroundPosition()
    {
        // Keep the sphere underground so it's invisible and has no collision
        _lightSpirit.Sphere.LocalPosition = new Vector3(_lightSpirit.LocalPosition.X, -100f, _lightSpirit.LocalPosition.Z);
        
        // Update physics body position to be underground too
        if (_lightSpirit.Sphere.ColliderBodies != null && _lightSpirit.Sphere.ColliderBodies.Length > 0)
        {
            _lightSpirit.Sphere.ColliderBodies[0].Position = _lightSpirit.Sphere.LocalPosition.ToJVector();
        }
    }

    public void DebugUpdate(IPlayerInput playerInput)
    {
        if (playerInput.IsKeyPressed(Keys.OemComma))
            _gateMarkSpawnAction.ForceReady();
    }
}

