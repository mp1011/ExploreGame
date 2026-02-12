using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.Decals;
using ExploringGame.Rendering;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.Entities;

/// <summary>
/// A visual marker on a wall indicating where the Light Spirit may break into the world
/// </summary>
public class GateMark : WallDecal
{
    private bool _isActive;
    private PointLights _pointLights;

    public bool IsActive
    {
        get => _isActive;
        set
        {
            if(value && !_isActive)
            {
                _isActive = true;

                // Add a faint red point light at the gatemark location
                // Range covers a small area around the gatemark
                var rangeMin = Position - new Vector3(2f, 2f, 2f);
                var rangeMax = Position + new Vector3(2f, 2f, 2f);
                ActivationLight = _pointLights.AddLight(Position, Color.Red, 0.3f, rangeMin, rangeMax);
            }
            else if(!value && _isActive)
            {
                _isActive = false;
                
                if (ActivationLight != null)
                {
                    _pointLights.RemoveLight(ActivationLight.Index);
                    ActivationLight = null;
                }
            }
        }
    }

    public PointLight ActivationLight { get; private set; }

    public GateMark(Room parentRoom, Side wallSide, Placement2D placement, PointLights pointLights) 
        : base(parentRoom, wallSide, placement)
    {
        _pointLights = pointLights;
        MainTexture = new TextureInfo(Color.Red, TextureKey.Wall);
    }
}
