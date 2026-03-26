using ExploringGame;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Testing;
using System.Linq;

using var game = new Game1(new BasementWorldSegment());
// using var game = new Game1(new UpstairsWorldSegment());

//var testMap = TestMaps.PathfindingTest();
//testMap.PlayerStart = testMap.TraverseAllChildren().OfType<Room>().First(p => p.Tag == "Room C").Position;
//LightIntensity.DefaultAmbientLight = LightIntensity.Bright;
//using var game = new Game1(testMap);


game.Run();
