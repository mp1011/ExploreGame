using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.GeometryBuilder.Shapes.Rooms.BasementRooms;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Rendering;
using ExploringGame.Rendering.RenderEffects;
using ExploringGame.Services;
using ExploringGame.Tests.TestHelpers;
using System.Linq;
using Xunit;

namespace ExploringGame.Tests;

/// <summary>
/// Tests to verify the correct number of lights are passed to the shader for each room
/// </summary>
public class ShaderLightCountTests
{
    [Fact]
    public void WhenRenderingBasementOffice_ShaderGetsZeroLights()
    {
        using var game = new TestGame(new HomeWorldSegmentGroup(), simulationTime: TimeSpan.FromSeconds(0.1));
        game.Run();


        var basement = game.GetService<LoadedLevelData>().ActiveSegments.Select(p => p.WorldSegment).OfType<BasementWorldSegment>().Single();

        var basementRoom = basement.TraverseAllChildren().OfType<Basement>().First();
        var basementLight = basementRoom.TraverseAllChildren().OfType<HighHatLight>().First();

        // Turn off all lights except the basement light
        game.SetAllLights(light => light == basementLight);

        // Get the rendering components
        var pointLightEffect = CreatePointLightRenderEffect(game);
        var loadedLevelData = game.GetService<LoadedLevelData>();

        // Find the BasementOffice room
        var basementOffice = basement.TraverseAllChildren().OfType<BasementOffice>().First();

        // Find the shape buffer for BasementOffice
        var officeBuffer = loadedLevelData.LoadedSegments
            .SelectMany(s => s.ShapeBuffers)
            .FirstOrDefault(sb => sb.LightingGroup == basementOffice.LightingGroup);

        Assert.NotNull(officeBuffer);

        // Get the lights that would be passed to the shader
        var (_, _, _, count) = pointLightEffect.GetActiveLightsForBuffer(officeBuffer);

        // Verify that BasementOffice gets 0 lights (because the basement light is in a different room)
        Assert.Equal(0, count);
    }

    [Fact]
    public void WhenRenderingBasement_ShaderGetsOneLight()
    {
        using var game = new TestGame(new HomeWorldSegmentGroup(), simulationTime: TimeSpan.FromSeconds(0.1));
        game.Run();

        var basement = game.GetService<LoadedLevelData>().ActiveSegments.Select(p => p.WorldSegment).OfType<BasementWorldSegment>().Single();

        var basementRoom = basement.TraverseAllChildren().OfType<Basement>().First();
        var basementLight = basementRoom.TraverseAllChildren().OfType<HighHatLight>().First();

        // Turn off all lights except the basement light
        game.SetAllLights(light => light == basementLight);

        // Get the rendering components
        var pointLightEffect = CreatePointLightRenderEffect(game);
        var loadedLevelData = game.GetService<LoadedLevelData>();

        // Find the shape buffer for Basement
        var basementBuffer = loadedLevelData.LoadedSegments
            .SelectMany(s => s.ShapeBuffers)
            .FirstOrDefault(sb => sb.LightingGroup == basementRoom.LightingGroup);

        Assert.NotNull(basementBuffer);

        // Get the lights that would be passed to the shader
        var (_, _, _, count) = pointLightEffect.GetActiveLightsForBuffer(basementBuffer);

        // Verify that Basement gets exactly 1 light
        Assert.Equal(1, count);
    }

    private PointLightRenderEffect CreatePointLightRenderEffect(TestGame game)
    {
        var pointLights = game.GetService<PointLights>();
        var roomLightingCalculator = game.GetService<RoomLightingCalculator>();
        return new PointLightRenderEffect(pointLights, roomLightingCalculator, game);
    }
}
