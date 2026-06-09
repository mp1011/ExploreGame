using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using Microsoft.Xna.Framework;
namespace ExploringGame.Logics;

public interface IWithRoom
{
    IRoom Room { get; set; }
}

// name is misleading as this interface does not expose placeable behaviors
public interface IPlaceableObject : IWithRoom
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
    void Stop();
}


public interface IShapeController<T> : IActiveObject
    where T : IShape
{
    T Shape { get; }
}