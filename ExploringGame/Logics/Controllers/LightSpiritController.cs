using ExploringGame.Entities;
using ExploringGame.LevelControl;
using ExploringGame.Logics.Controllers.LightSpiritPhases;
using ExploringGame.Rendering;
using ExploringGame.Services;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ExploringGame.Logics.Controllers;

public enum LightSpiritPhase
{
    Absent,
    BreakIn,
    HalfPresence,
    FullPresence
}

public class LightSpiritController : IActiveObject
{
    private readonly Player _player;
    private readonly Physics _physics;
    private readonly LoadedLevelData _loadedLevelData;
    private readonly PointLights _pointLights;
    private readonly Dictionary<LightSpiritPhase, IPhaseHandler> _phaseHandlers;
    
    private LightSpiritPhase _currentPhase;
    
    public LightSpirit LightSpirit { get; set; }

    public LightSpiritController(Player player, Physics physics, LoadedLevelData loadedLevelData, PointLights pointLights)
    {
        _player = player;
        _physics = physics;
        _loadedLevelData = loadedLevelData;
        _pointLights = pointLights;
        
        _phaseHandlers = new Dictionary<LightSpiritPhase, IPhaseHandler>();
    }

    public void Initialize()
    {
        // Initialize physics for the sphere
        LightSpirit.Sphere.InitializePhysics(_physics);
        
        // Find the world segment this light spirit belongs to
        var worldSegment = LightSpirit.FindFirstAncestor<GeometryBuilder.Shapes.WorldSegments.WorldSegment>();
        
        // Create phase handlers
        _phaseHandlers[LightSpiritPhase.Absent] = new AbsentPhaseHandler(LightSpirit);
        _phaseHandlers[LightSpiritPhase.BreakIn] = new BreakInPhaseHandler(LightSpirit, worldSegment, _loadedLevelData, _pointLights);
        _phaseHandlers[LightSpiritPhase.HalfPresence] = new HalfPresencePhaseHandler(LightSpirit, _player);
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

        // Update debug display with phase info
        var phaseDebug = _phaseHandlers[_currentPhase].DebugDescribe();
        if (string.IsNullOrEmpty(phaseDebug))
        {
            GameDebug.Debug.Watch2 = $"LS Phase: {LightSpirit.Phase} | Health: {LightSpirit.Health}";
        }
        else
        {
            GameDebug.Debug.Watch2 = $"LS Phase: {LightSpirit.Phase} | Health: {LightSpirit.Health} | {phaseDebug}";
        }

        // Update current phase handler
        _phaseHandlers[_currentPhase].Update(gameTime);
    }
}

