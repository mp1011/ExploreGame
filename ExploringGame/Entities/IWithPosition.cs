using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;

namespace ExploringGame.Entities;

public interface IWithPosition
{
    public Vector3 Position { get; set; }
    public Rotation Rotation { get; set; }
}