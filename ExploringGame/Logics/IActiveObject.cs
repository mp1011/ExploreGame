using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;
namespace ExploringGame.Logics;


public interface IPlaceableObject
{
    Shape Self { get; }
    Shape[] Children { get; }
}

public interface IActiveObject : IPlaceableObject
{
    void Update(GameTime gameTime);

}
