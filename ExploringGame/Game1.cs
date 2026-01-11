using ExploringGame.Entities;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.Rooms;
using ExploringGame.GeometryBuilder.Shapes.TestShapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Logics.Collision;
using ExploringGame.Logics.ShapeControllers;
using ExploringGame.Motion;
using ExploringGame.Rendering;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq; // For random texture generation

namespace ExploringGame;

public class Game1 : Game
{
    private ServiceContainer _serviceContainer;
    private Player _player;
    private PlayerMotion _playerMotion;
    private HeadBob _headBob;
    private PlayerInput _playerInput;

    private WorldSegment _mainShape;

    private List<IActiveObject> _activeObjects = new();
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private PointLightRenderEffect _pointLightEffect;
    private BasicRenderEffect _basicEffect;
    private IRenderEffect _renderEffect;

    private ShapeBuffer[] _shapeBuffers;
    private Matrix _view;
    private Matrix _projection;
    private TextureSheet _basementTextures;
    private SpriteFont _debugFont;

    private SetupColliderBodies _setupColliderBodies;
    private Physics _physics;

    private Dictionary<Shape, Triangle[]> _triangles;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
    }

    protected override void Initialize()
    {
        _serviceContainer = new ServiceContainer();

        // Set up projection
        _projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.ToRadians(70f), //MathHelper.PiOver4, 
            GraphicsDevice.Viewport.AspectRatio,
            0.1f, 100f);

        _serviceContainer.BindSingleton<PointLights>();
        _serviceContainer.BindSingleton<Player>();
        _serviceContainer.BindTransient<SetupColliderBodies>();
        _serviceContainer.BindSingleton<AudioService>();

        _physics = new Physics();
        _serviceContainer.Bind(_physics);

        _playerInput = new PlayerInput();
        _headBob = new HeadBob();
        _player = _serviceContainer.Get<Player>();

        var playerMover = new EntityMover(_player, _physics);
        _activeObjects.Add(playerMover);

        playerMover.CollisionResponder.AddResponse(new DetectFloorCollision(playerMover));

        _serviceContainer.Bind(_playerInput);
        _serviceContainer.BindTransient<DoorController>();

        _mainShape = CreateMainShape();                        
        _playerMotion = new PlayerMotion(_player, _headBob, _playerInput, playerMover);
        _activeObjects.AddRange(_serviceContainer.CreateControllers(_mainShape.TraverseAllChildren()));

        _setupColliderBodies = _serviceContainer.Get<SetupColliderBodies>();
        
        _graphics.PreferredDepthStencilFormat = DepthFormat.Depth24;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        _triangles = _mainShape.Build((QualityLevel)8);
        _setupColliderBodies.Execute(_mainShape);
       
        base.Initialize();
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

        _shapeBuffers = new ShapeBufferCreator(_triangles, _basementTextures, GraphicsDevice).Execute();
        _triangles.Clear();

        // Load debug font
        _debugFont = Content.Load<SpriteFont>("Font");

        _basicEffect = new BasicRenderEffect(GraphicsDevice, Content, _basementTextures.Texture);
        _pointLightEffect = new PointLightRenderEffect(_serviceContainer.Get<PointLights>(), 
            GraphicsDevice, Content, _basementTextures.Texture);

        _renderEffect = _pointLightEffect;
        _serviceContainer.Get<AudioService>().LoadContent(Content);
    }

    private WorldSegment CreateMainShape()
    {
        return new BasementWorldSegment();
    }

    public WorldSegment OilTankTest() => ComplexShapeTest(room => new OilTank(room));

    private WorldSegment CircleCutoutTest()
    {
        var worldSegment = new WorldSegment();
        var room = new Room(worldSegment, new BasementRoomTheme());
        room.Width = 8;
        room.Height = 3f;
        room.Depth = 8f;
        room.Y = 2;

        var light = new HighHatLight(room);
        light.Width = 2.0f;
        light.Depth = 2.0f;
        light.Height = 1.0f;
        light.Place().OnSideOuter(Side.Top, room);


        return worldSegment;
    }

    private WorldSegment ComplexShapeTest(Func<Shape, Shape> createShape)
    {
        var simpleRoom = new SimpleRoom(new BasementRoomTheme());
        simpleRoom.Width = 16f;
        simpleRoom.Height = 4f;
        simpleRoom.Depth = 8f;
        simpleRoom.Y = 2;

        var shape = simpleRoom.AddChild(createShape(simpleRoom));
        shape.Position = simpleRoom.Position;
        shape.Place().OnFloor();
        shape.Z += 2.0f;

        return new WorldSegment(simpleRoom);
    }

    private WorldSegment DoorTest()
    {
        var simpleRoom = new SimpleRoom(new BasementRoomTheme());
        simpleRoom.Width = 10f;
        simpleRoom.Height = 4f;
        simpleRoom.Depth = 10f;
        simpleRoom.Y = 2;

        var door1 = simpleRoom.AddChild(new Door(simpleRoom, new Angle(270f), new Angle(180f), HAlign.Left));
        door1.Place().OnFloor();
        door1.Z -= 3.0f;
        door1.X -= 2.0f;
        door1.Theme.MainTexture = new TextureInfo(Key: TextureKey.Ceiling, Color: Color.Red);

        var door2 = simpleRoom.AddChild(new Door(simpleRoom, new Angle(90), new Angle(180f), HAlign.Right));
        door2.Place().OnFloor();
        door2.Z -= 3.0f;
        door2.X += 2.0f;
        door2.Theme.MainTexture = new TextureInfo(Key: TextureKey.Ceiling, Color: Color.Blue);

        return new WorldSegment(simpleRoom);
    }

    private WorldSegment DoorTest2()
    {
        var simpleRoom = new SimpleRoom(new BasementRoomTheme());
        simpleRoom.Width = 10f;
        simpleRoom.Height = 4f;
        simpleRoom.Depth = 10f;
        simpleRoom.Y = 2;
        
        var closet = new BasementCloset(simpleRoom, Side.East);
        closet.Position = simpleRoom.Position;
        closet.Place().OnFloor();
        closet.Z -= 3.0f;

        var closet2 = new BasementCloset(simpleRoom, Side.West);
        closet2.Position = simpleRoom.Position;
        closet2.Place().OnFloor();
        closet2.Z += 3.0f;

        return new WorldSegment(simpleRoom);
    }


    private WorldSegment PhysicsTest()
    {
        var simpleRoom = new SimpleRoom(new BasementRoomTheme());
        simpleRoom.Width = 10f;
        simpleRoom.Height = 8f;
        simpleRoom.Depth = 10f;
        simpleRoom.Y = 2;
    
        var test = new PhysicsTestShape();
        test.Y = 0.0f;
        test.Z = -1.0f;

        simpleRoom.AddChild(test);

        var world = new WorldSegment();
        world.AddChild(simpleRoom);

        return world;
    }

    private WorldSegment MotionTest()
    {
       
        var simpleRoom = new SimpleRoom(new BasementRoomTheme());
        simpleRoom.Width = 16f;
        simpleRoom.Height = 4f;
        simpleRoom.Depth = 8f;
        simpleRoom.Y = 2.0f;
        
        var box = new TestMover();        
        simpleRoom.AddChild(box);
       
        return new WorldSegment(simpleRoom);
    }

    private WorldSegment FurnitureRotateTest()
    {
        var simpleRoom = new SimpleRoom(new BasementRoomTheme());
        simpleRoom.Width = 16f;
        simpleRoom.Height = 4f;
        simpleRoom.Depth = 8f;
        simpleRoom.Y = 2;

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
        var simpleRoom = new SimpleRoom(new BasementRoomTheme());
        simpleRoom.Width = 16f;
        simpleRoom.Height = 4f;
        simpleRoom.Depth = 8f;
        simpleRoom.Y = 2;
   
        var world = new WorldSegment();
        world.AddChild(simpleRoom);

        return world;
    }

    private WorldSegment ConnectingRoomsTest()
    {
        var world = new WorldSegment();
        var floorTexture = new TextureInfo(Key: TextureKey.Floor, Style: TextureStyle.XZTile, TileSize: 50.0f);

        var pos = 0.3f;

        var room = new Room(world);
        room.Width = 16f;
        room.Height = 4f;
        room.Depth = 12f;
        room.Y = 2;

        room.Theme.SideTextures[Side.Top] = new TextureInfo(TextureKey.Ceiling);
        room.Theme.SideTextures[Side.Bottom] = floorTexture;
        room.Theme.MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);

        var southRoom = new Room(world);
        southRoom.Width = 4f;
        southRoom.Height = 4f;
        southRoom.Depth = 10f;
        southRoom.Theme.SideTextures[Side.Top] = new TextureInfo(TextureKey.Ceiling);
        southRoom.Theme.SideTextures[Side.Bottom] = floorTexture;
        southRoom.Theme.MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);
        room.AddConnectingRoom(new RoomConnection(room, southRoom, Side.South, pos));

        var northRoom = new Room(world);
        northRoom.Width = 4f;
        northRoom.Height = 4f;
        northRoom.Depth = 10f;
        northRoom.Theme.SideTextures[Side.Top] = new TextureInfo(TextureKey.Ceiling);
        northRoom.Theme.SideTextures[Side.Bottom] = floorTexture;
        northRoom.Theme.MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);
        room.AddConnectingRoom(new RoomConnection(room, northRoom, Side.North, pos));
        northRoom.SetSideUnanchored(Side.Bottom, northRoom.GetSide(Side.Bottom) + 0.4f);

        var northRoom2 = new Room(world);
        northRoom2.Width = 40f;
        northRoom2.Height = 4f;
        northRoom2.Depth = 40f;
        northRoom2.Theme.SideTextures[Side.Top] = new TextureInfo(TextureKey.Ceiling);
        northRoom2.Theme.SideTextures[Side.Bottom] = floorTexture;
        northRoom2.Theme.MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);
        northRoom.AddConnectingRoom(new RoomConnection(northRoom, northRoom2, Side.North, pos));

        var westRoom = new Room(world);
        westRoom.Width = 10f;
        westRoom.Height = 4f;
        westRoom.Depth = 4f;
        westRoom.Theme.SideTextures[Side.Top] = new TextureInfo(TextureKey.Ceiling);
        westRoom.Theme.SideTextures[Side.Bottom] = floorTexture;
        westRoom.Theme.MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);
        room.AddConnectingRoom(new RoomConnection(room,westRoom, Side.West, pos));
        westRoom.SetSideUnanchored(Side.Top, northRoom.GetSide(Side.Top) - 0.4f);

        var eastRoom = new Room(world);
        eastRoom.Width = 10f;
        eastRoom.Height = 4f;
        eastRoom.Depth = 4f;
        eastRoom.Theme.SideTextures[Side.Top] = new TextureInfo(TextureKey.Ceiling);
        eastRoom.Theme.SideTextures[Side.Bottom] = floorTexture;
        eastRoom.Theme.MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);
        room.AddConnectingRoom(new RoomConnection(room, eastRoom, Side.East, pos));

        return world;
    }

    private WorldSegment RoomWithFireplace()
    {
        var simpleRoom = new SimpleRoom(new BasementRoomTheme());
        simpleRoom.Width = 16f;
        simpleRoom.Height = 4f;
        simpleRoom.Depth = 8f;
        simpleRoom.Y = 2;

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
        var simpleRoom = new SimpleRoom(new BasementRoomTheme());
        simpleRoom.Width = 16f;
        simpleRoom.Height = 4f;
        simpleRoom.Depth = 8f;
        simpleRoom.Y = 2;

        var officeDesk = new OfficeDesk(simpleRoom);
        officeDesk.Place().OnFloor();
        officeDesk.Place().OnSideInner(Side.North);
        officeDesk.Z += 0.1f;

        var ws = new WorldSegment();
        ws.AddChild(simpleRoom);
        return ws;
    }

    private WorldSegment FaceCutoutTestRoom()
    {
        var simpleRoom = new SimpleRoom(new BasementRoomTheme());
        simpleRoom.Width = 16f;
        simpleRoom.Height = 4f;
        simpleRoom.Depth = 8f;
        simpleRoom.Y = 2;

        var testShape = new FaceCutoutTest();
        testShape.Theme.MainTexture = new TextureInfo(TextureKey.Wall);
        simpleRoom.AddChild(testShape);
        testShape.Place().OnFloor();
        testShape.Y += 1.0f;

        return new WorldSegment(simpleRoom);
    }

    private Shape MengerSpongeRoom()
    {
        var simpleRoom = new SimpleRoom(new BasementRoomTheme());
        simpleRoom.Width = 16f;
        simpleRoom.Height = 4f;
        simpleRoom.Depth = 8f;
        simpleRoom.Y = 2;

        var sponge = new MengerSponge(new ShapeSplitter());
        simpleRoom.AddChild(sponge);
        sponge.Size = new Vector3(3f, 3f, 3f);
        sponge.Place().OnFloor();
        sponge.MainTexture = new TextureInfo(Color.Purple);
        return simpleRoom;
    }

    private bool _initialized = false;

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (!IsActive)
            return;

        if (!_initialized)
        {
            foreach (var obj in _activeObjects)
                obj.Initialize();

            _initialized = true;
        }

        _physics.Update(gameTime);

        foreach (var obj in _activeObjects)
            obj.Update(gameTime);
       
        _playerInput.Update();
        _playerMotion.Update(gameTime, Window);

        if (_playerInput.IsKeyPressed(GameKey.DebugToggleShader))
        {
            if (_renderEffect == _basicEffect)
                _renderEffect = _pointLightEffect;
            else
                _renderEffect = _basicEffect;
        }
            
      //  if(_collisionEnabled)
       //     _playerCollider.Update();

        _view = _player.CreateViewMatrix();
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {       
        GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer,Color.CornflowerBlue,1.0f,0);
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        _renderEffect.Draw(GraphicsDevice, _shapeBuffers, _view, _projection);
        
        // Draw debug information
        _spriteBatch.Begin();
        _spriteBatch.DrawString(_debugFont,
            $"Position: X={_player.Position.X.ToString("0.00")} Y={_player.Position.Y.ToString("0.00")} Z={_player.Position.Z.ToString("0.00")}",
            new Vector2(10, 10), Color.White);

        _spriteBatch.DrawString(_debugFont, "Yaw: " + _player.Rotation.Yaw.ToString("0.00"), new Vector2(10, 30), Color.White);
        _spriteBatch.DrawString(_debugFont, "Pitch: " + _player.Rotation.Pitch.ToString("0.00"), new Vector2(10, 50), Color.White);

        _spriteBatch.DrawString(_debugFont, "Degrees: " + _player.Rotation.YawDegrees.ToString("0.00"), new Vector2(10, 80), Color.White);
        _spriteBatch.DrawString(_debugFont, "Watch1: " + Debug.Watch1 ?? "", new Vector2(10, 100), Color.White);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
