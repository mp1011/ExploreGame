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

public interface IControllable<TController> : IControllable
    where TController : IActiveObject
{
    TController Controller { get; }
}

public interface IActiveObject
{
    void Initialize();
    void Update(GameTime gameTime);
}


public interface IShapeController<T> : IActiveObject
    where T : Shape
{
    T Shape { get; }
}