using ExploringGame.Entities;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Logics.Controllers;
using ExploringGame.Testing;
using ExploringGame.Tests.TestHelpers;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using System.Text;
using Xunit;

namespace ExploringGame.Tests;

public class LightSpiritTests
{
    [Fact]
    public void LightSpirit_AfterOneSecond_RemainsInAbsentPhase()
    {
        // Arrange
        var worldSegment = CreateTestWorldWithLightSpirit();
        
        // Run for 1 second (60 frames at 60 FPS)
        using var game = new TestGame(worldSegment, TimeSpan.FromSeconds(1));
        
        // Act
        game.Run();

        // Assert
        var loadedLevelData = game.GetService<LoadedLevelData>();
        var levelData = loadedLevelData.LoadedSegments.FirstOrDefault();
        
        Assert.NotNull(levelData);
        
        // Find the Light Spirit in active objects
        var lightSpiritController = levelData.ActiveObjects
            .OfType<LightSpiritController>()
            .FirstOrDefault();
        
        Assert.NotNull(lightSpiritController);
        Assert.Equal(LightSpiritPhase.Absent, lightSpiritController.LightSpirit.Phase);
    }

    [Fact]
    public void LightSpirit_EventuallyTransitionsTo_BreakInPhase()
    {
        // Arrange
        var worldSegment = CreateTestWorldWithLightSpirit();
        
        // Run for up to 2 minutes, but test will pass as soon as phase changes
        using var game = new TestGame(worldSegment, TimeSpan.FromMinutes(2), (testGame, gameTime) =>
        {
            var loadedLevelData = testGame.GetService<LevelControl.LoadedLevelData>();
            var levelData = loadedLevelData.LoadedSegments.FirstOrDefault();
            
            if (levelData == null)
                return TestResult.CONTINUE;

            var lightSpiritController = levelData.ActiveObjects
                .OfType<LightSpiritController>()
                .FirstOrDefault();

            if (lightSpiritController == null)
                return TestResult.CONTINUE;

            // Check if phase has transitioned to BreakIn
            if (lightSpiritController.LightSpirit.Phase == LightSpiritPhase.BreakIn)
            {
                return TestResult.PASS;
            }

            return TestResult.CONTINUE;
        });
        
        game.Run();
    }

    [Fact]
    public void BreakInPhase_SpawnsGateMarks()
    {
        // Arrange
        var worldSegment = CreateTestWorldWithLightSpirit();
        
        // Run for up to 2 minutes, checking if any gatemarks are created
        using var game = new TestGame(worldSegment, TimeSpan.FromMinutes(10), (testGame, gameTime) =>
        {
            var loadedLevelData = testGame.GetService<LevelControl.LoadedLevelData>();
            var levelData = loadedLevelData.LoadedSegments.FirstOrDefault();
            
            if (levelData == null)
                return TestResult.CONTINUE;

            // Check if any GateMarks exist in the world
            var gateMarks = levelData.WorldSegment.TraverseAllChildren()
                .OfType<Entities.GateMark>()
                .ToList();

            if (gateMarks.Count > 0)
            {
                return TestResult.PASS;
            }

            return TestResult.CONTINUE;
        });
        
        game.Run();
    }

    [Fact]
    public void BreakInPhase_ActivatesGateMarks()
    {
        // Arrange
        var worldSegment = CreateTestWorldWithLightSpirit();
        
        // Run for up to 3 minutes, checking if any gatemarks become active
        using var game = new TestGame(worldSegment, TimeSpan.FromMinutes(10), (testGame, gameTime) =>
        {
            var loadedLevelData = testGame.GetService<LevelControl.LoadedLevelData>();
            var levelData = loadedLevelData.LoadedSegments.FirstOrDefault();
            
            if (levelData == null)
                return TestResult.CONTINUE;

            // Check if any GateMarks are active
            var activeGateMarks = levelData.WorldSegment.TraverseAllChildren()
                .OfType<Entities.GateMark>()
                .Where(gm => gm.IsActive)
                .ToList();

            if (activeGateMarks.Count > 0)
            {
                return TestResult.PASS;
            }

            return TestResult.CONTINUE;
        });
        
        game.Run();
    }

    [Fact]
    public void BreakInPhase_EventuallyTransitionsTo_HalfPresencePhase()
    {
        // Arrange
        var worldSegment = CreateTestWorldWithLightSpirit();
        
        // Run for up to 5 minutes, checking if LS reaches Half-Presence phase
        using var game = new TestGame(worldSegment, TimeSpan.FromMinutes(20), (testGame, gameTime) =>
        {
            var loadedLevelData = testGame.GetService<LevelControl.LoadedLevelData>();
            var levelData = loadedLevelData.LoadedSegments.FirstOrDefault();
            
            if (levelData == null)
                return TestResult.CONTINUE;

            var lightSpiritController = levelData.ActiveObjects
                .OfType<LightSpiritController>()
                .FirstOrDefault();

            if (lightSpiritController == null)
                return TestResult.CONTINUE;

            // Check if phase has transitioned to HalfPresence
            if (lightSpiritController.LightSpirit.Phase == LightSpiritPhase.HalfPresence)
            {
                return TestResult.PASS;
            }

            return TestResult.CONTINUE;
        });
        
        game.Run();
    }

