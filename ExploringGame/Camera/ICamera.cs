using ExploringGame.Entities;
using Microsoft.Xna.Framework;

namespace ExploringGame.Camera;

public interface ICamera : IWithPosition
{
    Matrix CreateViewMatrix();
}
