using ExploringGame.GeometryBuilder;
using ExploringGame.LevelControl;
using ExploringGame.Rendering;
using Microsoft.Xna.Framework;

namespace ExploringGame.Logics.ShapeControllers;

public class LightController<T> : IShapeController<T>, IOnOff
    where T:Shape, ILightSource
{
    private readonly PointLights _pointLights;

    public LightController(PointLights pointLights)
    {
        _pointLights = pointLights;
    }

    public T Shape { get; set; }

    public Vector3 LightPosition => Shape.Position + new Vector3(0, -Shape.Height/2f, 0);

    private PointLight _light = PointLight.DefaultOff;

    public bool On
    {
        get => _light.On;
        set
        {
            if (value && !_light.On)
            {
                // Register light using the ILightSource interface
                _light = _pointLights.AddLight(Shape);
            }
            else if (!value && _light.On)
            {
                _pointLights.RemoveLight(Shape);
                _light = _light.TurnOff();
            }
        }
    }

    public StateKey StateKey => StateKey.None;

    public void Initialize()
    {
    }

    public void Stop()
    {
    }

    public void Update(GameTime gameTime)
    {       
    }
}
