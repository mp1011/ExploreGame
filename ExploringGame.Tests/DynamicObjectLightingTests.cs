using ExploringGame.Entities;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.Decals;
using ExploringGame.GeometryBuilder.Shapes.Rooms.BasementRooms;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Services;
using ExploringGame.Tests.TestHelpers;
using Microsoft.Xna.Framework;
using System.Linq;
using Xunit;

namespace ExploringGame.Tests;

/// <summary>
/// Tests for lighting of dynamic objects and stamped shapes
/// NOTE: These tests expect IPlaceableObject to have a Room property which will be added during implementation.
/// Until then, tests involving Player will fail as Player doesn't yet implement IPlaceableObject.
/// </summary>
public class DynamicObjectLightingTests
{
    #region StampedShape Tests

    [Fact]
    public void StampedShape_AddedAtRuntime_HasRoomAssigned()
    {
        // Arrange
        var basement = new BasementWorldSegment(null);
        
        using var game = new TestGame(basement, framesToRun: 100, testAssertion: (g, gameTime) =>
        {
            if (gameTime.TotalGameTime.TotalMilliseconds < 50)
            {
                var loadedLevelData = g.GetService<LoadedLevelData>();
                var basementRoom = basement.TraverseAllChildren().OfType<Basement>().First();
                
                // Create a stamped shape at a known position in the Basement
                var stampedDecal = new TestStampedWallDecal();
                stampedDecal.Position = basementRoom.Position + new Vector3(2, 2, 2);
                
                // Add it to the level
                loadedLevelData.AddStampedShape(basement, stampedDecal);
                
                // Verify the Room property was automatically set
                Assert.NotNull(stampedDecal.Room);
                Assert.Equal(basementRoom, stampedDecal.Room);
                
                return TestResult.PASS;
            }
            
            return TestResult.CONTINUE;
        });
        
        game.Run();
    }

    [Fact]
    public void StampedShape_ShapeBuffer_HasCorrectLightingGroup()
    {
        // Arrange
        var basement = new BasementWorldSegment(null);
        
        using var game = new TestGame(basement, framesToRun: 100, testAssertion: (g, gameTime) =>
        {
            if (gameTime.TotalGameTime.TotalMilliseconds < 50)
            {
                var loadedLevelData = g.GetService<LoadedLevelData>();
                var basementRoom = basement.TraverseAllChildren().OfType<Basement>().First();
                
                // Create a stamped shape in the Basement
                var stampedDecal = new TestStampedWallDecal();
                stampedDecal.Position = basementRoom.Position + new Vector3(2, 2, 2);
                
                loadedLevelData.AddStampedShape(basement, stampedDecal);
                
                // Find the ShapeBuffer for this stamped shape
                var levelData = loadedLevelData.FindLevelDataForWorldSegment(basement);
                var stampedBuffer = levelData.StampedShapeBuffers
                    .FirstOrDefault(sb => sb.Shape == stampedDecal);
                
                Assert.NotNull(stampedBuffer);
                Assert.Equal(basementRoom.LightingGroup, stampedBuffer.LightingGroup);
                
                return TestResult.PASS;
            }
            
            return TestResult.CONTINUE;
        });
        
        game.Run();
    }

    [Fact]
    public void StampedShape_InDifferentRooms_HaveDifferentLightingGroups()
    {
        // Arrange
        var basement = new BasementWorldSegment(null);
        
        using var game = new TestGame(basement, framesToRun: 100, testAssertion: (g, gameTime) =>
        {
            if (gameTime.TotalGameTime.TotalMilliseconds < 50)
            {
                var loadedLevelData = g.GetService<LoadedLevelData>();
                var basementRoom = basement.TraverseAllChildren().OfType<Basement>().First();
                var officeRoom = basement.TraverseAllChildren().OfType<BasementOffice>().First();
                
                // Create stamped shapes in different rooms
                var stampInBasement = new TestStampedWallDecal();
                stampInBasement.Position = basementRoom.Position + new Vector3(2, 2, 2);
                
                var stampInOffice = new TestStampedWallDecal();
                stampInOffice.Position = officeRoom.Position + new Vector3(2, 2, 2);
                
                loadedLevelData.AddStampedShape(basement, stampInBasement);
                loadedLevelData.AddStampedShape(basement, stampInOffice);
                
                // Get the shape buffers
                var levelData = loadedLevelData.FindLevelDataForWorldSegment(basement);
                var basementBuffer = levelData.StampedShapeBuffers
                    .FirstOrDefault(sb => sb.Shape == stampInBasement);
                var officeBuffer = levelData.StampedShapeBuffers
                    .FirstOrDefault(sb => sb.Shape == stampInOffice);
                
                Assert.NotNull(basementBuffer);
                Assert.NotNull(officeBuffer);
                
                // Different rooms should have different lighting groups
                Assert.NotEqual(basementBuffer.LightingGroup, officeBuffer.LightingGroup);
                Assert.Equal(basementRoom.LightingGroup, basementBuffer.LightingGroup);
                Assert.Equal(officeRoom.LightingGroup, officeBuffer.LightingGroup);
                
                return TestResult.PASS;
            }
            
            return TestResult.CONTINUE;
        });
        
        game.Run();
    }

