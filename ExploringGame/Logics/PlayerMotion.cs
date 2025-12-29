using ExploringGame.Entities;
using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ExploringGame.Logics;

internal class PlayerMotion
{
    private Player _player;
    private HeadBob _headBob;
    private MouseState _prevMouse;
    private bool _firstMouse = true;

    public PlayerMotion(Player player, HeadBob headBob)
    {
        _player = player;
        _headBob = headBob;
    }

    public void Update(GameTime gameTime, GameWindow window)
    {
        var cameraPosition = _player.Position;
        var yaw = _player.Rotation.Yaw;
        var pitch = _player.Rotation.Pitch;

        // Camera movement
        var k = Keyboard.GetState();
        float speed = 0.1f;
        Vector3 forward = Vector3.Transform(Vector3.Forward, Matrix.CreateFromYawPitchRoll(yaw, 0, 0));
        Vector3 right = Vector3.Transform(Vector3.Right, Matrix.CreateFromYawPitchRoll(yaw, 0, 0));
        Vector3 nextPosition = cameraPosition;
        if (k.IsKeyDown(Keys.W)) nextPosition += forward * speed;
        if (k.IsKeyDown(Keys.S)) nextPosition -= forward * speed;
        if (k.IsKeyDown(Keys.A)) nextPosition -= right * speed;
        if (k.IsKeyDown(Keys.D)) nextPosition += right * speed;

        // should have a "current speed" of player and not check keys directly
        bool isMoving = k.IsKeyDown(Keys.W) || k.IsKeyDown(Keys.A) || k.IsKeyDown(Keys.S) || k.IsKeyDown(Keys.D);

        nextPosition = _headBob.Update(isMoving, gameTime, nextPosition);

        _player.Position = nextPosition;

        // Mouse look
        var mouse = Mouse.GetState();
        if (!_firstMouse)
        {
            var delta = mouse.Position - _prevMouse.Position;
            yaw -= delta.X * 0.01f;
            pitch -= delta.Y * 0.01f;
            pitch = MathHelper.Clamp(pitch, -MathHelper.PiOver2 + 0.1f, MathHelper.PiOver2 - 0.1f);
            Mouse.SetPosition(window.ClientBounds.Width / 2, window.ClientBounds.Height / 2);
            _prevMouse = Mouse.GetState();
        }
        else
        {
            _firstMouse = false;
            _prevMouse = mouse;
        }

        _player.Rotation = new Rotation(yaw, pitch, _player.Rotation.Roll);
    }

}
