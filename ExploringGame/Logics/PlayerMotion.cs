using ExploringGame.Entities;
using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ExploringGame.Logics;

internal class PlayerMotion
{
    public const float WalkAccel = 0.006f;
    public const float StopAccel = 0.009f;

    private PlayerInput _playerInput;
    private Player _player;
    private HeadBob _headBob;
    private MouseState _prevMouse;
    private bool _firstMouse = true;

    public PlayerMotion(Player player, HeadBob headBob, PlayerInput playerInput)
    {
        _playerInput = playerInput;
        _player = player;
        _headBob = headBob;
    }

    public void Update(GameTime gameTime, GameWindow window)
    {
        var cameraPosition = _player.Position;
        var yaw = _player.Rotation.Yaw;
        var pitch = _player.Rotation.Pitch;

        _player.Motion.TargetMotion = GetMotionTarget(yaw);

        if (_player.Motion.TargetMotion.LengthSquared() > 0)
            _player.Motion.Acceleration = WalkAccel;
        else
            _player.Motion.Acceleration = StopAccel;

            _player.Motion.Update();

        Vector3 nextPosition = cameraPosition + _player.Motion.CurrentMotion;

        bool isMoving = _player.Motion.CurrentMotion.LengthSquared() > 0;

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

    public Vector3 GetMotionTarget(float yaw)
    {
        float speed = _playerInput.IsKeyDown(GameKey.Run) ? 0.2f : 0.1f;
        Vector3 forward = Vector3.Transform(Vector3.Forward, Matrix.CreateFromYawPitchRoll(yaw, 0, 0));
        Vector3 right = Vector3.Transform(Vector3.Right, Matrix.CreateFromYawPitchRoll(yaw, 0, 0));
        Vector3 target = Vector3.Zero;

        if (_playerInput.IsKeyDown(GameKey.Forward)) target += forward * speed;
        if (_playerInput.IsKeyDown(GameKey.Backward)) target -= forward * speed;
        if (_playerInput.IsKeyDown(GameKey.StrafeLeft)) target -= right * speed;
        if (_playerInput.IsKeyDown(GameKey.StrafeRight)) target += right * speed;

        return target;
    }
}
