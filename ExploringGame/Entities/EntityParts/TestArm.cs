using ExploringGame.GeometryBuilder;
using ExploringGame.Logics;
using ExploringGame.Logics.Controllers;
using Microsoft.Xna.Framework;

namespace ExploringGame.Entities.EntityParts;

public class TestArm : EntityPart<Puppet>, IControllable
{
  
    public TestArm(Puppet puppet) : base(puppet)
    {
        Size = new Vector3(0.5f, 2.0f, 0.5f);
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid();
    }

    public IActiveObject CreateController(ServiceContainer serviceContainer)
    {
        var controller = serviceContainer.Get<PuppetPartController>();
        controller.PuppetPart = this;
        return controller;
    }
}