using ExploringGame.Config;
using ExploringGame.Entities;
using ExploringGame.Extensions;
using ExploringGame.GameDebug;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.Rooms;
using ExploringGame.GeometryBuilder.Shapes.TestShapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Logics.Collision;
using ExploringGame.Logics.ShapeControllers;
using ExploringGame.Rendering;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame;

public class Game1 : Game
{
    private ServiceContainer _serviceContainer;
    private Player _player;
    private CameraService _cameraService;
    private PlayerMotion _playerMotion;
    private DebugController _debugController;
    private HeadBob _headBob;
    private PlayerInput _playerInput;
    private EntityMover _playerMover;
    private LoadedLevelData _loadedLevelData;
    private WorldSegment _mainShape;

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private PointLightRenderEffect _pointLightEffect;
    private BasicRenderEffect _basicEffect;
    private IRenderEffect _renderEffect;


    private SpriteFont _debugFont;

    private SetupColliderBodies _setupColliderBodies;
    private Physics _physics;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
        _graphics.IsFullScreen = false;
    }

    protected override void Initialize()
    {
        _serviceContainer = new ServiceContainer();
        _serviceContainer.Bind(_serviceContainer);

        _serviceContainer.Bind<Game>(this);
        _physics = new Physics();
        _serviceContainer.Bind(_physics);

        _serviceContainer.BindSingleton<GameState>();
        _serviceContainer.BindSingleton<LoadedTextureSheets>();
        _serviceContainer.BindSingleton<TransitionShapesRegistrar>();      
        _serviceContainer.BindSingleton<LoadedLevelData>();
        _loadedLevelData = _serviceContainer.Get<LoadedLevelData>();

        _serviceContainer.BindSingleton<PointLights>();
        _serviceContainer.BindSingleton<Player>();
        _serviceContainer.BindTransient<SetupColliderBodies>();
        _serviceContainer.BindSingleton<AudioService>();
        
        _playerInput = new PlayerInput();
        _headBob = new HeadBob();
        _player = _serviceContainer.Get<Player>();

        _playerMover = new EntityMover(_player, _physics);
        _playerMover.CollisionResponder.AddResponse(new DetectFloorCollision(_playerMover));

        _serviceContainer.Bind(_playerInput);
        _serviceContainer.BindTransient<DoorController>();

        _mainShape = CreateMainShape();
    //    _player.Position = _mainShape.Children.First().Position;
        _playerMotion = new PlayerMotion(_player, _headBob, _playerInput, _playerMover);
        _setupColliderBodies = _serviceContainer.Get<SetupColliderBodies>();

        _graphics.PreferredDepthStencilFormat = DepthFormat.Depth24;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        _serviceContainer.BindSingleton<CameraService>();
        _cameraService = _serviceContainer.Get<CameraService>();

        _serviceContainer.BindSingleton<DebugController>();
        _debugController = _serviceContainer.Get<DebugController>();


        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Load debug font
        _debugFont = Content.Load<SpriteFont>("Font");

        _basicEffect = new BasicRenderEffect(this);
        _pointLightEffect = new PointLightRenderEffect(_serviceContainer.Get<PointLights>(), this);

        var loadedTextures = _serviceContainer.Get<LoadedTextureSheets>();
        loadedTextures.AddTexture(new BasementTextureSheet(Content));
        loadedTextures.AddTexture(new UpstairsTextureSheet(Content));

        // todo - need better way to load textures
        _basicEffect.SetTextures(loadedTextures);
        _pointLightEffect.SetTextures(loadedTextures);

        _renderEffect = _pointLightEffect;
        _serviceContainer.Get<AudioService>().LoadContent(Content);
    }

    private WorldSegment CreateMainShape()
    {
        return _serviceContainer.Get<BasementWorldSegment>();
       // return DoubleDoorJunctionTest(DoorDirection.Push);
    }

    private WorldSegment JunctionTest(HAlign doorAlign, DoorDirection doorDirection)
    {
        var ws = new WorldSegment();
        var room = new Room(ws, new BasementRoomTheme());
        room.Width = 10;
        room.Height = 3f;
        room.Depth = 10f;

        var westRoom = room.Copy();
        var eastRoom = room.Copy();
        var northRoom = room.Copy();
        var southRoom = room.Copy();

        room.AddConnectingRoomWithJunction(new DoorJunction(westRoom, Side.West, doorAlign, doorDirection, StateKey.OfficeDoor1Open), westRoom, Side.West);
        room.AddConnectingRoomWithJunction(new DoorJunction(eastRoom, Side.East, doorAlign, doorDirection, StateKey.OfficeDoor1Open), eastRoom, Side.East);
        room.AddConnectingRoomWithJunction(new DoorJunction(southRoom, Side.South, doorAlign, doorDirection, StateKey.OfficeDoor1Open), southRoom, Side.South);
        room.AddConnectingRoomWithJunction(new DoorJunction(northRoom, Side.North, doorAlign, doorDirection, StateKey.OfficeDoor1Open), northRoom, Side.North);

        return ws;
    }

    private WorldSegment DoubleDoorJunctionTest(DoorDirection doorDirection)
    {
        var ws = new WorldSegment();
        var room = new Room(ws, new BasementRoomTheme());
        room.Width = 10;
        room.Height = 3f;
        room.Depth = 10f;

        var westRoom = room.Copy();
        var eastRoom = room.Copy();
        var northRoom = room.Copy();
        var southRoom = room.Copy();

        room.AddConnectingRoomWithJunction(new DoubleDoorJunction(westRoom, Side.West, doorDirection, StateKey.OfficeDoor1Open), westRoom, Side.West);
        room.AddConnectingRoomWithJunction(new DoubleDoorJunction(eastRoom, Side.East, doorDirection, StateKey.OfficeDoor1Open), eastRoom, Side.East);
        room.AddConnectingRoomWithJunction(new DoubleDoorJunction(southRoom, Side.South, doorDirection, StateKey.OfficeDoor1Open), southRoom, Side.South);
        room.AddConnectingRoomWithJunction(new DoubleDoorJunction(northRoom, Side.North, doorDirection, StateKey.OfficeDoor1Open), northRoom, Side.North);

        return ws;
    }

    private WorldSegment TwoHallTest()
    {
        var ws = new WorldSegment();
        var room = new Room(ws, new BasementRoomTheme());
        room.Width = 10;
        room.Height = 3f;
        room.Depth = 10f;

        //var south1 = room.Copy(width: 2f);
        //room.AddConnectingRoom(new RoomConnection(room, south1, Side.South, HAlign.Left));
        //var south2 = room.Copy(width: 2f);
        //room.AddConnectingRoom(new RoomConnection(room, south2, Side.South, HAlign.Right));

        //var north1 = room.Copy(width: 2f);
        //room.AddConnectingRoom(new RoomConnection(room, north1, Side.North, HAlign.Left));
        //var north2 = room.Copy(width: 2f);
        //room.AddConnectingRoom(new RoomConnection(room, north2, Side.North, HAlign.Right));

        // Both sides indent = gap
        // left only = gap
        // right only = ok
        // neither ok
        // left only, no right - ok

        var west1 = room.Copy(depth: 2f);
        room.AddConnectingRoom(new RoomConnection(room, west1, Side.West, HAlign.Left, Offset: 1.0f));
        var west2 = room.Copy(depth: 2f);
        room.AddConnectingRoom(new RoomConnection(room, west2, Side.West, HAlign.Right, Offset: -0.0f));

        //var east1 = room.Copy(depth: 2f);
        //room.AddConnectingRoom(new RoomConnection(room, east1, Side.East, HAlign.Left));
        //var east2 = room.Copy(depth: 2f);
        //room.AddConnectingRoom(new RoomConnection(room, east2, Side.East, HAlign.Right));


        return ws;
    }

    private WorldSegment OilTankTest() => ComplexShapeTest(room => new OilTank(room));

    private WorldSegment CircleCutoutTest()
    {
        var worldSegment = new WorldSegment();
        var room = new Room(worldSegment, new BasementRoomTheme());
        room.Width = 8;
        room.Height = 3f;
        room.Depth = 8f;
        room.Y = 2;

        var light = new HighHatLight(room, 2.0f, 0f);
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

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (!IsActive)
            return;

        _physics.Update(gameTime);

        if(_loadedLevelData.LoadedSegments.Count == 0)
        {
            _loadedLevelData.LoadSegment(_mainShape);
            _loadedLevelData.SwapActive();
            _playerMover.Initialize();
        }

        _playerMover.Update(gameTime);
        _loadedLevelData.Update(gameTime);

        _playerInput.Update();
        if (_playerInput.IsKeyDown(GameKey.DebugKey))
        {
            _debugController.Update();
            if (_playerInput.IsKeyDown(GameKey.DebugKey) && _playerInput.IsKeyPressed(Keys.S))
            {
                if (_renderEffect == _basicEffect)
                    _renderEffect = _pointLightEffect;
                else
                    _renderEffect = _basicEffect;
            }
        }
        else
        {
            _playerMotion.Update(gameTime, Window);
        }


        _cameraService.Update();
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {       
        GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer,Color.CornflowerBlue,1.0f,0);
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        foreach(var levelData in _loadedLevelData.LoadedSegments)
            _renderEffect.Draw(GraphicsDevice, levelData.ShapeBuffers, _cameraService.View, _cameraService.Projection);
        
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
