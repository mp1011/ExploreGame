using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ExploringGame.Tests.TestHelpers;

public class TestGame : Game1
{
    private int _framesRemaining;
    private bool _screenshotTaken = false;
    private RenderTarget2D _renderTarget;
    private string _screenshotPath;

    public MockPlayerInput MockPlayerInput { get; }

    public TestGame(WorldSegment worldSegment, int framesToRun) : base(worldSegment, useTestRenderer: true)
    {
        MockPlayerInput = new MockPlayerInput();
        _framesRemaining = framesToRun;
        
        // Create screenshots directory in test output
        var screenshotDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Screenshots");
        Directory.CreateDirectory(screenshotDir);
        _screenshotPath = Path.Combine(screenshotDir, $"test_{DateTime.Now:yyyyMMdd_HHmmss}.png");

        _graphics.SynchronizeWithVerticalRetrace = false;
        IsFixedTimeStep = false;
    }

    public T GetService<T>() => _serviceContainer.Get<T>();

    protected override IPlayerInput CreatePlayerInput() => MockPlayerInput;

    protected override void LoadContent()
    {
        base.LoadContent();
        _renderEffect = _basicEffect;
        
        // Create render target with same size as back buffer
        _renderTarget = new RenderTarget2D(
            GraphicsDevice,
            GraphicsDevice.PresentationParameters.BackBufferWidth,
            GraphicsDevice.PresentationParameters.BackBufferHeight,
            false,
            SurfaceFormat.Color,
            DepthFormat.Depth24);
    }

    protected override void Update(GameTime gameTime)
    {
        if (--_framesRemaining <= 0 && _screenshotTaken)
        {
            Exit();            
        }
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        if (_framesRemaining <= 0 && !_screenshotTaken)
        {
            // Render to texture
            GraphicsDevice.SetRenderTarget(_renderTarget);
            DrawWorld(GraphicsDevice);
            GraphicsDevice.SetRenderTarget(null);
            
            // Save screenshot
            using (var stream = File.Create(_screenshotPath))
            {
                _renderTarget.SaveAsPng(stream, _renderTarget.Width, _renderTarget.Height);
            }
            
            _screenshotTaken = true;
            Console.WriteLine($"Screenshot saved to: {_screenshotPath}");
        }
        else
        {
            // draw nothing
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _renderTarget?.Dispose();
        }
        base.Dispose(disposing);
    }
}
