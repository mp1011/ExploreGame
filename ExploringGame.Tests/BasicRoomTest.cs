using ExploringGame.Entities;
using ExploringGame.Logics;
using ExploringGame.Testing;
using ExploringGame.Tests.TestHelpers;
using Microsoft.Xna.Framework;
using Xunit;

namespace ExploringGame.Tests;

public class BasicRoomTest
{
    [Fact]
    public void Player_WalksForward_StaysInRoom()
    {
        using var g = new TestGame(TestMaps.EmptyRoom(), framesToRun: 5000);

        g.MockPlayerInput.AddKeyPress(frame: 1, key: GameKey.Forward);
        g.Run();

        var player = g.GetService<Player>();

        var distanceToCenter = (player.Position - Vector3.Zero).Length();
        Assert.True(distanceToCenter > 3f && distanceToCenter < 4f);
    }

    [Fact]
    public void Player_StaysStill()
    {
        using var g = new TestGame(TestMaps.EmptyRoom(), framesToRun: 5000);
        g.Run();

        var player = g.GetService<Player>();

        var distanceToCenter = (player.Position - Vector3.Zero).Length();
        Assert.True(distanceToCenter < 2f);
    }
}
