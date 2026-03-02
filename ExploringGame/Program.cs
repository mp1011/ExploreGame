using ExploringGame;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Testing;

using var game = new Game1(new BasementWorldSegment(null));
// LightIntensity.DefaultAmbientLight = LightIntensity.Bright;
game.Run();