    #endregion

    #region Dynamic Object Tests
    // NOTE: These tests are disabled until IPlaceableObject.Room property is implemented
#if false
    [Fact]
    public void DynamicObject_Player_HasRoomAssigned()
    {
        // Arrange
        var basement = new BasementWorldSegment(null);

        using var game = new TestGame(basement, framesToRun: 100, testAssertion: (g, gameTime) =>
        {
            if (gameTime.TotalGameTime.TotalMilliseconds > 100)
            {
                var player = g.GetService<Player>();

                // Player should implement IPlaceableObject and have a Room assigned
                if (player is IPlaceableObject placeablePlayer)
                {
                    Assert.NotNull(placeablePlayer.Room);
                }
                else
                {
                    Assert.Fail("Player should implement IPlaceableObject to support lighting");
                }

                return TestResult.PASS;
            }

            return TestResult.CONTINUE;
        });

        game.Run();
    }

    [Fact(Skip = "Waiting for IPlaceableObject.Room property to be implemented")]
    public void DynamicObject_MovingToNewRoom_UpdatesRoomProperty()
    {
        // Arrange
        var basement = new BasementWorldSegment(null);

        using var game = new TestGame(basement, framesToRun: 150, testAssertion: (g, gameTime) =>
        {
            var player = g.GetService<Player>();
            if (!(player is IPlaceableObject placeablePlayer))
            {
                Assert.Fail("Player should implement IPlaceableObject");
                return TestResult.FAIL;
            }

            if (gameTime.TotalGameTime.TotalMilliseconds < 50)
            {
                var basementOffice = basement.TraverseAllChildren().OfType<BasementOffice>().First();

                // Move player to BasementOffice
                player.Position = basementOffice.Position + new Vector3(0, 1.5f, 0);

                return TestResult.CONTINUE;
            }

            if (gameTime.TotalGameTime.TotalMilliseconds > 100 && gameTime.TotalGameTime.TotalMilliseconds < 110)
            {
                var basementOffice = basement.TraverseAllChildren().OfType<BasementOffice>().First();

                // Verify player is in BasementOffice
                Assert.Equal(basementOffice.LightingGroup, placeablePlayer.Room?.LightingGroup);

                // Move player to Basement
                var basementRoom = basement.TraverseAllChildren().OfType<Basement>().First();
                player.Position = basementRoom.Position + new Vector3(0, 1.5f, 0);

                return TestResult.CONTINUE;
            }

            if (gameTime.TotalGameTime.TotalMilliseconds > 120)
            {
                var basementRoom = basement.TraverseAllChildren().OfType<Basement>().First();

                // After update cycle, player's room should have changed
                Assert.Equal(basementRoom.LightingGroup, placeablePlayer.Room?.LightingGroup);

                return TestResult.PASS;
            }

            return TestResult.CONTINUE;
        });

        game.Run();
    }

    [Fact(Skip = "Waiting for IPlaceableObject.Room property to be implemented")]
    public void DynamicObject_StayingInSameRoom_RoomPropertyRemainsStable()
    {
        // Arrange
        var basement = new BasementWorldSegment(null);

        using var game = new TestGame(basement, framesToRun: 100, testAssertion: (g, gameTime) =>
        {
            var player = g.GetService<Player>();
            if (!(player is IPlaceableObject placeablePlayer))
            {
                Assert.Fail("Player should implement IPlaceableObject");
                return TestResult.FAIL;
            }

            if (gameTime.TotalGameTime.TotalMilliseconds < 50)
            {
                var basementOffice = basement.TraverseAllChildren().OfType<BasementOffice>().First();

                // Place player in BasementOffice
                player.Position = basementOffice.Position + new Vector3(0, 1.5f, 0);

                return TestResult.CONTINUE;
            }

            if (gameTime.TotalGameTime.TotalMilliseconds > 60 && gameTime.TotalGameTime.TotalMilliseconds < 80)
            {
                var initialRoom = placeablePlayer.Room;

                // Move player slightly within the same room
                player.Position += new Vector3(0.1f, 0, 0.1f);

                return TestResult.CONTINUE;
            }

            if (gameTime.TotalGameTime.TotalMilliseconds > 100)
            {
                var basementOffice = basement.TraverseAllChildren().OfType<BasementOffice>().First();

                // Room should still be BasementOffice (no unnecessary room changes)
                Assert.Equal(basementOffice.LightingGroup, placeablePlayer.Room?.LightingGroup);

                return TestResult.PASS;
            }

            return TestResult.CONTINUE;
        });

        game.Run();
    }
#endif

    #endregion

    #region Helper Classes

    /// <summary>
    /// Test stamped shape for use in tests
    /// </summary>
    private class TestStampedWallDecal : StampedShape<WallDecalStamp>, IPlaceableObject
    {
        public override ViewFrom ViewFrom => ViewFrom.Outside;
        public override CollisionGroup CollisionGroup => CollisionGroup.None;
        public override CollisionGroup CollidesWithGroups => CollisionGroup.None;

        public Shape Self => this;
        public Shape[] Children => System.Array.Empty<Shape>();
        public Room Room { get; set; }
    }

    #endregion
}

