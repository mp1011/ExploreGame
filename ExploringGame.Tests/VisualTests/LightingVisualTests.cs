using ExploringGame.GameDebug;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.GeometryBuilder.Shapes.Rooms.BasementRooms;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Services;
using ExploringGame.Tests.TestHelpers;
using Microsoft.Xna.Framework;
using System.Linq;
using Xunit;

namespace ExploringGame.Tests.VisualTests;

/// <summary>
/// Visual test for the lighting system - captures a screenshot for manual inspection.
/// </summary>
public class LightingVisualTests
{
    [Fact]
    public void Basement_PointLightVisual()
    {
        var basement = new BasementWorldSegment(null);

        using var game = new TestGame(basement, simulationTime: TimeSpan.FromSeconds(1), testAssertion: (g, gameTime) =>
        {
            if (gameTime.TotalGameTime.TotalMilliseconds < 50)
            {
                var basementRoom = basement.TraverseAllChildren().OfType<Basement>().First();

                // Create a fixed camera positioned to view the point light
                var cameraPosition = basementRoom.Position + new Vector3(3f, 0f, 4f);
                var lightPosition = basementRoom.TraverseAllChildren().OfType<HighHatLight>().First().Position;

                var camera = new DebugFixedCamera(cameraPosition, new Rotation(0, 0, 0));
               
                // Replace the default camera with our fixed one
                var cameraService = g.GetService<CameraService>();
                cameraService.SetCamera(camera);

                return TestResult.PASS;
            }

            return TestResult.CONTINUE;
        });

        game.Run();


        game.AssertScreenshot("Fixtures/Basement_PointLightVisual.png");

    }
}
