using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ExploringGame.Logics.ShapeControllers;

public class LightController : IShapeController<HighHatLight>, IOnOff
{
    private readonly PointLights _pointLights;

    public LightController(PointLights pointLights)
    {
        _pointLights = pointLights;
    }

    public HighHatLight Shape { get; set; }

    public Vector3 LightPosition => Shape.Position + new Vector3(0, -Shape.Height, 0);

    private int? _lightIndex;
    public bool On
    {
        get => _lightIndex.HasValue;
        set
        {
            if (value && !_lightIndex.HasValue)
                _lightIndex = _pointLights.AddLight(LightPosition);
            else if (!value && _lightIndex.HasValue)
            {
                _pointLights.RemoveLight(_lightIndex.Value);
                _lightIndex = null;
            }
        }
    }

    public void Initialize()
    {
    }

    public void Update(GameTime gameTime)
    {
        if(!On && Keyboard.GetState().IsKeyDown(Keys.L))
        {
            On = true;
        }
        if (On && Keyboard.GetState().IsKeyDown(Keys.O))
        {
            On = false;
        }
    }
}
