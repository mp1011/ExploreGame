using ExploringGame.Entities;
using ExploringGame.GameDebug;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Logics.Collision;
using ExploringGame.Logics.ShapeControllers;
using ExploringGame.Rendering;
using ExploringGame.Rendering.RenderEffects;
using ExploringGame.Rendering.RenderPasses;
using ExploringGame.Services;
using ExploringGame.Story;
using ExploringGame.Story.Debug;
using ExploringGame.Story.PlotPoints;
using ExploringGame.Story.Scene01;
using ExploringGame.Story.Scene01.Act01;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
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
    private WorldSegmentGroup _currentSegmentGroup;
    private SceneManager _sceneManager;
    private DialogueManager _dialogueManager;
    private AudioService _audioService;

    protected GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private RenderPassRegistry _renderPassRegistry;
    private RenderTargetTransformService _renderTargetTransformService;
    private RenderTarget2D _environmentRenderTarget;

    private SpriteFont _debugFont;

    private SetupColliderBodies _setupColliderBodies;
    private Physics _physics;

    public virtual Random Random { get; }  = new Random();

    public Game1(WorldSegmentGroup currentGroup)
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
        _graphics.IsFullScreen = true00000000000000000000;
        _currentSegmentGroup = currentGroup;
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
        _serviceContainer.BindSingleton<RenderTargetTransformService>();
        _serviceContainer.BindSingleton<LoadedLevelData>();
        _loadedLevelData = _serviceContainer.Get<LoadedLevelData>();
        _renderTargetTransformService = _serviceContainer.Get<RenderTargetTransformService>();
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
        _player.Mover = _playerMover;
        _playerMover.CollisionResponder.AddResponse(new DetectFloorCollision(_playerMover));

        
        _serviceContainer.BindTransient<DoorController>();

        _playerMotion = new PlayerMotion(_player, _playerInput, _playerMover);
        _serviceContainer.Bind(_playerMotion);
        _setupColliderBodies = _serviceContainer.Get<SetupColliderBodies>();

        _graphics.PreferredDepthStencilFormat = DepthFormat.Depth24;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        _serviceContainer.BindSingleton<CameraService>();
        _cameraService = _serviceContainer.Get<CameraService>();

        _serviceContainer.BindSingleton<DebugController>();
        _debugController = _serviceContainer.Get<DebugController>();

        _serviceContainer.BindSingleton<WorldSegmentActivationManager>();
        _segmentActivationManager = _serviceContainer.Get<WorldSegmentActivationManager>();

        _serviceContainer.BindSingleton<SceneManager>();
        _sceneManager = _serviceContainer.Get<SceneManager>();

        _serviceContainer.BindSingleton<DialogueManager>();
        _dialogueManager = _serviceContainer.Get<
            DialogueManager>();
 
        base.Initialize();
    }

    protected virtual Scene LoadInitialScene()
    {
        return _serviceContainer.Get<DebugScene>();
//        return _serviceContainer.Get<SceneOne>();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Load debug font
        _debugFont = Content.Load<SpriteFont>("Font");

        // Set up render passes
        _renderPassRegistry = new RenderPassRegistry();

        var loadedTextures = _serviceContainer.Get<LoadedTextureSheets>();
        loadedTextures.AddTexture(new BasementTextureSheet(Content));
        loadedTextures.AddTexture(new UpstairsTextureSheet(Content));
        loadedTextures.AddTexture(new SkyTextureSheet(Content));
        loadedTextures.AddTexture(new OutdoorsTextureSheet(Content));
        _renderTargetTransformService.LoadContent(Content);

        var basicEffect = new BasicRenderEffect(_serviceContainer.Get<RoomLightingCalculator>(), this);
        var pointLightEffect = new PointLightRenderEffect(_serviceContainer.Get<PointLights>(), _serviceContainer.Get<RoomLightingCalculator>(), this);
       
        var opaquePass = new OpaqueRenderPass(pointLightEffect);
        opaquePass.LoadContent(this, loadedTextures);
        _renderPassRegistry.Register(opaquePass);

        // Create and register glass pass
        var glassEffect = new GlassRenderEffect(this);
        var glassPass = new GlassRenderPass(glassEffect);
        glassPass.LoadContent(this, loadedTextures);
        _renderPassRegistry.Register(glassPass);

        // Create and register grass pass
        var grassEffect = new GrassRenderEffect(_cameraService, this);
        var grassPass = new GrassRenderPass(grassEffect);
        grassPass.LoadContent(this, loadedTextures);
        _renderPassRegistry.Register(grassPass);

        // Create and register skybox pass (DrawOrder = 90)
        var skyboxEffect = new SkyboxRenderEffect(this);
        var skyboxPass = new SkyboxRenderPass(skyboxEffect);
        skyboxPass.LoadContent(this, loadedTextures);
        _renderPassRegistry.Register(skyboxPass);

        var staticShadowRenderEffect = new StaticShadowRenderEffect(this);
        staticShadowRenderEffect.SetTextures(loadedTextures);
        _renderPassRegistry.Register(new StaticShadowRenderPass(staticShadowRenderEffect));

        var dialogueRenderPass = _serviceContainer.Get<DialogueRenderPass>();
        dialogueRenderPass.LoadContent(this, loadedTextures);
        _renderPassRegistry.RegisterInterface(dialogueRenderPass);
      
        // Provide registry to LoadedLevelData so it can group buffers by pass
        _loadedLevelData.SetRenderPassRegistry(_renderPassRegistry);

        _serviceContainer.BindSingleton<PlotPointFactory>();

        _audioService = _serviceContainer.Get<AudioService>();
        _audioService.LoadContent(Content);
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
            _player.Position = _currentSegmentGroup.DefaultPlayerStart;
            _player.Rotation = new Rotation(Yaw: _currentSegmentGroup.DefaultPlayerAngle);
            _playerMover.Initialize();
            _segmentActivationManager.ActivateGroup(_currentSegmentGroup);

            _sceneManager.Initialize(LoadInitialScene());
        }

        _playerMover.Update(gameTime);
        _segmentActivationManager.Update();
        _loadedLevelData.Update(gameTime);
        _sceneManager.Update(gameTime);
        _dialogueManager.Update(gameTime);

        _playerInput.Update(Window);
        if (_playerInput.IsKeyDown(GameKey.DebugKey))
        {
            _debugController.Update();
        }
        else
        {
            _playerMotion.Update(gameTime, Window);
        }

        //if (_playerInput.IsKeyPressed(Keys.NumPad7))
        //    PointLightRenderEffect.DAtten += 0.01f;
        //if (_playerInput.IsKeyPressed(Keys.NumPad1))
        //    PointLightRenderEffect.DAtten -= 0.01f;
        //if (_playerInput.IsKeyPressed(Keys.NumPad9))
        //    PointLightRenderEffect.DMod += 0.1f;
        //if (_playerInput.IsKeyPressed(Keys.NumPad3))
        //    PointLightRenderEffect.DMod -= 0.1f;


        // Update debug display with player health
        // GameDebug.Debug.Watch1 = $"Player Health: {_player.Health}";

        // Check if player is dead
        if (_player.Health <= 0)
        {
            Exit();
        }

        _cameraService.Update();
        base.Update(gameTime);
    }

    private void EnsureEnvironmentRenderTarget(GraphicsDevice graphicsDevice)
    {
        var width = graphicsDevice.Viewport.Width;
        var height = graphicsDevice.Viewport.Height;

        if (_environmentRenderTarget != null &&
            _environmentRenderTarget.Width == width &&
            _environmentRenderTarget.Height == height)
        {
            return;
        }

        _environmentRenderTarget?.Dispose();
        _environmentRenderTarget = new RenderTarget2D(
            graphicsDevice,
            width,
            height,
            false,
            SurfaceFormat.Color,
            DepthFormat.Depth24);
    }

    protected virtual void DrawEnvironment(GraphicsDevice graphicsDevice)
    {
        graphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1.0f, 0);
        graphicsDevice.DepthStencilState = GameDebug.Debug.NoDepthStencil ? DepthStencilState.None : DepthStencilState.Default;

        foreach (var pass in _renderPassRegistry.EnvironmentPasses)
        {
            foreach (var levelData in _loadedLevelData.ActiveSegments)
            {
                // Get buffers for this pass
                if (levelData.BuffersByPass.TryGetValue(pass, out var buffers) && buffers.Count > 0)
                {
                    // Use skybox view for skybox pass, regular view for others
                    var view = pass is SkyboxRenderPass ? _cameraService.SkyboxView : _cameraService.View;
                    pass.Draw(graphicsDevice, buffers, view, _cameraService.Projection);
                }

                // Also draw stamped shapes for non-skybox passes
                if (pass is OpaqueRenderPass && levelData.StampedShapeBuffers.Any())
                {
                    pass.Draw(graphicsDevice, levelData.StampedShapeBuffers, _cameraService.View, _cameraService.Projection);
                }
            }
        }
    }

    protected virtual void DrawInterface(GraphicsDevice graphicsDevice)
    {
        foreach (var pass in _renderPassRegistry.InterfacePasses)
        {
            // dialogue pass is special
            if (pass is DialogueRenderPass)
            {
                // note - probably want a different view
                pass.Draw(graphicsDevice, null, _cameraService.View, _cameraService.Projection);
            }
        }
    }

    protected virtual void DrawWorld(GraphicsDevice graphicsDevice)
    {
        EnsureEnvironmentRenderTarget(graphicsDevice);

        var previousRenderTargets = graphicsDevice.GetRenderTargets();
        graphicsDevice.SetRenderTarget(_environmentRenderTarget);
        DrawEnvironment(graphicsDevice);

        if (previousRenderTargets.Length == 0)
        {
            graphicsDevice.SetRenderTarget(null);
        }
        else
        {
            graphicsDevice.SetRenderTargets(previousRenderTargets);
        }

        _renderTargetTransformService.Draw(_spriteBatch, graphicsDevice, _environmentRenderTarget);

        DrawInterface(graphicsDevice);
    }

    protected override void Draw(GameTime gameTime)
    {
        DrawWorld(GraphicsDevice);

        // Draw debug information
        _spriteBatch.Begin();
      //  _spriteBatch.DrawString(_debugFont, $"DAtten: {PointLightRenderEffect.DAtten}    DMod: {PointLightRenderEffect.DMod}", new Vector2(10, 30), Color.Green);
     //   _spriteBatch.DrawString(_debugFont, $"LAtten: {PointLightRenderEffect.LAtten}    LMod: {PointLightRenderEffect.LMod}", new Vector2(10, 50), Color.Green);

        //_spriteBatch.DrawString(_debugFont,
        //    $"Position: X={_player.Position.X.ToString("0.00")} Y={_player.Position.Y.ToString("0.00")} Z={_player.Position.Z.ToString("0.00")}",
        //    new Vector2(10, 10), Color.White);

        //_spriteBatch.DrawString(_debugFont, "Yaw: " + _player.Rotation.Yaw.ToString("0.00"), new Vector2(10, 30), Color.White);
        //_spriteBatch.DrawString(_debugFont, "Pitch: " + _player.Rotation.Pitch.ToString("0.00"), new Vector2(10, 50), Color.White);

        _spriteBatch.DrawString(_debugFont, "Degrees: " + _player.Rotation.YawDegrees.ToString("0.00"), new Vector2(10, 80), Color.White);
        _spriteBatch.DrawString(_debugFont, "Watch1: " + Debug.Watch1 ?? "", new Vector2(10, 100), Color.White);
        _spriteBatch.DrawString(_debugFont, "Watch2: " + Debug.Watch2 ?? "", new Vector2(10, 120), Color.White);

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    protected override void EndRun()
    {
        _environmentRenderTarget?.Dispose();
        _renderTargetTransformService?.Dispose();
        _audioService.Dispose();
    }
}
