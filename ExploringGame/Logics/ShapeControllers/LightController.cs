using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.LevelControl;
using ExploringGame.Rendering;
using Microsoft.Xna.Framework;

namespace ExploringGame.Logics.ShapeControllers;

public class LightController : IShapeController<HighHatLight>, IOnOff
{
    private readonly PointLights _pointLights;

    public LightController(PointLights pointLights)
    {
        _pointLights = pointLights;
    }

    public HighHatLight Shape { get; set; }

    public Vector3 LightPosition => Shape.Position + new Vector3(0, -Shape.Height/2f, 0);

    private PointLight _light = PointLight.DefaultOff;

    public bool On
    {
        get => _light.On;
        set
        {
            if (value && !_light.On)
                _light = _pointLights.AddLight(LightPosition,
                    color: Color.White,
                    intensity: Shape.Intensity,
                    rangeMin: Shape.RangeMin,
                    rangeMax: Shape.RangeMax);
            else if (!value && _light.On)
            {
                _pointLights.RemoveLight(_light.Index);
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
