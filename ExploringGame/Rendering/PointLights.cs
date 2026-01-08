using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Rendering;

record PointLight(Vector3 Position, Color Color, float Intensity);

public class PointLights
{
    public const int MAX_LIGHTS = 10;

    private List<PointLight> _lights = new List<PointLight>();

    public Vector3[] Positions { get; private set; }
    public Vector3[] Colors { get; private set; }
    public float[] Intensities { get; private set; }


    public PointLights()
    {
        RefreshArrays();
    }

    public int? AddLight(Vector3 position, Color? color = null, float intensity = 1.0f)
    {
        if (_lights.Count >= MAX_LIGHTS)
            return null;

        color ??= Color.White;
        _lights.Add(new PointLight(position, color.Value, intensity));
        RefreshArrays();
        return _lights.Count - 1;
    }

    public void RemoveLight(int index)
    {
        _lights.RemoveAt(index);
        RefreshArrays();
    }

    private void RefreshArrays()
    {
        Positions = _lights.Select(p=>p.Position).ToArray();
        Colors = _lights.Select(p => p.Color.ToVector3()).ToArray();
        Intensities = _lights.Select(p => p.Intensity).ToArray();
    }
}
