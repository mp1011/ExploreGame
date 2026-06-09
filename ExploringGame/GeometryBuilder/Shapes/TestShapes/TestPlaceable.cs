using ExploringGame.GeometryBuilder.Shapes.SimpleShapes;
using ExploringGame.Logics;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.TestShapes;

public class TestPlaceable : Box, IPlaceableObject
{
    private Vector3 _savedPosition;
    private Rotation _savedRotation;

    public Shape Self => this;

    public IRoom Room { get; set; }
    
    Shape[] IPlaceableObject.Children => TraverseAllChildren();

    public TestPlaceable(Room room, float size, Color color) : base(new Theme(color))
    {
        Room = room;
        Size = new Vector3(size, size, size);
    }

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
