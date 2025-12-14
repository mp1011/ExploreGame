using ExploringGame.Logics;
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
    private Matrix _view;
    private Matrix _projection;
    private Vector3 _cameraPosition = new Vector3(0, 1.5f, 0);
    private float _yaw = 0f, _pitch = 0f;
    private MouseState _prevMouse;
    private Texture2D _roughGrayTexture;
    private Effect _pointLightEffect;
    private HeadBob _headBob = new HeadBob();



    struct VertexPositionColorTexture : IVertexType
    {
        public Vector3 Position;
        public Color Color;
        public Vector2 TexCoord;

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(16, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

        public VertexPositionColorTexture(Vector3 pos, Color color, Vector2 tex)
        {
            Position = pos;
            Color = color;
            TexCoord = tex;
        }
    }

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

        // Room dimensions: 16x4x8 feet (256 sq ft floor, doubled width and depth)
        float width = 16f, height = 4f, depth = 8f;
        float hw = width / 2, hh = height, hd = depth / 2;
        // Vertices for each face (floor, ceiling, 4 walls) with texture coordinates
        var verts = new VertexPositionColorTexture[24]
        {
            // Floor (dark green)
            new VertexPositionColorTexture(new Vector3(-hw, 0, -hd), Color.DarkGreen, new Vector2(0, 0)),
            new VertexPositionColorTexture(new Vector3(-hw, 0, hd), Color.DarkGreen, new Vector2(0, 1)),
            new VertexPositionColorTexture(new Vector3(hw, 0, hd), Color.DarkGreen, new Vector2(1, 1)),
            new VertexPositionColorTexture(new Vector3(hw, 0, -hd), Color.DarkGreen, new Vector2(1, 0)),
            // Ceiling (white)
            new VertexPositionColorTexture(new Vector3(-hw, hh, -hd), Color.White, new Vector2(0, 0)),
            new VertexPositionColorTexture(new Vector3(-hw, hh, hd), Color.White, new Vector2(0, 1)),
            new VertexPositionColorTexture(new Vector3(hw, hh, hd), Color.White, new Vector2(1, 1)),
            new VertexPositionColorTexture(new Vector3(hw, hh, -hd), Color.White, new Vector2(1, 0)),
            // Wall 1 (blue, -Z)
            new VertexPositionColorTexture(new Vector3(-hw, 0, -hd), Color.Blue, new Vector2(0, 1)),
            new VertexPositionColorTexture(new Vector3(hw, 0, -hd), Color.Blue, new Vector2(1, 1)),
            new VertexPositionColorTexture(new Vector3(hw, hh, -hd), Color.Blue, new Vector2(1, 0)),
            new VertexPositionColorTexture(new Vector3(-hw, hh, -hd), Color.Blue, new Vector2(0, 0)),
            // Wall 2 (blue, +X)
            new VertexPositionColorTexture(new Vector3(hw, 0, -hd), Color.Blue, new Vector2(0, 1)),
            new VertexPositionColorTexture(new Vector3(hw, 0, hd), Color.Blue, new Vector2(1, 1)),
            new VertexPositionColorTexture(new Vector3(hw, hh, hd), Color.Blue, new Vector2(1, 0)),
            new VertexPositionColorTexture(new Vector3(hw, hh, -hd), Color.Blue, new Vector2(0, 0)),
            // Wall 3 (blue, +Z)
            new VertexPositionColorTexture(new Vector3(hw, 0, hd), Color.Blue, new Vector2(1, 1)),
            new VertexPositionColorTexture(new Vector3(-hw, 0, hd), Color.Blue, new Vector2(0, 1)),
            new VertexPositionColorTexture(new Vector3(-hw, hh, hd), Color.Blue, new Vector2(0, 0)),
            new VertexPositionColorTexture(new Vector3(hw, hh, hd), Color.Blue, new Vector2(1, 0)),
            // Wall 4 (blue, -X)
            new VertexPositionColorTexture(new Vector3(-hw, 0, hd), Color.Blue, new Vector2(1, 1)),
            new VertexPositionColorTexture(new Vector3(-hw, 0, -hd), Color.Blue, new Vector2(0, 1)),
            new VertexPositionColorTexture(new Vector3(-hw, hh, -hd), Color.Blue, new Vector2(0, 0)),
            new VertexPositionColorTexture(new Vector3(-hw, hh, hd), Color.Blue, new Vector2(1, 0)),
        };
        // Indices for drawing each face as a quad (2 triangles per face), reversed winding for inside view
        var indices = new short[] {
            // Floor (0,1,2,3) - should be 0,2,1 and 0,3,2 for correct inside winding
            0,2,1, 0,3,2,
            // Ceiling (4,5,6,7) - should be 4,5,6 and 4,6,7 for correct inside winding
            4,5,6, 4,6,7,
            // Wall 1 (8,9,10,11)
            8,11,10, 8,10,9,
            // Wall 2 (12,13,14,15)
            12,15,14, 12,14,13,
            // Wall 3 (16,17,18,19)
            16,19,18, 16,18,17,
            // Wall 4 (20,21,22,23)
            20,23,22, 20,22,21
        };
        // Create vertex buffer for custom vertex type
        var vb = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColorTexture), verts.Length, BufferUsage.WriteOnly);
        vb.SetData(verts);
        _roomBuffer = vb;
        _roomIndices = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
        _roomIndices.SetData(indices);
        // Use BasicEffect with texture
        _effect = new BasicEffect(GraphicsDevice)
        {
            TextureEnabled = true,
            Texture = _roughGrayTexture,
            VertexColorEnabled = true,
            LightingEnabled = true,
            PreferPerPixelLighting = true
        };
        _effect.AmbientLightColor = new Vector3(0.08f, 0.08f, 0.08f); // Very low ambient
        _effect.DirectionalLight0.Enabled = false;


        _pointLightEffect = Content.Load<Effect>("PointLightEffect");

        // Set up point light parameters
        Vector3 lightPos = new Vector3(0, hh, 0); // Center of ceiling
        _pointLightEffect.Parameters["LightPosition"].SetValue(lightPos);
        _pointLightEffect.Parameters["LightColor"].SetValue(new Vector3(1f, 1f, 1f)); // White light
        _pointLightEffect.Parameters["LightIntensity"].SetValue(1.0f); // Adjust for brightness
        _pointLightEffect.Parameters["AmbientColor"].SetValue(new Vector3(0.08f, 0.08f, 0.08f));
        _pointLightEffect.Parameters["Texture"].SetValue(_roughGrayTexture);
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

       

        // Clamp camera position to stay inside the room (with margin)
        float margin = 0.2f; // Prevents camera from touching walls
        var width = 16f; var height = 4f; var depth = 8f;
        var hw = width / 2 - margin;
        var hd = depth / 2 - margin;
        // Clamp X and Z
        nextPosition.X = MathHelper.Clamp(nextPosition.X, -hw, hw);
        nextPosition.Z = MathHelper.Clamp(nextPosition.Z, -hd, hd);
        // Clamp Y (optional, but keep camera above floor and below ceiling)
        nextPosition.Y = MathHelper.Clamp(nextPosition.Y, 0.2f, height - 0.2f);
        _cameraPosition = nextPosition;

        // Mouse look
        var mouse = Mouse.GetState();
        if (IsActive)
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
            _prevMouse = mouse;
        }

        // Update view matrix
        var lookDir = Vector3.Transform(Vector3.Forward, Matrix.CreateFromYawPitchRoll(_yaw, _pitch, 0));
        _view = Matrix.CreateLookAt(_cameraPosition, _cameraPosition + lookDir, Vector3.Up);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _pointLightEffect.Parameters["World"].SetValue(Matrix.Identity);
        _pointLightEffect.Parameters["View"].SetValue(_view);
        _pointLightEffect.Parameters["Projection"].SetValue(_projection);

        foreach (var pass in _pointLightEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            GraphicsDevice.SetVertexBuffer(_roomBuffer);
            GraphicsDevice.Indices = _roomIndices;
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 12);
        }
        base.Draw(gameTime);
    }
}
