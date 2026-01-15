using ExploringGame.Entities;
using Microsoft.Xna.Framework;

namespace ExploringGame.Services;

public class CameraService
{
    private ICamera _current;
    
    public Matrix View { get; private set; }
    public Matrix Projection { get; private set; }

    public CameraService(Player player, Game game)
    {
        SetCamera(player);

        // standard projection, for now
        Projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.ToRadians(70f), 
            game.GraphicsDevice.Viewport.AspectRatio,
            0.1f, 100f);
    }

    public void Update()
    {
        View = _current.CreateViewMatrix();
    }

    public void SetCamera(ICamera camera)
    {
        _current = camera;
        View = camera.CreateViewMatrix();
    }
}
