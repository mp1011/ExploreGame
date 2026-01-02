using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;

namespace ExploringGame.Logics;

public interface IActiveObject
{
    void Update(GameTime gameTime);

    Shape Self { get; }
    Shape[] Children { get; }
}
