using ExploringGame.GeometryBuilder.Shapes.Structures;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Services;
using ExploringGame.Story;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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

    private Action<TestGame> _testSetup;

    protected override bool AlwaysActive => true;
    public MockPlayerInput MockPlayerInput { get; }

    public override Random Random => new Random(12345);

    public TestGame(WorldSegmentGroup worldSegmentGroup, TimeSpan simulationTime, Func<TestGame, GameTime, TestResult> testAssertion = null, Action<TestGame> testSetup = null, string screenshotName = null) : 
        this(worldSegmentGroup, (int)(simulationTime.TotalSeconds * 60), testAssertion, testSetup, screenshotName)
    {}

    public TestGame(WorldSegmentGroup worldSegmentGroup, int framesToRun, Func<TestGame, GameTime, TestResult> testAssertion = null, Action<TestGame> testSetup = null, string screenshotName = null) 
        : base(worldSegmentGroup)
    {
        // seems to be some unknown problem with audio content in tests
        AudioService.Enabled = false;

        // Set higher default ambient light for visual tests (so rooms without lighting data are visible)
        LightIntensity.DefaultAmbientLight = LightIntensity.IndoorLight;

        MockPlayerInput = new MockPlayerInput();
        _framesRemaining = framesToRun;
        _testAssertion = testAssertion;
        _testSetup = testSetup;

        // Create screenshots directory in test output
        var screenshotDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Screenshots");
        Directory.CreateDirectory(screenshotDir);

        // Use provided screenshot name or auto-detect from call stack
        var filename = !string.IsNullOrEmpty(screenshotName) 
            ? $"{screenshotName}.png"
            : $"{GetTestNameFromCallStack()}.png";
        _screenshotPath = Path.Combine(screenshotDir, filename);

        _graphics.SynchronizeWithVerticalRetrace = false;
        IsFixedTimeStep = false;
    }

    protected override Scene LoadInitialScene()
    {
        return _serviceContainer.Get<NullScene>();
    }

    private static string GetTestNameFromCallStack()
    {
        try
        {
            var stackTrace = new StackTrace();

            // Look through the stack for a test method (has [Fact] or [Theory] attribute)
            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                var frame = stackTrace.GetFrame(i);
                var method = frame?.GetMethod();

                if (method == null)
                    continue;

                // Check if this method has [Fact] or [Theory] attribute
                var isTestMethod = method.GetCustomAttributes(typeof(Xunit.FactAttribute), true).Any() ||
                                   method.GetCustomAttributes(typeof(Xunit.TheoryAttribute), true).Any();

                if (isTestMethod)
                {
                    var testName = new StringBuilder(method.Name);

                    // Try to get parameter values from the current frame's local variables
                    // This is a best-effort approach
                    var parameters = method.GetParameters();
                    if (parameters.Length > 0)
                    {
                        // For Theory tests, append parameter info
                        // Note: We can't easily get actual runtime values, but we can indicate it's parameterized
                        testName.Append("_");

                        // Generate a timestamp-based suffix for parameterized tests
                        testName.Append(DateTime.Now.ToString("HHmmss_fff"));
                    }

                    return testName.ToString();
                }
            }
        }
        catch
        {
            // Fall back to timestamp if we can't determine test name
        }

        return $"test_{DateTime.Now:yyyyMMdd_HHmmss}";
    }

    public T GetService<T>() => _serviceContainer.Get<T>();

    protected override IPlayerInput CreatePlayerInput() => MockPlayerInput;

    protected override void LoadContent()
    {
        base.LoadContent();

        // Create render target with same size as back buffer
        _renderTarget = new RenderTarget2D(
            GraphicsDevice,
            GraphicsDevice.PresentationParameters.BackBufferWidth,
            GraphicsDevice.PresentationParameters.BackBufferHeight,
            false,
            SurfaceFormat.Color,
            DepthFormat.Depth24);

        Console.WriteLine("TEST BEGIN");
    }

    private TimeSpan _lastLogTime = TimeSpan.Zero;
    private bool _ranSetup = false;
    protected override void Update(GameTime gameTime)
    {
        try
        {
            if (--_framesRemaining <= 0 && _screenshotTaken)
            {
                Exit();            
            }
            
            if(!_ranSetup && _testSetup != null)
            {
                _testSetup.Invoke(this);
                _ranSetup = true;
            }

            var fakeTime = FakeFrameTime();
            if((fakeTime.TotalGameTime - _lastLogTime) > TimeSpan.FromMinutes(1))
            {
                _lastLogTime = fakeTime.TotalGameTime;
                Console.Write("*");
            }

            base.Update(fakeTime);

            // Execute test assertion if provided
            if (_testAssertion != null && !_testPassed && _testFailureMessage == null)
            {
                var result = _testAssertion(this, fakeTime);

                switch (result)
                {
                    case TestResult.PASS:
                        _testPassed = true;
                        _framesRemaining = 0;
                        break;

                    case TestResult.FAIL:
                        _testFailureMessage = "Test assertion failed during game execution";
                        _framesRemaining = 0;
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
            _framesRemaining = 0;
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

    public void SetAllLights(Func<ILightSource, bool> shouldTurnOn)
    {
        var loadedLevelData = GetService<LoadedLevelData>();
        var allLights = loadedLevelData.LoadedSegments
            .SelectMany(ld => ld.WorldSegment.TraverseAllChildren())
            .OfType<ILightSource>()
            .ToArray();

        foreach (var light in allLights)
            light.On = shouldTurnOn(light);
    }

    public void SetAllDoors(Func<Door, bool> shouldOpen)
    {
        var loadedLevelData = GetService<LoadedLevelData>();
        var allDoors = loadedLevelData.LoadedSegments
            .SelectMany(ld => ld.WorldSegment.TraverseAllChildren())
            .OfType<Door>()
            .ToArray();

        foreach (var door in allDoors)
            door.Open = shouldOpen(door);
    }

}
