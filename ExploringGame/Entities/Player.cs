using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;

namespace ExploringGame.Entities;

class Player : IWithPosition
{
    public Vector3 Position { get; set; } = new Vector3(0, 1.5f, 0);
    public Rotation Rotation { get; set; } = new Rotation(0, 0.1f, 0);
    
    public Player()
    {
    }

    public Matrix CreateViewMatrix()
    {
        var lookDir = Vector3.Transform(Vector3.Forward, Matrix.CreateFromYawPitchRoll(Rotation.Yaw, Rotation.Pitch, 0));
        return Matrix.CreateLookAt(Position, Position + lookDir, Vector3.Up);
    }
}
