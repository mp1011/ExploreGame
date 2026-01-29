using ExploringGame.Entities;
using ExploringGame.GameDebug;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.Rooms;
using ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;
using ExploringGame.GeometryBuilder.Shapes.TestShapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Logics.Collision;
using ExploringGame.Logics.ShapeControllers;
using ExploringGame.Rendering;
using ExploringGame.Services;
using ExploringGame.Testing;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics.Tracing;

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
        _renderEffect = _basicEffect;
        return TestMaps.TextureTestMap(new UpstairsHallTheme());
      //  return new BasementWorldSegment(null);
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
            _player.Position = _mainShape.DefaultPlayerStart;
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
