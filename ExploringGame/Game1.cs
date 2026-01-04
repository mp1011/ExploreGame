using ExploringGame.Entities;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.Rooms;
using ExploringGame.GeometryBuilder.Shapes.TestShapes;
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

    // 3D room and camera fields
    private BasicEffect _effect;

    private ShapeBuffer[] _shapeBuffers;
    private Matrix _view;
    private Matrix _projection;
    private TextureSheet _basementTextures;
    private Effect _pointLightEffect;
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
       // _playerCollider = new EntityCollider { Entity = _player, CurrentWorldSegment = _mainShape };
     
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

        _serviceContainer.Get<AudioService>().LoadContent(Content);
    }

    private WorldSegment CreateMainShape()
    {
        return BasementOffice();
    }

    private WorldSegment DoorTest()
    {
        var simpleRoom = new SimpleRoom();
        simpleRoom.Width = 10f;
        simpleRoom.Height = 4f;
        simpleRoom.Depth = 10f;
        simpleRoom.Y = 2;

        simpleRoom.SideTextures[Side.Top] = new TextureInfo(TextureKey.Ceiling);
        simpleRoom.SideTextures[Side.Bottom] = new TextureInfo(Key: TextureKey.Floor, Style: TextureStyle.XZTile, TileSize: 50.0f);
        simpleRoom.MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);


        var door1 = simpleRoom.AddChild(new Door(simpleRoom, new Angle(270f), new Angle(180f), HingePosition.Left));
        door1.Place().OnFloor();
        door1.Z -= 3.0f;
        door1.X -= 2.0f;
        door1.MainTexture = new TextureInfo(Key: TextureKey.Ceiling, Color: Color.Red);

        var door2 = simpleRoom.AddChild(new Door(simpleRoom, new Angle(90), new Angle(180f), HingePosition.Right));
        door2.Place().OnFloor();
        door2.Z -= 3.0f;
        door2.X += 2.0f;
        door2.MainTexture = new TextureInfo(Key: TextureKey.Ceiling, Color: Color.Blue);

        return new WorldSegment(simpleRoom);
    }

    private WorldSegment DoorTest2()
    {
        var simpleRoom = new SimpleRoom();
        simpleRoom.Width = 10f;
        simpleRoom.Height = 4f;
        simpleRoom.Depth = 10f;
        simpleRoom.Y = 2;

        simpleRoom.SideTextures[Side.Top] = new TextureInfo(TextureKey.Ceiling);
        simpleRoom.SideTextures[Side.Bottom] = new TextureInfo(Key: TextureKey.Floor, Style: TextureStyle.XZTile, TileSize: 50.0f);
        simpleRoom.MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);

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
        var simpleRoom = new SimpleRoom();
        simpleRoom.Width = 10f;
        simpleRoom.Height = 8f;
        simpleRoom.Depth = 10f;
        simpleRoom.Y = 2;

        simpleRoom.SideTextures[Side.Top] = new TextureInfo(TextureKey.Ceiling);
        simpleRoom.SideTextures[Side.Bottom] = new TextureInfo(Key: TextureKey.Floor, Style: TextureStyle.XZTile, TileSize: 50.0f);
        simpleRoom.MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);


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
       
        var simpleRoom = new SimpleRoom();
        simpleRoom.Width = 16f;
        simpleRoom.Height = 4f;
        simpleRoom.Depth = 8f;
        simpleRoom.Y = 2.0f;
        
        simpleRoom.SideTextures[Side.Top] = new TextureInfo(TextureKey.Ceiling);
        simpleRoom.SideTextures[Side.Bottom] = new TextureInfo(TextureKey.Floor);
        simpleRoom.MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);

        var box = new TestMover();        
        simpleRoom.AddChild(box);
       
        return new WorldSegment(simpleRoom);
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
        simpleRoom.Width = 16f;
        simpleRoom.Height = 4f;
        simpleRoom.Depth = 8f;
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

        var pos = 0.3f;

        var room = new Room();
        room.Width = 16f;
        room.Height = 4f;
        room.Depth = 12f;
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
        room.AddConnectingRoom(new RoomConnection(southRoom, Side.South, pos));

        var northRoom = new Room();
        northRoom.Width = 4f;
        northRoom.Height = 4f;
        northRoom.Depth = 10f;
        northRoom.SideTextures[Side.Top] = new TextureInfo(TextureKey.Ceiling);
        northRoom.SideTextures[Side.Bottom] = floorTexture;
        northRoom.MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);
        room.AddConnectingRoom(new RoomConnection(northRoom, Side.North, pos));
        northRoom.SetSideUnanchored(Side.Bottom, northRoom.GetSide(Side.Bottom) + 0.4f);

        var northRoom2 = new Room();
        northRoom2.Width = 40f;
        northRoom2.Height = 4f;
        northRoom2.Depth = 40f;
        northRoom2.SideTextures[Side.Top] = new TextureInfo(TextureKey.Ceiling);
        northRoom2.SideTextures[Side.Bottom] = floorTexture;
        northRoom2.MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);
        northRoom.AddConnectingRoom(new RoomConnection(northRoom2, Side.North, pos));

        var westRoom = new Room();
        westRoom.Width = 10f;
        westRoom.Height = 4f;
        westRoom.Depth = 4f;
        westRoom.SideTextures[Side.Top] = new TextureInfo(TextureKey.Ceiling);
        westRoom.SideTextures[Side.Bottom] = floorTexture;
        westRoom.MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);
        room.AddConnectingRoom(new RoomConnection(westRoom, Side.West, pos));
        westRoom.SetSideUnanchored(Side.Top, northRoom.GetSide(Side.Top) - 0.4f);

        var eastRoom = new Room();
        eastRoom.Width = 10f;
        eastRoom.Height = 4f;
        eastRoom.Depth = 4f;
        eastRoom.SideTextures[Side.Top] = new TextureInfo(TextureKey.Ceiling);
        eastRoom.SideTextures[Side.Bottom] = floorTexture;
        eastRoom.MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);
        room.AddConnectingRoom(new RoomConnection(eastRoom, Side.East, pos));

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

    private WorldSegment FaceCutoutTestRoom()
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

        return new WorldSegment(simpleRoom);
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

    private bool _collisionEnabled = true;
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

        if(_playerInput.IsKeyPressed(GameKey.DebugToggleCollision))
            _collisionEnabled = !_collisionEnabled;
            
      //  if(_collisionEnabled)
       //     _playerCollider.Update();

        _view = _player.CreateViewMatrix();
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {       
        GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer,Color.CornflowerBlue,1.0f,0);
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        foreach (var shapeBuffer in _shapeBuffers)
        {
            _effect.World = shapeBuffer.Shape.GetWorldMatrix();
            _effect.View = _view;
            _effect.Projection = _projection;
            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.SetVertexBuffer(shapeBuffer.VertexBuffer);
                GraphicsDevice.Indices = shapeBuffer.IndexBuffer;
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, shapeBuffer.TriangleCount);
            }
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
