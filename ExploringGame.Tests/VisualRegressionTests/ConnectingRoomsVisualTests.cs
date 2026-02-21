using ExploringGame.Testing;
using ExploringGame.Tests.TestHelpers;
using Microsoft.Xna.Framework;
using Xunit;

namespace ExploringGame.Tests.VisualRegressionTests;

public class ConnectingRoomsVisualTests
{
    private const string ReferenceImageDir = @"VisualRegressionTests\ReferenceImages";

    /// <summary>
    /// Generates a reference screenshot with camera rotated left.
    /// Run this test, then manually copy the screenshot to ReferenceImages/ConnectingRooms_CameraLeft.png
    /// </summary>
    [Fact]
    public void ConnectingRoomsRendersCorrectly()
    {
        // Arrange
        var worldSegment = TestMaps.ConnectingRoomsTest();
        using var game = new TestGame(worldSegment, framesToRun: 100);

        game.MockPlayerInput.AddMouseDeltas(
            startFrame: 3, 
            numFrames: 40, 
            deltaPerFrame: new Vector2(-2.0f, 0f));

        game.Run();

        game.AssertScreenshot(referenceImagePath: $@"Fixtures\ConnectingRooms_CameraLeft.png");
    }  
}
