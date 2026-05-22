using ExploringGame.Logics;
using ExploringGame.Logics.Collision;
using ExploringGame.Rendering;
using ExploringGame.Services;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes;

public abstract class PlaceableShape : Shape, IPlaceableObject, ICollidable
{
    private Vector3 _savedPosition;
    private Rotation _savedRotation;

    public override ShapeBufferType ShapeBufferType => ShapeBufferType.ActiveObject;

    public Shape Self => this;

    Shape[] IPlaceableObject.Children => TraverseAllChildren();

    public IRoom Room { 
        get; 
        set; 
    }

    public abstract CollisionGroup CollisionGroup { get; }
    public abstract CollisionGroup CollidesWithGroups { get; }

    protected override void BeforeBuild()
    {
        _savedPosition = Position;
        _savedRotation = Rotation;
        Position = Vector3.Zero;
        Rotation = null;
    }

    protected override void AfterBuild()
    {
        Position = _savedPosition;
        Rotation = _savedRotation;
    }
}
