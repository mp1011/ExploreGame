using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Logics.Controllers;
using ExploringGame.Services;

namespace ExploringGame.Entities.EntityParts;

public class Tentacle : EntityPart<Puppet>, IControllable
{
    private Shape _connectsTo;

    public Tentacle(Puppet entity, Shape connectsTo, float radius, float length) : base(entity)
    {
        _connectsTo = connectsTo;
        Width = radius * 2;
        Depth = radius * 2;
        Height = length;

        Position = connectsTo.Position;
        SetSide(Side.Bottom, connectsTo.GetSide(Side.Top));
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return TriangleMaker.BuildEllipsoid(this, 16);
    }


    public static void GenerateTentacleArm(WorldSegment worldSegment, Puppet entity, int sections)
    {
        //clean this up
        var radius = 0.2f;
        var length = 0.5f;

        Shape baseShape = entity;

        while(sections-- > 0)
        {
            var nextSection = worldSegment.AddChild(new Tentacle(entity, baseShape, radius, length));
            baseShape = nextSection;
        }
    }

    public IActiveObject CreateController(ServiceContainer serviceContainer)
    {
        var controller = serviceContainer.Get<TentacleController>();
        controller.Tentacle = this;
        controller.ConnectsTo = _connectsTo;
        return controller;
    }
}
