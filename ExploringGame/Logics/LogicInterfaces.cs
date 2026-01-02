using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;
namespace ExploringGame.Logics;


public interface IPlaceableObject
{
    Shape Self { get; }
    Shape[] Children { get; }
}

public interface IControllable
{
    IActiveObject CreateController(ServiceContainer serviceContainer);
}

public interface IActiveObject
{
    void Update(GameTime gameTime);
}


public interface IShapeController<T> : IActiveObject
    where T : Shape
{
    T Shape { get; }
}