using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Logics.ShapeControllers;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ExploringGame.GeometryBuilder.Shapes.Furniture;

public class Lamp : PlaceableShape, IControllable<LightSwitchController<Lamp>>, ISwitchShape
{
    public override CollisionGroup CollisionGroup => CollisionGroup.Environment;

    public override CollisionGroup CollidesWithGroups => CollisionGroup.MovingObjects;

    public override IColliderMaker ColliderMaker => ColliderMakers.BoundingBox(this);

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public LightBulb Bulb { get; }

    public Lamp(Room room, StateKey stateKey)
    {
        StateKey = stateKey;
        Width = Measure.Inches(10);
        Height = Measure.Inches(20);
        Depth = Measure.Inches(10);

        Bulb = new LightBulb(room, this, stateKey);
        Bulb.Place().AtParent().OnSideOuter(Side.Top);

        ControlledObjects.Add(Bulb);
    }

    public override Theme Theme => new BasicFurnitureTheme(Color.Turquoise);

    public LightSwitchController<Lamp> Controller { get; private set; }

    public List<IOnOff> ControlledObjects { get; } = new List<IOnOff>();

    public StateKey StateKey { get; }

    public bool On
    {
        get => Controller.On;
        set => Controller.On = value;

    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return TriangleMaker.BuildEllipsoid(this, 16);
    }

    public IActiveObject CreateController(ServiceContainer serviceContainer)
    {
        var controller = serviceContainer.Get<LightSwitchController<Lamp>>();
        controller.Shape = this;
        Controller = controller;
        return controller;
    }
}
