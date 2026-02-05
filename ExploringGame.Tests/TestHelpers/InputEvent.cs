using ExploringGame.Logics;
using Microsoft.Xna.Framework;

namespace ExploringGame.Tests.TestHelpers;

public record InputEvent(int FrameNumber, GameKey Key, bool IsPressed, Vector2? MouseDelta);
