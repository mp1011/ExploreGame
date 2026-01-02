using ExploringGame.Logics;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes;

public abstract class PlaceableShape : Shape, IPlaceableObject
{
    private Vector3 _savedPosition;
    private Rotation _savedRotation;

    public Shape Self => this;

    Shape[] IPlaceableObject.Children => TraverseAllChildren();

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
