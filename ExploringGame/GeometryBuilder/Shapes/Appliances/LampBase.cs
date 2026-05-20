using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Logics.ShapeControllers;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ExploringGame.GeometryBuilder.Shapes.Appliances;

public abstract class LampBase : PlaceableShape, IControllable<LightSwitchController>, ISwitchShape
{
    public override CollisionGroup CollisionGroup => CollisionGroup.Environment;

    public override CollisionGroup CollidesWithGroups => CollisionGroup.MovingObjects;

    public override IColliderMaker ColliderMaker => ColliderMakers.BoundingBox(this);

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public LightBulb Bulb { get; }

    public LampBase(Room room, StateKey stateKey, float width, float depth, float height)
    {
        Room = room;
        room.AddChild(this);

        StateKey = stateKey;
        Width = width;
        Height = height;
        Depth = depth;

        Bulb = CreateBulb(room, StateKey);
        ControlledObjects.Add(Bulb);
    }

    protected abstract LightBulb CreateBulb(Room room, StateKey stateKey);

    public override Theme Theme => new BasicFurnitureTheme(Color.Turquoise);

    public LightSwitchController Controller { get; private set; }

    public List<IOnOff> ControlledObjects { get; } = new List<IOnOff>();

    public StateKey StateKey { get; }

    public bool On
    {
        get => Controller.On;
        set => Controller.On = value;

    }
    public IActiveObject CreateController(ServiceContainer serviceContainer)
    {
        var controller = serviceContainer.Get<LightSwitchController>();
        controller.Shape = this;
        Controller = controller;
        return controller;
    }
}
