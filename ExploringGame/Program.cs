using ExploringGame;
using ExploringGame.Audio;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Testing;
using System.Diagnostics;
using System.Linq;

// character test
//AudioService.Enabled = true;
//ExploringGame.GameDebug.Debug.NoScene = true;
//using var game = new Game1(new SingleSegmentGroup(TestMaps.NpcTest()));


//AudioService.Enabled = true;
//ExploringGame.GameDebug.Debug.NoScene = true;
//using var game = new Game1(new SingleSegmentGroup(TestMaps.JunctionTest(ExploringGame.GeometryBuilder.HAlign.Left, ExploringGame.GeometryBuilder.DoorDirection.Push)));


//scene test
AudioService.Enabled = true;
ExploringGame.GameDebug.Debug.NoScene = false;
using var game = new Game1(new HomeWorldSegmentGroup());


//var testMap = TestMaps.CircleCutoutTest();
//var testMap = TestMaps.SkyboxTest();
//var testMap = TestMaps.SkyDomeTest();
//using var game = new Game1(testMap);

//var testMap = TestMaps.PathfindingTest();
//testMap.PlayerStart = testMap.TraverseAllChildren().OfType<Room>().First(p => p.Tag == "Room C").Position;
//LightIntensity.DefaultAmbientLight = LightIntensity.Bright;
//using var game = new Game1(testMap);


game.Run();
