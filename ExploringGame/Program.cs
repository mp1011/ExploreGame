using ExploringGame;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Testing;

using var game = new Game1(new BasementWorldSegment(null));
game.Run();
