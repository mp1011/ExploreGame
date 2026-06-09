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
                ActivationLight = _pointLights.AddLight(LocalPosition, Color.Red, 0.3f);
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

    public GateMark(Room parentRoom, Side wallSide, Vector2 centerUV, PointLights pointLights) 
        : base(parentRoom, wallSide, centerUV)
    {
        _pointLights = pointLights;
        MainTexture = new TextureInfo(Color.Red, TextureKey.Wall);
        Size = new Vector3(1.0f, 0.1f, 1.0f);
    }
}
