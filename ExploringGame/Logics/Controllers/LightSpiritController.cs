using ExploringGame.Entities;
using ExploringGame.GameDebug;
using ExploringGame.LevelControl;
using ExploringGame.Logics.Collision;
using ExploringGame.Logics.Controllers.LightSpiritPhases;
using ExploringGame.Rendering;
using ExploringGame.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Logics.Controllers;

public enum LightSpiritPhase
{
    Absent,
    BreakIn,
    HalfPresence,
    FullPresence
}

public class LightSpiritController : IActiveObject, IDebugControllable
{
    private readonly Random _random;
    private readonly Player _player;
    private readonly Physics _physics;
    private readonly LoadedLevelData _loadedLevelData;
    private readonly PointLights _pointLights;
    private readonly Dictionary<LightSpiritPhase, IPhaseHandler> _phaseHandlers;
    
    private LightSpiritPhase _currentPhase;
    
    public LightSpirit LightSpirit { get; set; }

    public LightSpiritController(Player player, Physics physics, LoadedLevelData loadedLevelData, PointLights pointLights, Random random)
    {
        _random = random;
        _player = player;
        _physics = physics;
        _loadedLevelData = loadedLevelData;
        _pointLights = pointLights;
        
        _phaseHandlers = new Dictionary<LightSpiritPhase, IPhaseHandler>();
    }

    public void Initialize()
    {        
        // Find the world segment this light spirit belongs to
        var worldSegment = LightSpirit.FindFirstAncestor<GeometryBuilder.Shapes.WorldSegments.WorldSegment>();
        
        // Create phase handlers
        _phaseHandlers[LightSpiritPhase.Absent] = new AbsentPhaseHandler(LightSpirit);
        _phaseHandlers[LightSpiritPhase.BreakIn] = new BreakInPhaseHandler(LightSpirit, worldSegment, _loadedLevelData, _pointLights, _random);
        _phaseHandlers[LightSpiritPhase.HalfPresence] = new HalfPresencePhaseHandler(LightSpirit, _player, _physics, _loadedLevelData, _random);
        _phaseHandlers[LightSpiritPhase.FullPresence] = new FullPresencePhaseHandler(LightSpirit, _player);
        
        // Start in Absent phase
        _currentPhase = LightSpiritPhase.Absent;
        LightSpirit.Phase = LightSpiritPhase.Absent;
        _phaseHandlers[_currentPhase].OnEnter();
    }

    public void Stop()
    {
        _phaseHandlers[_currentPhase]?.OnExit();
    }

    public void Update(GameTime gameTime)
    {
        // Check for phase transitions
        if (LightSpirit.Phase != _currentPhase)
        {
            // Exit old phase
            _phaseHandlers[_currentPhase].OnExit();

            // Enter new phase
            _currentPhase = LightSpirit.Phase;
            _phaseHandlers[_currentPhase].OnEnter();
        }

        // Calculate light level at Light Spirit's location
        float lightLevel = CalculateLightLevel();

        // Update debug display with phase info and light level
        var phaseDebug = _phaseHandlers[_currentPhase].DebugDescribe();
        if (string.IsNullOrEmpty(phaseDebug))
        {
            GameDebug.Debug.Watch2 = $"LS Phase: {LightSpirit.Phase} | Health: {LightSpirit.Health} | Light: {lightLevel:F2}";
        }
        else
        {
            GameDebug.Debug.Watch2 = $"LS Phase: {LightSpirit.Phase} | Health: {LightSpirit.Health} | Light: {lightLevel:F2} | {phaseDebug}";
        }

        // Update current phase handler
        _phaseHandlers[_currentPhase].Update(gameTime);
    }

    /// <summary>
    /// Calculate the light level at the Light Spirit's current position
    /// </summary>
    private float CalculateLightLevel()
    {
        // Get the room the Light Spirit is in
        var room = _loadedLevelData.RoomGraph?.GetAllRooms()
            .FirstOrDefault(r => r.ContainsPoint(LightSpirit.Position));

        if (room == null || _loadedLevelData.LightingCalculator == null)
            return 0f;

        // Get the room's base light level
        if (!_loadedLevelData.LightingCalculator.RoomLightGraph.TryGet(room, out var lightData))
            return 0f;

        float roomLight = lightData.GetTotalLight();

        // Check for direct line of sight to any light sources
        var lightSources = lightData.GetLightSources().Where(ls => ls.On);
        foreach (var light in lightSources)
        {
            // Simple ray check - could use physics if needed
            // For now, just use distance-based bonus for lights in same room
            float distance = Vector3.Distance(LightSpirit.Position, light.LightPosition);
            if (distance < 10f) // Within 10 units
            {
                float directBonus = light.Intensity / System.Math.Max(1f, distance / 5f);
                roomLight = System.Math.Max(roomLight, directBonus);
            }
        }

        return roomLight;
    }

    public void DebugUpdate(IPlayerInput playerInput)
    {
        if (playerInput.IsKeyPressed(Keys.PageDown))
            _phaseHandlers[_currentPhase].ForceNextPhase(); 

        _phaseHandlers[_currentPhase].DebugUpdate(playerInput);
    }
}

