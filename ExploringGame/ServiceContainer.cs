using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame;

public class ServiceContainer
{
    private IKernel _kernel;
    public ServiceContainer()
    {
        _kernel = new StandardKernel();        
    }

    public void Bind<T>(T service)
    {
        _kernel.Bind<T>().ToConstant(service);
    }

    public void BindTransient<T>()
    {
        _kernel.Bind<T>().To<T>();
    }

    public void BindSingleton<T>()
    {
        _kernel.Bind<T>().To<T>().InSingletonScope();
    }

    public void BindSingleton(object o, Type t)
    {
        _kernel.Bind(t).ToConstant(o);
    }

    public IActiveObject[] CreateControllers<T>(IEnumerable<T> objects)
    {
        return objects.OfType<IControllable>().Select(p => p.CreateController(this)).ToArray();          
    }

    public T Get<T>()
    {
        return _kernel.Get<T>();
    }

    public object Get(Type t)
    {
        return _kernel.Get(t);
    }
}
