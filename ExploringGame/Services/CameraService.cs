using ExploringGame.Entities;
using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;
using System;

namespace ExploringGame.Services;

public class CameraService
{
    private ICamera _current;
    private readonly float FarPlaneDistance = Measure.Feet(100000);

    public Matrix View { get; private set; }
    public Matrix SkyboxView { get; private set; }
    public Matrix Projection { get; private set; }


    public CameraService(Player player, Game game)
    {
        SetCamera(player);

        // standard projection, for now
        Projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.ToRadians(70f), 
            game.GraphicsDevice.Viewport.AspectRatio,
            0.1f, 300f);
    }

    public void Update()
    {
        View = _current.CreateViewMatrix();
        SkyboxView = CreateRotationOnlyView(View);
    }

    public void SetCamera(ICamera camera)
    {
        _current = camera;
        View = camera.CreateViewMatrix();
        SkyboxView = CreateRotationOnlyView(View);
    }

    private Matrix CreateRotationOnlyView(Matrix view)
    {
        var rotationOnlyView = view;
        rotationOnlyView.M41 = 0;
        rotationOnlyView.M42 = 0;
        rotationOnlyView.M43 = 0;
        return rotationOnlyView;
    }
}
