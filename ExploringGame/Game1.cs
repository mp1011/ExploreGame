using ExploringGame.Entities;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.Rooms;
using ExploringGame.GeometryBuilder.Shapes.TestShapes;
using ExploringGame.Logics;
using ExploringGame.Logics.Collision;
using ExploringGame.Motion;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System; // For random texture generation

namespace ExploringGame;

public class Game1 : Game
{
    private Player _player;
    private PlayerMotion _playerMotion;
    private EntityMover _playerGroundMover, _playerGravityMover;
    private HeadBob _headBob;
    private EntityCollider _playerCollider;
    private PlayerInput _playerInput;
    private GravityController _gravityController;

    private WorldSegment _mainShape;

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    // 3D room and camera fields
    private BasicEffect _effect;
    private VertexBuffer _roomBuffer;
    private IndexBuffer _roomIndices;
    private int _triangleCount;
    private Matrix _view;
    private Matrix _projection;
    private TextureSheet _basementTextures;
    private Effect _pointLightEffect;
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
        _projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.ToRadians(70f), //MathHelper.PiOver4, 
            GraphicsDevice.Viewport.AspectRatio, 
            0.1f, 100f);

        _playerInput = new PlayerInput();
        _headBob = new HeadBob();
        _player = new Player();

        _playerGroundMover = new EntityMover(new AcceleratedMotion(), _player);
        _playerGravityMover = new EntityMover(new AcceleratedMotion(), _player);

        _mainShape = CreateMainShape();
        _playerCollider = new EntityCollider { Entity = _player, CurrentWorldSegment = _mainShape };
        _gravityController = new GravityController(_player, _playerCollider, _playerGravityMover);

        _playerMotion = new PlayerMotion(_player, _headBob, _playerInput, _playerGroundMover, _gravityController);

        base.Initialize();

        _graphics.PreferredDepthStencilFormat = DepthFormat.Depth24;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _basementTextures = new TextureSheet(Content.Load<Texture2D>("basement"))
            .Add(TextureKey.Floor, left: 1753, top: 886, right: 2866, bottom: 1640)
            .Add(TextureKey.Wall, left: 2975, top: 808, right: 4483, bottom: 2806)
            .Add(TextureKey.Ceiling, left: 214, top: 24, right: 1523, bottom: 2008)
            .Add(TextureKey.Wood, left: 1995, top: 80, right: 3625, bottom: 669)
            .Add(TextureKey.None, left: 912, top: 2221, right: 922, bottom: 2231);

        // Load debug font
        _debugFont = Content.Load<SpriteFont>("Font");

        SetBuffers();
        
        // Use BasicEffect with texture
        _effect = new BasicEffect(GraphicsDevice)
        {
            TextureEnabled = true,
            VertexColorEnabled = true,
            LightingEnabled = true,
            PreferPerPixelLighting = true
        };
        _effect.AmbientLightColor = new Vector3(0.38f, 0.38f, 0.38f); // Very low ambient
        _effect.DirectionalLight0.Enabled = false;
        _effect.Texture = _basementTextures.Texture;


        _pointLightEffect = Content.Load<Effect>("PointLightEffect");

        // Set up point light parameters
        Vector3 lightPos = new Vector3(0, 3, 0); // Center of ceiling
        _pointLightEffect.Parameters["LightPosition"].SetValue(lightPos);
        _pointLightEffect.Parameters["LightColor"].SetValue(new Vector3(1f, 1f, 1f)); // White light
        _pointLightEffect.Parameters["LightIntensity"].SetValue(1.0f); // Adjust for brightness
        _pointLightEffect.Parameters["AmbientColor"].SetValue(new Vector3(0.08f, 0.08f, 0.08f));
        _pointLightEffect.Parameters["Texture"].SetValue(_basementTextures.Texture);
    }

    private WorldSegment CreateMainShape()
    {
        return BasementOffice();
    }

    private WorldSegment BasementOffice()
    {
        var ws = new WorldSegment();
        var office = new BasementOffice(ws);

        return ws;
    }

    private WorldSegment FurnitureRotateTest()
    {
        var simpleRoom = new SimpleRoom();
        simpleRoom.Width = 16f;
        simpleRoom.Height = 4f;
        simpleRoom.Depth = 8f;
        simpleRoom.Y = 2;

        simpleRoom.SideTextures[Side.Top] = new TextureInfo(TextureKey.Ceiling);
        simpleRoom.SideTextures[Side.Bottom] = new TextureInfo(TextureKey.Floor);
        simpleRoom.MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);

        var officeDesk = new OfficeDesk(simpleRoom);
        officeDesk.Place().OnFloor();
        officeDesk.Place().OnSideInner(Side.North);
        officeDesk.Z += 0.1f;

        var officeDesk2 = new OfficeDesk(simpleRoom);
        officeDesk2.Position = simpleRoom.Position;
        officeDesk2.Place().OnFloor();
        officeDesk2.X += 3.0f;

        officeDesk2.Rotation = new Rotation(0.5f, 0.2f, 0f);

        var ws = new WorldSegment();
        ws.AddChild(simpleRoom);
        return ws;
    }

    private Shape SingleFaceTest()
    {
        var faceTest = new SingleFaceTest(Side.North);
        faceTest.Y = 2.0f;
        faceTest.Z = -1.0f;
        return faceTest;
    }

    private WorldSegment EmptyRoom()
    {
        var simpleRoom = new SimpleRoom();
        simpleRoom.Width = 40f;
        simpleRoom.Height = 4f;
        simpleRoom.Depth = 40f;
        simpleRoom.Y = 2;

        simpleRoom.SideTextures[Side.Top] = new TextureInfo(TextureKey.Ceiling);
        simpleRoom.SideTextures[Side.Bottom] = new TextureInfo(Key: TextureKey.Floor, Style: TextureStyle.XZTile, TileSize: 50.0f);
        simpleRoom.MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);

        var world = new WorldSegment();
        world.AddChild(simpleRoom);

        return world;
    }

    private WorldSegment ConnectingRoomsTest()
    {
        var floorTexture = new TextureInfo(Key: TextureKey.Floor, Style: TextureStyle.XZTile, TileSize: 50.0f);

        var room = new Room();
        room.Width = 16f;
        room.Height = 4f;
        room.Depth = 8f;
        room.Y = 2;

        room.SideTextures[Side.Top] = new TextureInfo(TextureKey.Ceiling);
        room.SideTextures[Side.Bottom] = floorTexture;
        room.MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);

        var southRoom = new Room();
        southRoom.Width = 4f;
        southRoom.Height = 4f;
        southRoom.Depth = 10f;
        southRoom.SideTextures[Side.Top] = new TextureInfo(TextureKey.Ceiling);
        southRoom.SideTextures[Side.Bottom] = floorTexture;
        southRoom.MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);
        room.AddConnectingRoom(new RoomConnection(southRoom, Side.South, 0.2f));

        var northRoom = new Room();
        northRoom.Width = 4f;
        northRoom.Height = 4f;
        northRoom.Depth = 10f;
        northRoom.SideTextures[Side.Top] = new TextureInfo(TextureKey.Ceiling);
        northRoom.SideTextures[Side.Bottom] = floorTexture;
        northRoom.MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);

        room.AddConnectingRoom(new RoomConnection(northRoom, Side.North, 0.2f));
        northRoom.SetSideUnanchored(Side.Bottom, northRoom.GetSide(Side.Bottom) + 0.4f);

        var northRoom2 = new Room();
        northRoom2.Width = 40f;
        northRoom2.Height = 4f;
        northRoom2.Depth = 40f;
        northRoom2.SideTextures[Side.Top] = new TextureInfo(TextureKey.Ceiling);
        northRoom2.SideTextures[Side.Bottom] = floorTexture;
        northRoom2.MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);
        northRoom.AddConnectingRoom(new RoomConnection(northRoom2, Side.North, 0.5f));

        var westRoom = new Room();
        westRoom.Width = 10f;
        westRoom.Height = 4f;
        westRoom.Depth = 4f;
        westRoom.SideTextures[Side.Top] = new TextureInfo(TextureKey.Ceiling);
        westRoom.SideTextures[Side.Bottom] = floorTexture;
        westRoom.MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);
        room.AddConnectingRoom(new RoomConnection(westRoom, Side.West, 0.2f));
        westRoom.SetSideUnanchored(Side.Top, northRoom.GetSide(Side.Top) - 0.4f);

        var eastRoom = new Room();
        eastRoom.Width = 10f;
        eastRoom.Height = 4f;
        eastRoom.Depth = 4f;
        eastRoom.SideTextures[Side.Top] = new TextureInfo(TextureKey.Ceiling);
        eastRoom.SideTextures[Side.Bottom] = floorTexture;
        eastRoom.MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);
        room.AddConnectingRoom(new RoomConnection(eastRoom, Side.East, 0.2f));

        var world = new WorldSegment();
        world.AddChild(room);
        world.AddChild(southRoom);
        world.AddChild(northRoom);
        world.AddChild(northRoom2);
        world.AddChild(eastRoom);
        world.AddChild(westRoom);

        return world;
    }

    private WorldSegment RoomWithFireplace()
    {
        var simpleRoom = new SimpleRoom();
        simpleRoom.Width = 16f;
        simpleRoom.Height = 4f;
        simpleRoom.Depth = 8f;
        simpleRoom.Y = 2;

        simpleRoom.SideTextures[Side.Top] = new TextureInfo(TextureKey.Ceiling);
        simpleRoom.SideTextures[Side.Bottom] = new TextureInfo(TextureKey.Floor);
        simpleRoom.MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);

        var officeDesk = new OfficeDesk(simpleRoom);
        officeDesk.Place().OnFloor();
        officeDesk.Place().OnSideInner(Side.West);

        var fireplace = new ElectricFireplace(simpleRoom);
        fireplace.Place().OnFloor();
        fireplace.Place().OnSideInner(Side.North);

        var ws = new WorldSegment();
        ws.AddChild(simpleRoom);
        return ws;
    }

    private WorldSegment RoomWithDesk()
    {
        var simpleRoom = new SimpleRoom();
        simpleRoom.Width = 16f;
        simpleRoom.Height = 4f;
        simpleRoom.Depth = 8f;
        simpleRoom.Y = 2;

        simpleRoom.SideTextures[Side.Top] = new TextureInfo(TextureKey.Ceiling);
        simpleRoom.SideTextures[Side.Bottom] = new TextureInfo(TextureKey.Floor);
        simpleRoom.MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);

        var officeDesk = new OfficeDesk(simpleRoom);
        officeDesk.Place().OnFloor();
        officeDesk.Place().OnSideInner(Side.North);
        officeDesk.Z += 0.1f;

        var ws = new WorldSegment();
        ws.AddChild(simpleRoom);
        return ws;
    }

    private Shape FaceCutoutTestRoom()
    {
        var simpleRoom = new SimpleRoom();
        simpleRoom.Width = 16f;
        simpleRoom.Height = 4f;
        simpleRoom.Depth = 8f;
        simpleRoom.Y = 2;

        simpleRoom.SideTextures[Side.Top] = new TextureInfo(TextureKey.Ceiling);
        simpleRoom.SideTextures[Side.Bottom] = new TextureInfo(TextureKey.Floor);
        simpleRoom.MainTexture = new TextureInfo(TextureKey.Wall);

        var testShape = new FaceCutoutTest();
        testShape.MainTexture = new TextureInfo(TextureKey.Wall);
        simpleRoom.AddChild(testShape);
        testShape.Place().OnFloor();
        testShape.Y += 1.0f;
        return simpleRoom;
    }

    private Shape MengerSpongeRoom()
    {
        var simpleRoom = new SimpleRoom();
        simpleRoom.Width = 16f;
        simpleRoom.Height = 4f;
        simpleRoom.Depth = 8f;
        simpleRoom.Y = 2;

        simpleRoom.SideTextures[Side.Top] = new TextureInfo(TextureKey.Ceiling);
        simpleRoom.SideTextures[Side.Bottom] = new TextureInfo(TextureKey.Floor);
        simpleRoom.MainTexture = new TextureInfo(TextureKey.Wall);

        var sponge = new MengerSponge(new ShapeSplitter());
        simpleRoom.AddChild(sponge);
        sponge.Size = new Vector3(3f, 3f, 3f);
        sponge.Place().OnFloor();
        sponge.MainTexture = new TextureInfo(Color.Purple);
        return simpleRoom;
    }

    private void SetBuffers()
    {        
        var builder = new VertexBufferBuilder();

        var triangles = _mainShape.Build((QualityLevel)8);
        var buffers = builder.Build(triangles, _basementTextures, GraphicsDevice);

        _roomBuffer = buffers.Item1;
        _roomIndices = buffers.Item2;
        _triangleCount = buffers.Item3;
    }

    private bool _collisionEnabled = true;
    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (!IsActive)
            return;

        _playerInput.Update();
        _playerGroundMover.Update();
        _playerGravityMover.Update();
        _gravityController.Update();
        _playerMotion.Update(gameTime, Window);

        if(_playerInput.IsKeyPressed(GameKey.DebugToggleCollision))
            _collisionEnabled = !_collisionEnabled;
            
        if(_collisionEnabled)
            _playerCollider.Update();

        _view = _player.CreateViewMatrix();
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {       
        GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer,Color.CornflowerBlue,1.0f,0);
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        _effect.World = Matrix.Identity;
        _effect.View = _view;
        _effect.Projection = _projection;
        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            GraphicsDevice.SetVertexBuffer(_roomBuffer);
            GraphicsDevice.Indices = _roomIndices;
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _triangleCount);
        }

        //_pointLightEffect.Parameters["World"].SetValue(Matrix.Identity);
        //_pointLightEffect.Parameters["View"].SetValue(_view);
        //_pointLightEffect.Parameters["Projection"].SetValue(_projection);

        //foreach (var pass in _pointLightEffect.CurrentTechnique.Passes)
        //{
        //    pass.Apply();
        //    GraphicsDevice.SetVertexBuffer(_roomBuffer);
        //    GraphicsDevice.Indices = _roomIndices;
        //    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _triangleCount);
        //}

        // Draw debug information
        _spriteBatch.Begin();
        _spriteBatch.DrawString(_debugFont, "Position: " + _player.Position.ToString(), new Vector2(10, 10), Color.White);
        _spriteBatch.DrawString(_debugFont, "Yaw: " + _player.Rotation.Yaw.ToString(), new Vector2(10, 30), Color.White);
        _spriteBatch.DrawString(_debugFont, "Pitch: " + _player.Rotation.Pitch.ToString(), new Vector2(10, 50), Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
