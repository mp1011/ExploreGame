using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.Logics;
using ExploringGame.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System; // For random texture generation

namespace ExploringGame;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    // 3D room and camera fields
    private BasicEffect _effect;
    private VertexBuffer _roomBuffer;
    private IndexBuffer _roomIndices;
    private int _triangleCount;
    private Matrix _view;
    private Matrix _projection;
    private Vector3 _cameraPosition = new Vector3(0, 1.5f, 0);
    private float _yaw = 0f, _pitch = 0.1f;
    private MouseState _prevMouse;
    private Texture2D _roughGrayTexture;
    private Effect _pointLightEffect;
    private HeadBob _headBob = new HeadBob();
    private SpriteFont _debugFont;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
    }

    protected override void Initialize()
    {
        // Set up projection
        _projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.1f, 100f);
        base.Initialize();

        _graphics.PreferredDepthStencilFormat = DepthFormat.Depth24;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Generate a random rough gray texture
        int texSize = 64;
        Color[] texData = new Color[texSize * texSize];
        Random rand = new Random();
        for (int y = 0; y < texSize; y++)
        {
            for (int x = 0; x < texSize; x++)
            {
                int gray = 100 + rand.Next(80); // 100-179
                texData[y * texSize + x] = new Color(gray, gray, gray);
            }
        }
        _roughGrayTexture = new Texture2D(GraphicsDevice, texSize, texSize);
        _roughGrayTexture.SetData(texData);

        // Load debug font
        _debugFont = Content.Load<SpriteFont>("Font");

        SetBuffers();
        
        // Use BasicEffect with texture
        _effect = new BasicEffect(GraphicsDevice)
        {
            TextureEnabled = false,
            VertexColorEnabled = true,
            LightingEnabled = true,
            PreferPerPixelLighting = true
        };
        //  _effect.AmbientLightColor = new Vector3(0.08f, 0.08f, 0.08f); // Very low ambient
        _effect.AmbientLightColor = new Vector3(0.38f, 0.38f, 0.38f); // Very low ambient
        _effect.DirectionalLight0.Enabled = false;


        _pointLightEffect = Content.Load<Effect>("PointLightEffect");

        // Set up point light parameters
        Vector3 lightPos = new Vector3(0, 4, 0); // Center of ceiling
        _pointLightEffect.Parameters["LightPosition"].SetValue(lightPos);
        _pointLightEffect.Parameters["LightColor"].SetValue(new Vector3(1f, 1f, 1f)); // White light
        _pointLightEffect.Parameters["LightIntensity"].SetValue(1.0f); // Adjust for brightness
        _pointLightEffect.Parameters["AmbientColor"].SetValue(new Vector3(0.08f, 0.08f, 0.08f));
        _pointLightEffect.Parameters["Texture"].SetValue(_roughGrayTexture);
    }

    private void SetBuffers()
    {
        var simpleRoom = new SimpleRoom();
        simpleRoom.Width = 16f;
        simpleRoom.Height = 4f;
        simpleRoom.Depth = 8f;
        simpleRoom.Y = 2;

        simpleRoom.SideColors[Side.Top] = Color.White;
        simpleRoom.SideColors[Side.Bottom] = Color.Green;
        simpleRoom.MainColor = Color.Orange;


        var box = new Box();
        simpleRoom.AddChild(box);
        box.Width = 2f;
        box.Height = 2f;
        box.Depth = 2f;
        box.Place().OnFloor();
        box.Place().OnSide(Side.NorthEast);
        box.MainColor = Color.Blue;

        var box2 = new Box();
        simpleRoom.AddChild(box2);
        box2.Width = 1f;
        box2.Height = 3f;
        box2.Depth = 2f;
        box2.Place().OnFloor();
        box2.Place().OnSide(Side.NorthWest);
        box2.MainColor = Color.Yellow;

        var sponge = new MengerSponge(new ShapeSplitter());
        simpleRoom.AddChild(sponge);
        sponge.Size = new Vector3(3f, 3f, 3f);
        sponge.Place().OnFloor();
        sponge.MainColor = Color.Purple;

        var builder = new VertexBufferBuilder();
        var buffers = builder.Build(simpleRoom, GraphicsDevice, qualityLevel: 4);

        _roomBuffer = buffers.Item1;
        _roomIndices = buffers.Item2;
        _triangleCount = buffers.Item3;
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // Camera movement
        var k = Keyboard.GetState();
        float speed = 0.1f;
        Vector3 forward = Vector3.Transform(Vector3.Forward, Matrix.CreateFromYawPitchRoll(_yaw, 0, 0));
        Vector3 right = Vector3.Transform(Vector3.Right, Matrix.CreateFromYawPitchRoll(_yaw, 0, 0));
        Vector3 nextPosition = _cameraPosition;
        if (k.IsKeyDown(Keys.W)) nextPosition += forward * speed;
        if (k.IsKeyDown(Keys.S)) nextPosition -= forward * speed;
        if (k.IsKeyDown(Keys.A)) nextPosition -= right * speed;
        if (k.IsKeyDown(Keys.D)) nextPosition += right * speed;

        // should have a "current speed" of player and not check keys directly
        bool isMoving = k.IsKeyDown(Keys.W) || k.IsKeyDown(Keys.A) || k.IsKeyDown(Keys.S) || k.IsKeyDown(Keys.D);

        nextPosition = _headBob.Update(isMoving, gameTime, nextPosition);

       

        //// Clamp camera position to stay inside the room (with margin)
        //float margin = 0.2f; // Prevents camera from touching walls
        //var width = 16f; var height = 4f; var depth = 8f;
        //var hw = width / 2 - margin;
        //var hd = depth / 2 - margin;
        //// Clamp X and Z
        //nextPosition.X = MathHelper.Clamp(nextPosition.X, -hw, hw);
        //nextPosition.Z = MathHelper.Clamp(nextPosition.Z, -hd, hd);
        //// Clamp Y (optional, but keep camera above floor and below ceiling)
        //nextPosition.Y = MathHelper.Clamp(nextPosition.Y, 0.2f, height - 0.2f);
        _cameraPosition = nextPosition;

        // Mouse look
        var mouse = Mouse.GetState();
        if (!_firstMouse && IsActive)
        {
            var delta = mouse.Position - _prevMouse.Position;
            _yaw -= delta.X * 0.01f;
            _pitch -= delta.Y * 0.01f;
            _pitch = MathHelper.Clamp(_pitch, -MathHelper.PiOver2 + 0.1f, MathHelper.PiOver2 - 0.1f);
            Mouse.SetPosition(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2);
            _prevMouse = Mouse.GetState();
        }
        else
        {
            _firstMouse = false;
            _prevMouse = mouse;
        }

        // Update view matrix
        var lookDir = Vector3.Transform(Vector3.Forward, Matrix.CreateFromYawPitchRoll(_yaw, _pitch, 0));
        _view = Matrix.CreateLookAt(_cameraPosition, _cameraPosition + lookDir, Vector3.Up);

        base.Update(gameTime);
    }
    private bool _firstMouse = true;

    protected override void Draw(GameTime gameTime)
    {       
        GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer,Color.CornflowerBlue,1.0f,0);
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        //_effect.World = Matrix.Identity;
        //_effect.View = _view;
       // _effect.Projection = _projection;
        _pointLightEffect.Parameters["World"].SetValue(Matrix.Identity);
        _pointLightEffect.Parameters["View"].SetValue(_view);
        _pointLightEffect.Parameters["Projection"].SetValue(_projection);

        foreach (var pass in _pointLightEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            GraphicsDevice.SetVertexBuffer(_roomBuffer);
            GraphicsDevice.Indices = _roomIndices;
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _triangleCount);
        }

        // Draw debug information
        _spriteBatch.Begin();
        _spriteBatch.DrawString(_debugFont, "Position: " + _cameraPosition.ToString(), new Vector2(10, 10), Color.White);
        _spriteBatch.DrawString(_debugFont, "Yaw: " + _yaw.ToString(), new Vector2(10, 30), Color.White);
        _spriteBatch.DrawString(_debugFont, "Pitch: " + _pitch.ToString(), new Vector2(10, 50), Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
