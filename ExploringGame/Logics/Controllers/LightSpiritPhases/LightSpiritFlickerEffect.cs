using ExploringGame.Entities;
using ExploringGame.GeometryBuilder;
using ExploringGame.LevelControl;
using ExploringGame.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Logics.Controllers.LightSpiritPhases;

/// <summary>
/// Manages the flickering effect of lights when the Light Spirit is nearby during Half Presence phase.
/// Lights closer to the Light Spirit flicker more intensely and gain a red tint.
/// </summary>
public class LightSpiritFlickerEffect
{
    private readonly LightSpirit _lightSpirit;
    private readonly LoadedLevelData _loadedLevelData;
    private readonly WaypointDistanceCalculator _distanceCalculator;
    private readonly Random _random;
    private readonly TimedAction _updateTargetsAction;
    private readonly Dictionary<ILightSource, LightFlickerState> _affectedLights = new();

    private readonly float SameRoomDistance = Measure.Feet(20);
    private readonly float NearbyRoomDistance = Measure.Feet(40);
    private const float MaxIntensityBoost = 2.0f;
    private const float TargetInterpolationTime = 0.2f;
    private const float RedTintStrength = 0.4f;

    public LightSpiritFlickerEffect(LightSpirit lightSpirit, LoadedLevelData loadedLevelData, WaypointDistanceCalculator distanceCalculator, Random random)
    {
        _lightSpirit = lightSpirit;
        _loadedLevelData = loadedLevelData;
        _distanceCalculator = distanceCalculator;
        _random = random;
        _updateTargetsAction = new TimedAction(TimeSpan.FromMilliseconds(500), UpdateFlickerTargets);
    }

    public void Update(GameTime gameTime)
    {
        _updateTargetsAction.Update(gameTime);
        InterpolateLights(gameTime);
    }

    private void UpdateFlickerTargets()
    {
        var allLights = _loadedLevelData.LoadedSegments
            .SelectMany(ld => ld.WorldSegment.TraverseAllChildren())
            .OfType<ILightSource>()
            .Where(light => light.On)
            .ToList();

        var currentlyAffectedLights = new HashSet<ILightSource>();

        foreach (var light in allLights)
        {
            var distance = _distanceCalculator.CalculateDistance(_lightSpirit.Position, light.LightPosition);
            if (distance == null)
                continue;

            var flickerLevel = CalculateFlickerLevel(distance.Value);

            if (flickerLevel > 0f)
            {
                currentlyAffectedLights.Add(light);

                if (!_affectedLights.ContainsKey(light))
                {
                    _affectedLights[light] = new LightFlickerState
                    {
                        OriginalIntensity = light.Intensity,
                        OriginalColor = light.Color,
                        CurrentIntensity = light.Intensity,
                        CurrentColor = light.Color
                    };
                }

                var state = _affectedLights[light];
                var randomBoost = (float)_random.NextDouble() * MaxIntensityBoost * flickerLevel;
                state.TargetIntensity = state.OriginalIntensity + randomBoost;

                var targetRedColor = Color.Lerp(state.OriginalColor, Color.Red, flickerLevel * RedTintStrength);
                state.TargetColor = targetRedColor;
            }
        }

        // For lights no longer affected, set targets back to original
        var lightsToRemove = new List<ILightSource>();
        foreach (var kvp in _affectedLights)
        {
            if (!currentlyAffectedLights.Contains(kvp.Key))
            {
                var state = kvp.Value;
                state.TargetIntensity = state.OriginalIntensity;
                state.TargetColor = state.OriginalColor;

                if (Math.Abs(state.CurrentIntensity - state.OriginalIntensity) < 0.01f &&
                    state.CurrentColor == state.OriginalColor)
                {
                    lightsToRemove.Add(kvp.Key);
                }
            }
        }

        foreach (var light in lightsToRemove)
        {
            _affectedLights.Remove(light);
        }
    }

    private void InterpolateLights(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        float t = Math.Min(deltaTime / TargetInterpolationTime, 1.0f);

        foreach (var kvp in _affectedLights)
        {
            var light = kvp.Key;
            var state = kvp.Value;

            state.CurrentIntensity = MathHelper.Lerp(state.CurrentIntensity, state.TargetIntensity, t);
            state.CurrentColor = Color.Lerp(state.CurrentColor, state.TargetColor, t);

            light.Intensity = state.CurrentIntensity;
            light.Color = state.CurrentColor;
        }
    }

    private float CalculateFlickerLevel(float distance)
    {
        if (distance < SameRoomDistance)
            return 1.0f;
        else if (distance < NearbyRoomDistance)
        {
            var t = (distance - SameRoomDistance) / (NearbyRoomDistance - SameRoomDistance);
            return 1.0f - t;
        }
        else
            return 0f;
    }

    private class LightFlickerState
    {
        public float OriginalIntensity { get; set; }
        public Color OriginalColor { get; set; }
        public float CurrentIntensity { get; set; }
        public Color CurrentColor { get; set; }
        public float TargetIntensity { get; set; }
        public Color TargetColor { get; set; }
    }
}
