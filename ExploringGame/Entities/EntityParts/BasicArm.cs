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

        UpperArm = AddChild(new BasicArmPart(this, puppet, parentAnchor, armRadius, upperArmLength));
        LowerArm = AddChild(new BasicArmPart(this, puppet, UpperArm, armRadius, lowerArmLength));
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return Array.Empty<Triangle>();
    }
}

public class BasicArmPart : EntityPart<Puppet>, IControllable
{
    private Shape _connectsTo;

    public BasicArmPart(BasicArm arm, Puppet entity, Shape connectsTo, float radius, float length) : base(entity)
    {
        arm.AddChild(this);
        _connectsTo = connectsTo;
        Width = radius * 2;
        Depth = radius * 2;
        Height = length;

        WorldPosition = connectsTo.WorldPosition;
        this.SetWorldSide(Side.Bottom, connectsTo.GetWorldSide(Side.Top));
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
