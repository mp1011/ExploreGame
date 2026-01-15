using ExploringGame.Logics;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.TestShapes;

public class TestMover : Box, IPlaceableObject
{
  
    public TestMover()
    {
        Width = 1.0f;
        Height = 1.0f;
        Depth = 1.0f;
        MainTexture = new TextureInfo(Key: TextureKey.Floor, Color: Color.LightBlue);
    }

    public Shape Self => this;

    Shape[] IPlaceableObject.Children => TraverseAllChildren();

    
}

public class TestMoverController : IShapeController<TestMover>
{
    private float _yaw = 0;

    public TestMover Shape { get; set; }

    public void Initialize()
    {
    }

    public void Stop()
    {
    }

    public void Update(GameTime gameTime)
    {
        if (Shape.Y < 3.0f)
            Shape.Y += 0.01f;
        Shape.Z = -2.0f;
        Shape.Rotation = new Rotation(_yaw += 0.1f, 0, 0);
    }
}