    [Fact]
    public void LightSpirit_CanOpenDoors_JunctionTest()
    {
        var worldSegment = new BasementWorldSegment(null);
        using var game = new TestGame(worldSegment, TimeSpan.FromMinutes(20), (g, gameTime) =>
        {
            if (gameTime.TotalGameTime.TotalMilliseconds < 50)
                g.SetAllDoors(_ => false);


            if (gameTime.TotalGameTime.TotalMinutes > 10)
            {
                var openDoor = g.GetService<LoadedLevelData>().LoadedSegments.SelectMany(p => p.WorldSegment.TraverseAllChildren())
                .OfType<Door>()
                .FirstOrDefault(p => p.Open);

                if (openDoor != null)
                    return TestResult.PASS;
            }
            
            return TestResult.CONTINUE;
        });
        game.Run();
    }

    [Fact]
    public void LightSpirit_CanTurnOnLightSwitches_Basement()
    {
        // Arrange: Use BasementWorldSegment and turn all lights off
        var worldSegment = new ExploringGame.GeometryBuilder.Shapes.WorldSegments.BasementWorldSegment(null);
        using var game = new TestGame(worldSegment, TimeSpan.FromMinutes(5), (g, gameTime) =>
        {
            if (gameTime.TotalGameTime.TotalMilliseconds < 50)
            {
                g.SetAllLights(light => false);
                return TestResult.CONTINUE;
            }
            return TestResult.CONTINUE;
        });
        game.Run();

        // Assert: At least one light is on
        var loadedLevelData = game.GetService<LoadedLevelData>();
        var anyLightOn = loadedLevelData.LoadedSegments
            .SelectMany(ld => ld.WorldSegment.TraverseAllChildren())
            .OfType<ILightSource>()
            .Any(light => light.On);
        Assert.True(anyLightOn, "Expected at least one light to be on after 5 minutes.");
    }

    /// <summary>
    /// In this test, we allow the Light Spirit to spawn and wait long enough for it to reach the Half-Presence phase.
    /// We open all doors so that we're only testing the Light Spirit's ability to path-find to the player.
    /// </summary>
    [Fact]
    public void LightSpirit_CanSeekPlayer_Basement()
    {
        var worldSegment = new BasementWorldSegment(null);
        var log = new List<string>();
        double lastLogTime = 0;

        using var game = new TestGame(worldSegment, TimeSpan.FromMinutes(30), (g, gameTime) =>
        {
            if (gameTime.TotalGameTime.TotalMilliseconds < 50)
                g.SetAllDoors(d => d.StateKey != StateKey.LinenClosetDoorOpen);

            // check once every simulated minute
            if (gameTime.TotalGameTime.TotalSeconds - lastLogTime >= 60f)
            {
                lastLogTime = gameTime.TotalGameTime.TotalSeconds;
                var loadedLevelData = g.GetService<LoadedLevelData>();
                var lightSpiritController = loadedLevelData.LoadedSegments
                    .SelectMany(ld => ld.ActiveObjects)
                    .OfType<LightSpiritController>()
                    .FirstOrDefault();
                var player = g.GetService<Player>();
                if (lightSpiritController != null && player != null && lightSpiritController.LightSpirit.Phase == LightSpiritPhase.HalfPresence)
                {
                    var distance = Vector3.Distance(player.Position, lightSpiritController.LightSpirit.Position);
                    Console.WriteLine($"D = {distance}");
                    if (distance < 1.5f)
                        return TestResult.PASS;
                }
            }
            return TestResult.CONTINUE;
        });
        game.Run();
    }

    private WorldSegment CreateTestWorldWithLightSpirit()
    {
        var worldSegment = new TestWorldSegment(new Vector3(0, 1.5f, 5));

        // Create a simple room
        var room = new Room(worldSegment);
        room.Width = 10f;
        room.Height = 3f;
        room.Depth = 10f;
        room.Position = Vector3.Zero;

        // Add WallDecalStamp so GateMarks can be created as stamped shapes
        var wallDecalStamp = new GeometryBuilder.Shapes.Decals.WallDecalStamp();
        wallDecalStamp.Position = new Vector3(0, 5, 0); // Position doesn't matter for stamps
        worldSegment.AddChild(wallDecalStamp);

        // Add the Light Spirit
        var lightSpirit = new LightSpirit();
        lightSpirit.Position = new Vector3(0, -100, 0); // Start underground
        worldSegment.AddChild(lightSpirit);

        return worldSegment;
    }


}
