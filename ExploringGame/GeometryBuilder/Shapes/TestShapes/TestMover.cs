using ExploringGame.Logics;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.TestShapes;

public class TestMover : Box, IActiveObject
{
    private float _yaw = 0;

    public TestMover()
    {
        Width = 1.0f;
        Height = 1.0f;
        Depth = 1.0f;
        MainTexture = new TextureInfo(Key: TextureKey.Floor, Color: Color.LightBlue);
    }

    public Shape Self => this;

    Shape[] IActiveObject.Children => TraverseAllChildren();

    public void Update(GameTime gameTime)
    {
        if(Y < 3.0f)
            Y += 0.01f;
        Z = -2.0f;
        Rotation = new Rotation(_yaw += 0.1f, 0, 0);
    }
}
