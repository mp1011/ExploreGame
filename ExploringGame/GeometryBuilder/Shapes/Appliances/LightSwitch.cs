using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Logics.Collision;
using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Logics.ShapeControllers;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ExploringGame.GeometryBuilder.Shapes.Appliances;

public class LightSwitch : Shape, IControllable<LightSwitchController>, ICollidable, ISwitchShape
{
    public override Theme Theme => new Theme(Color.Red);
    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public override IColliderMaker ColliderMaker => new BoundingBoxColliderMaker(this);

    public StateKey StateKey { get; }

    public bool On
    {
        get => Controller.On;
        set => Controller.On = value;
    }

    public List<IOnOff> ControlledObjects { get; } = new List<IOnOff>();
    public LightSwitch(Room room, Side wallSide, StateKey key)
    {
        room.AddChild(this);

        Height = Measure.Inches(5);

        if(wallSide.GetAxis() == Axis.X)
        {
            Width = Measure.Inches(0.5f);
            Depth = Measure.Inches(5f);
        }
        else
        {
            Width = Measure.Inches(5f);
            Depth = Measure.Inches(0.5f);            
        }
       

        Position = room.Position;
        Y += Measure.Inches(20);
        StateKey = key;
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid();
    }

    public LightSwitchController Controller { get; private set; }

    public CollisionGroup CollisionGroup => CollisionGroup.Environment;

    public CollisionGroup CollidesWithGroups => CollisionGroup.None;

    public IActiveObject CreateController(ServiceContainer serviceContainer)
    {
        var controller = serviceContainer.Get<LightSwitchController>();
        controller.Shape = this;
        Controller = controller;
        return controller;
    }
}
