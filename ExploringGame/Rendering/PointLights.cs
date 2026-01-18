using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace ExploringGame.Rendering;

public record PointLight(int Index, Vector3 Position, Color Color, float Intensity)
{
    public bool On => Intensity > 0f;

    public PointLight TurnOff() => new PointLight(Index, Vector3.Zero, Color.White, 0f);

    public static PointLight DefaultOff => new PointLight(-1, Vector3.Zero, Color.White, 0f);
}

public class PointLights
{
    public const int MAX_LIGHTS = 10;

    private PointLight[] _lights;

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

    public PointLight AddLight(Vector3 position, Color? color = null, float intensity = 1.0f)
    {
        color = color ?? Color.White;
        var existing = _lights.FirstOrDefault(p => p.Position == position && p.Color == color && p.Intensity == intensity);

        if (existing != null)
            return existing ?? PointLight.DefaultOff;

        var firstFree = _lights.FirstOrDefault(p => !p.On);
        if (firstFree == null)
            return PointLight.DefaultOff;

        _lights[firstFree.Index] = new PointLight(firstFree.Index, position, color.Value, intensity);
        RefreshArrays();
        return _lights[firstFree.Index];
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
}
