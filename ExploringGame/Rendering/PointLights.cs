using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using ExploringGame.Logics;

namespace ExploringGame.Rendering;

public record PointLight(int Index, Vector3 Position, Color Color, float Intensity)
{
    public bool On => Intensity > 0f;

    public PointLight TurnOff() => new PointLight(Index, Vector3.Zero, Color.White, 0f);

    public static PointLight DefaultOff => new PointLight(-1, Vector3.Zero, Color.White, 0f);
}

public class PointLights
{
    public const int MAX_LIGHTS = 20;

    private PointLight[] _lights;
    private Dictionary<ILightSource, int> _lightSourceToIndex = new();

    public Vector3[] Positions { get; private set; }
    public Vector3[] Colors { get; private set; }
    public float[] Intensities { get; private set; }


    public PointLights()
    {
        _lights = Enumerable.Range(0, MAX_LIGHTS)
            .Select(p => new PointLight(p, Vector3.Zero, Color.White, 0f))
            .ToArray();

        RefreshArrays();
    }

    public PointLight AddLight(Vector3 position, Color color, float intensity)
    {
        var existing = _lights.FirstOrDefault(p => p.Position == position && p.Color == color && p.Intensity == intensity);

        if (existing != null)
            return existing ?? PointLight.DefaultOff;

        var firstFree = _lights.FirstOrDefault(p => !p.On);
        if (firstFree == null)
            return PointLight.DefaultOff;

        _lights[firstFree.Index] = new PointLight(firstFree.Index, position, color, intensity);
        RefreshArrays();
        return _lights[firstFree.Index];
    }

    public PointLight AddLight(ILightSource lightSource)
    {
        if (_lightSourceToIndex.ContainsKey(lightSource))
        {
            // Update existing light
            var index = _lightSourceToIndex[lightSource];
            _lights[index] = new PointLight(index, lightSource.LightPosition, lightSource.Color, lightSource.Intensity);
            RefreshArrays();
            return _lights[index];
        }

        var firstFree = _lights.FirstOrDefault(p => !p.On);
        if (firstFree == null)
            return PointLight.DefaultOff;

        _lights[firstFree.Index] = new PointLight(firstFree.Index, lightSource.LightPosition, lightSource.Color, lightSource.Intensity);
        _lightSourceToIndex[lightSource] = firstFree.Index;
        RefreshArrays();
        return _lights[firstFree.Index];
    }

    public void RemoveLight(ILightSource lightSource)
    {
        if (_lightSourceToIndex.TryGetValue(lightSource, out var index))
        {
            RemoveLight(index);
            _lightSourceToIndex.Remove(lightSource);
        }
    }

    public void RemoveLight(int index)
    {
        if (index < 0)
            return;

        _lights[index] = _lights[index].TurnOff();
        RefreshArrays();
    }

    private void RefreshArrays()
    {
        Positions = _lights.Select(p=>p.Position).ToArray();
        Colors = _lights.Select(p => p.Color.ToVector3()).ToArray();
        Intensities = _lights.Select(p => p.Intensity).ToArray();
    }

    /// <summary>
    /// Get filtered light arrays for a specific set of light sources
    /// </summary>
    public (Vector3[] positions, Vector3[] colors, float[] intensities, int count) GetLightsForSources(IEnumerable<ILightSource> lightSources)
    {
        var filteredLights = new List<(Vector3 pos, Vector3 color, float intensity)>();

        foreach (var lightSource in lightSources)
        {
            if (_lightSourceToIndex.TryGetValue(lightSource, out var index))
            {
                var light = _lights[index];
                if (light.On)
                {
                    filteredLights.Add((light.Position, light.Color.ToVector3(), light.Intensity));
                }
            }
        }

        // Pad to MAX_LIGHTS
        while (filteredLights.Count < MAX_LIGHTS)
        {
            filteredLights.Add((Vector3.Zero, Vector3.Zero, 0f));
        }

        return (
            filteredLights.Select(l => l.pos).ToArray(),
            filteredLights.Select(l => l.color).ToArray(),
            filteredLights.Select(l => l.intensity).ToArray(),
            filteredLights.Count(l => l.intensity > 0)
        );
    }
}
