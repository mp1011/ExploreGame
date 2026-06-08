using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Logics.Controllers;
using ExploringGame.Services;
using System;

namespace ExploringGame.Entities.EntityParts;


public class BasicArm : Shape
{
    public override ViewFrom ViewFrom => ViewFrom.None;

    public BasicArmPart UpperArm { get; }
    public BasicArmPart LowerArm { get; }

    public BasicArm(WorldSegment worldSegment, Puppet puppet, Shape parentAnchor, float armRadius, float upperArmLength, float lowerArmLength)
    {
        puppet.AddChild(this);

        UpperArm = worldSegment.AddChild(new BasicArmPart(puppet, parentAnchor, armRadius, upperArmLength));
        LowerArm = worldSegment.AddChild(new BasicArmPart(puppet, UpperArm, armRadius, lowerArmLength));
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return Array.Empty<Triangle>();
    }
}

public class BasicArmPart : EntityPart<Puppet>, IControllable
{
    private Shape _connectsTo;

    public BasicArmPart(Puppet entity, Shape connectsTo, float radius, float length) : base(entity)
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


    public IActiveObject CreateController(ServiceContainer serviceContainer)
    {
        var controller = serviceContainer.Get<PuppetArmController>();
        controller.ArmPart = this;
        controller.ConnectsTo = _connectsTo;
        return controller;
    }
}
