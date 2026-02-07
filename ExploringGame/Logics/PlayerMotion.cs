using ExploringGame.Entities;
using ExploringGame.GameDebug;
using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;

namespace ExploringGame.Logics;

internal class PlayerMotion
{
    public const float RunSpeed = 12.0f;
    public const float WalkSpeed = 6.0f;
    public const float WalkAccel = 0.4f;
    public const float StopAccel = 1.0f;
    public const float JumpSpeed = -5.00f;

    public static float Gravity => Debug.FlyMode ? 0f : 10.0f;
    public const float GravityAccel = 0.4f;

    private IPlayerInput _playerInput;
    private Player _player;
    private EntityMover _playerMotion;

    public PlayerMotion(Player player, IPlayerInput playerInput, EntityMover playerMotion)
    {
        _playerInput = playerInput;
        _player = player;
        _playerMotion = playerMotion;
    }

    public void Update(GameTime gameTime, GameWindow window)
    {
        var cameraPosition = _player.Position;
        var yaw = _player.Rotation.Yaw;
        var pitch = _player.Rotation.Pitch;

        _playerMotion.Motion.TargetMotion = GetMotionTarget(yaw);
        _playerMotion.Motion.TargetY = Gravity;
        _playerMotion.Motion.Gravity = GravityAccel;

        if (_playerMotion.Motion.TargetMotion.LengthSquared() > 0)
            _playerMotion.Motion.Acceleration = WalkAccel;
        else
            _playerMotion.Motion.Acceleration = StopAccel;

        if (Debug.FlyMode)
        {
            if (_playerInput.IsKeyDown(GameKey.Jump))           
                _playerMotion.Motion.CurrentY = JumpSpeed;            
            else if (_playerInput.IsKeyDown(GameKey.Crouch))
                _playerMotion.Motion.CurrentY = -JumpSpeed;
            else
                _playerMotion.Motion.CurrentY = 0f;
        }
        else
        {
            //todo, check if on floor
            if (_playerInput.IsKeyPressed(GameKey.Jump) && _playerMotion.Motion.CurrentY == 0)
                _playerMotion.Motion.CurrentY = JumpSpeed;
        }

        // Mouse look
        var mouseDelta = _playerInput.GetMouseDelta();
        yaw -= mouseDelta.X * 0.01f;
        pitch -= mouseDelta.Y * 0.01f;
        pitch = MathHelper.Clamp(pitch, -MathHelper.PiOver2 + 0.1f, MathHelper.PiOver2 - 0.1f);
        _playerInput.CenterMouse(window);

        _player.Rotation = new Rotation(yaw, pitch, _player.Rotation.Roll);
    }

    public Vector3 GetMotionTarget(float yaw)
    {
        float speed = _playerInput.IsKeyDown(GameKey.Run) ? RunSpeed : WalkSpeed;
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
