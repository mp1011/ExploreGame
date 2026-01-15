using Microsoft.Xna.Framework;

namespace ExploringGame.Entities;

public interface ICamera : IWithPosition
{
    Matrix CreateViewMatrix();
}
