using ExploringGame.GameDebug;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.GeometryBuilder.Shapes.Rooms.BasementRooms;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Services;
using ExploringGame.Tests.TestHelpers;
using Microsoft.Xna.Framework;
using System.Linq;
using Xunit;

namespace ExploringGame.Tests.VisualRegressionTests;

/// <summary>
/// Visual test for the lighting system - captures a screenshot for manual inspection.
/// </summary>
public class LightingVisualTests
{
    [Theory]
    [InlineData(LightIntensity.VeryDim)]
    [InlineData(LightIntensity.Dim)]
    [InlineData(LightIntensity.IndoorLight)]
    [InlineData(LightIntensity.Bright)]
    [InlineData(LightIntensity.ExtremelyBright)]
    public void Basement_PointLightVisual(double lightIntensity)
    {
        var basement = new HomeWorldSegmentGroup();

        // Generate screenshot name from test name and parameters
        var testName = nameof(Basement_PointLightVisual);
        var intensityName = GetIntensityName(lightIntensity);
        var screenshotName = $"{testName}_{intensityName}";

        using var game = new TestGame(basement, 
            simulationTime: TimeSpan.FromSeconds(1),
            screenshotName: screenshotName,
            testAssertion: (g, gameTime) =>
        {
            var basement = g.GetService<LoadedLevelData>().ActiveSegments.Select(p => p.WorldSegment).OfType<BasementWorldSegment>().Single();

            if (gameTime.TotalGameTime.TotalMilliseconds < 50)
            {
                LightIntensity.DefaultAmbientLight = LightIntensity.Darkness;
                var basementRoom = basement.TraverseAllChildren().OfType<Basement>().First();
                var basementLight = basementRoom.TraverseAllChildren().OfType<HighHatLight>().First();

                // Turn off all lights except the basement light
                g.SetAllLights(light => light == basementLight);

                // Set the light's intensity for this test
                basementLight.Intensity = (float)lightIntensity;

                // Create a fixed camera positioned to view the point light
                var cameraPosition = basementRoom.LocalPosition + new Vector3(3f, 0f, 4f);
                var lightPosition = basementLight.LocalPosition;

                var camera = new DebugFixedCamera(cameraPosition, new Rotation(0, 0, 0));

                // Replace the default camera with our fixed one
                var cameraService = g.GetService<CameraService>();
                cameraService.SetCamera(camera);
                return TestResult.PASS;
            }

            return TestResult.CONTINUE;
        });

        game.Run();

        game.AssertScreenshot($"Fixtures/{screenshotName}.png");
    }

    private static string GetIntensityName(double intensity)
    {
        return intensity switch
        {
            LightIntensity.Darkness => "Darkness",
            LightIntensity.VeryDim => "VeryDim",
            LightIntensity.Dim => "Dim",
            LightIntensity.IndoorLight => "IndoorLight",
            LightIntensity.Bright => "Bright",
            LightIntensity.VeryBright => "VeryBright",
            LightIntensity.ExtremelyBright => "ExtremelyBright",
            _ => intensity.ToString("F1")
        };
    }
}
