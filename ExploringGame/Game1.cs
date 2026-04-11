using ExploringGame.Entities;
using ExploringGame.GameDebug;
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
using System.Linq;

namespace ExploringGame;

public class Game1 : Game
{
    protected ServiceContainer _serviceContainer;
    private Player _player;
    protected CameraService _cameraService;
    private PlayerMotion _playerMotion;
    private DebugController _debugController;
    private IPlayerInput _playerInput;
    private EntityMover _playerMover;
    protected LoadedLevelData _loadedLevelData;
    private WorldSegmentActivationManager _segmentActivationManager;
    private WorldSegment _mainShape;

    protected GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private IRenderEffect _renderEffect;
    private IRenderEffect _skyboxEffect;
    private GrassRenderEffect _grassRenderEffect;


    private SpriteFont _debugFont;

    private SetupColliderBodies _setupColliderBodies;
    private Physics _physics;

    public virtual Random Random { get; }  = new Random();

    public Game1(WorldSegment mainWorldSegment)
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
        _graphics.IsFullScreen = false;
        _mainShape = mainWorldSegment;
    }

    protected virtual bool AlwaysActive => false;

    protected virtual IPlayerInput CreatePlayerInput() => new PlayerInput();
    
    protected override void Initialize()
    {
        _serviceContainer = new ServiceContainer();
        _serviceContainer.Bind(_serviceContainer);

        _serviceContainer.Bind<Game>(this);
        _physics = new Physics();
        _serviceContainer.Bind(_physics);

        _serviceContainer.Bind(Random);
        _serviceContainer.BindSingleton<GameState>();
        _serviceContainer.BindSingleton<LoadedTextureSheets>();  
        _serviceContainer.BindSingleton<PointLights>();
        _serviceContainer.BindSingleton<RoomLightingCalculator>();
        _serviceContainer.BindSingleton<WorldSegmentAnchorProcessor>();
        _serviceContainer.BindSingleton<LoadedLevelData>();
        _loadedLevelData = _serviceContainer.Get<LoadedLevelData>();
        _serviceContainer.BindSingleton<EntityRoomFinder>();

        _serviceContainer.BindSingleton<Player>();
        _serviceContainer.BindTransient<SetupColliderBodies>();
        _serviceContainer.BindSingleton<AudioService>();
        _serviceContainer.BindTransient<TestEntityController>();
        _serviceContainer.BindTransient<Testing.TestShapeStampGeneratorController>();
        _serviceContainer.BindTransient<Logics.Controllers.LightSpiritController>();

        _playerInput = CreatePlayerInput();
        _serviceContainer.Bind(_playerInput);
        _player = _serviceContainer.Get<Player>();

        _playerMover = new EntityMover(_player, _physics);
        _playerMover.CollisionResponder.AddResponse(new DetectFloorCollision(_playerMover));

        
        _serviceContainer.BindTransient<DoorController>();

        _playerMotion = new PlayerMotion(_player, _playerInput, _playerMover);
        _setupColliderBodies = _serviceContainer.Get<SetupColliderBodies>();

        _graphics.PreferredDepthStencilFormat = DepthFormat.Depth24;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        _serviceContainer.BindSingleton<CameraService>();
        _cameraService = _serviceContainer.Get<CameraService>();

        _serviceContainer.BindSingleton<DebugController>();
        _debugController = _serviceContainer.Get<DebugController>();

        _serviceContainer.BindSingleton<WorldSegmentActivationManager>();
        _segmentActivationManager = _serviceContainer.Get<WorldSegmentActivationManager>();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Load debug font
        _debugFont = Content.Load<SpriteFont>("Font");

        var basicEffect = new BasicRenderEffect(_serviceContainer.Get<RoomLightingCalculator>(), this);
        var pointLightEffect = new PointLightRenderEffect(_serviceContainer.Get<PointLights>(), _serviceContainer.Get<RoomLightingCalculator>(), this);
        var dualEffect = new TwoPassRenderEffect(basicEffect, pointLightEffect);

        var skyboxEffect = new SkyboxRenderEffect(this);

        var loadedTextures = _serviceContainer.Get<LoadedTextureSheets>();
        loadedTextures.AddTexture(new BasementTextureSheet(Content));
        loadedTextures.AddTexture(new UpstairsTextureSheet(Content));
        loadedTextures.AddTexture(new SkyTextureSheet(Content));
        loadedTextures.AddTexture(new OutdoorsTextureSheet(Content));

        dualEffect.SetTextures(loadedTextures);
        skyboxEffect.SetTextures(loadedTextures);

        _renderEffect = dualEffect;
        _grassRenderEffect = new GrassRenderEffect(_cameraService, this);
        _grassRenderEffect.SetTextures(loadedTextures);
        _skyboxEffect = skyboxEffect;
        _serviceContainer.Get<AudioService>().LoadContent(Content);
    }

    private bool _ranInit = false;
    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (!IsActive && !AlwaysActive)
            return;

        _physics.Update(gameTime);

        if(!_ranInit)
        {
            _ranInit = true;
            _player.Position = _mainShape.DefaultPlayerStart;
            _playerMover.Initialize();
            _segmentActivationManager.ActivateSegmentAndNeighbors(_mainShape);
        }

        _playerMover.Update(gameTime);
        _segmentActivationManager.Update();
        _loadedLevelData.Update(gameTime);

        _playerInput.Update(Window);
        if (_playerInput.IsKeyDown(GameKey.DebugKey))
        {
            _debugController.Update();
        }
        else
        {
            _playerMotion.Update(gameTime, Window);
        }

        // Update debug display with player health
        GameDebug.Debug.Watch1 = $"Player Health: {_player.Health}";
        
        // Check if player is dead
        if (_player.Health <= 0)
        {
            Exit();
        }

        _cameraService.Update();
        base.Update(gameTime);
    }

    protected virtual void DrawWorld(GraphicsDevice graphicsDevice)
    {
        graphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1.0f, 0);
        graphicsDevice.DepthStencilState = GameDebug.Debug.NoDepthStencil ? DepthStencilState.None : DepthStencilState.Default;

        // Render geometry first
        foreach (var levelData in _loadedLevelData.ActiveSegments)
        {
            _renderEffect.Draw(graphicsDevice, levelData.ShapeBuffers, _cameraService.View, _cameraService.Projection);
            _renderEffect.Draw(graphicsDevice, levelData.StampedShapeBuffers.ToArray(), _cameraService.View, _cameraService.Projection);

            // Render grass blades if present
            foreach(var grassBuffer in levelData.GrassShapeBuffers)
                _grassRenderEffect.Draw(graphicsDevice, grassBuffer, _cameraService.View, _cameraService.Projection);
        }

        // Render skybox LAST with custom shader that forces depth to 1.0
        if (_loadedLevelData.SkyboxBuffer != null)
        {
            _skyboxEffect.Draw(graphicsDevice, new[] { _loadedLevelData.SkyboxBuffer }, _cameraService.SkyboxView, _cameraService.Projection);
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        DrawWorld(GraphicsDevice);

        // Draw debug information
        _spriteBatch.Begin();
        _spriteBatch.DrawString(_debugFont,
            $"Position: X={_player.Position.X.ToString("0.00")} Y={_player.Position.Y.ToString("0.00")} Z={_player.Position.Z.ToString("0.00")}",
            new Vector2(10, 10), Color.White);

        _spriteBatch.DrawString(_debugFont, "Yaw: " + _player.Rotation.Yaw.ToString("0.00"), new Vector2(10, 30), Color.White);
        _spriteBatch.DrawString(_debugFont, "Pitch: " + _player.Rotation.Pitch.ToString("0.00"), new Vector2(10, 50), Color.White);

        _spriteBatch.DrawString(_debugFont, "Degrees: " + _player.Rotation.YawDegrees.ToString("0.00"), new Vector2(10, 80), Color.White);
        _spriteBatch.DrawString(_debugFont, "Watch1: " + Debug.Watch1 ?? "", new Vector2(10, 100), Color.White);
        _spriteBatch.DrawString(_debugFont, "Watch2: " + Debug.Watch2 ?? "", new Vector2(10, 120), Color.White);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
