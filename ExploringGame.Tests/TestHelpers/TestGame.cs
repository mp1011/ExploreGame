using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace ExploringGame.Tests.TestHelpers;

public class TestGame : Game1
{
    private int _framesRemaining;
    private bool _screenshotTaken = false;
    private RenderTarget2D _renderTarget;
    private string _screenshotPath;
    private TimeSpan _fakeElapsedTime = TimeSpan.Zero;
    private TimeSpan _fakeFrameTime = TimeSpan.FromMilliseconds(16.67); // 60 fps
    private Color[] _screenshotData; // Store screenshot in memory
    private Func<TestGame, GameTime, TestResult> _testAssertion;
    private bool _testPassed = false;
    private string _testFailureMessage;

    public MockPlayerInput MockPlayerInput { get; }


    public TestGame(WorldSegment worldSegment, TimeSpan simulationTime, Func<TestGame, GameTime, TestResult> testAssertion = null) : 
        this(worldSegment, (int)(simulationTime.TotalSeconds * 60), testAssertion)
    {}

    public TestGame(WorldSegment worldSegment, int framesToRun, Func<TestGame, GameTime, TestResult> testAssertion = null) : base(worldSegment, useTestRenderer: true)
    {
        MockPlayerInput = new MockPlayerInput();
        _framesRemaining = framesToRun;
        _testAssertion = testAssertion;
        
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
        try
        {
            if (--_framesRemaining <= 0 && _screenshotTaken)
            {
                Exit();            
            }

            var fakeTime = FakeFrameTime();
            base.Update(fakeTime);

            // Execute test assertion if provided
            if (_testAssertion != null)
            {
                var result = _testAssertion(this, fakeTime);

                switch (result)
                {
                    case TestResult.PASS:
                        _testPassed = true;
                        Exit();
                        break;

                    case TestResult.FAIL:
                        _testFailureMessage = "Test assertion failed during game execution";
                        Exit();
                        break;

                    case TestResult.CONTINUE:
                        // Keep running
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            _testFailureMessage = $"Exception during test execution: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}";
            Exit();
        }
    }

    private GameTime FakeFrameTime()
    {
        _fakeElapsedTime += _fakeFrameTime;
        return new GameTime(_fakeElapsedTime, _fakeFrameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        try
        {
            if (_framesRemaining <= 0 && !_screenshotTaken)
            {
                // Render to texture
                GraphicsDevice.SetRenderTarget(_renderTarget);
                DrawWorld(GraphicsDevice);
                GraphicsDevice.SetRenderTarget(null);
                
                // Save screenshot to memory
                _screenshotData = new Color[_renderTarget.Width * _renderTarget.Height];
                _renderTarget.GetData(_screenshotData);
                
                // Save screenshot to disk
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
        catch (Exception ex)
        {
            _testFailureMessage = $"Exception during draw: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}";
            Exit();
        }
    }

    public void AssertScreenshot(string referenceImagePath, double maxAverageDifference = 5.0)
    {
        if (_screenshotData == null)
            throw new InvalidOperationException("No screenshot has been taken yet. Make sure the game has run.");

        var fullReferencePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, referenceImagePath);
        if (!File.Exists(fullReferencePath))
            throw new FileNotFoundException($"Reference image not found: {fullReferencePath}");

        Texture2D referenceTexture;
        using (var stream = File.OpenRead(fullReferencePath))
        {
            referenceTexture = Texture2D.FromStream(GraphicsDevice, stream);
        }

        // Check dimensions match
        if (referenceTexture.Width != _renderTarget.Width || referenceTexture.Height != _renderTarget.Height)
        {
            throw new InvalidOperationException(
                $"Image dimensions don't match. Reference: {referenceTexture.Width}x{referenceTexture.Height}, " +
                $"Screenshot: {_renderTarget.Width}x{_renderTarget.Height}");
        }

        // Get reference image data
        var referenceData = new Color[referenceTexture.Width * referenceTexture.Height];
        referenceTexture.GetData(referenceData);

        // Compare images
        double totalDifference = 0;
        int pixelCount = _screenshotData.Length;

        for (int i = 0; i < pixelCount; i++)
        {
            var screenshot = _screenshotData[i];
            var reference = referenceData[i];

            // Calculate RGB difference
            double rDiff = Math.Abs(screenshot.R - reference.R);
            double gDiff = Math.Abs(screenshot.G - reference.G);
            double bDiff = Math.Abs(screenshot.B - reference.B);

            totalDifference += (rDiff + gDiff + bDiff) / 3.0;
        }

        double averageDifference = totalDifference / pixelCount;

        Assert.True(
            averageDifference <= maxAverageDifference,
            $"Screenshot does not match reference image. " +
            $"Average RGB difference: {averageDifference:F2} (max allowed: {maxAverageDifference}). " +
            $"Screenshot saved to: {_screenshotPath}");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _renderTarget?.Dispose();

            if (_testFailureMessage != null)
                Assert.Fail(_testFailureMessage);
            else if (_testAssertion != null && !_testPassed)
                Assert.Fail("Test did not pass before game simulation ended");
        }
        base.Dispose(disposing);
    }
}
