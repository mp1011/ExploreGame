using ExploringGame;
using ExploringGame.Audio;
using ExploringGame.GameDebug;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Story;
using ExploringGame.Story.Scene01.Act02;
using ExploringGame.Testing;
using System;
using System.Diagnostics;
using System.Linq;

// character test
AudioService.Enabled = true;
ExploringGame.GameDebug.Debug.NoScene = false;
ExploringGame.GameDebug.Debug.UseDebugScene = true;
using var game = new Game1(new SingleSegmentGroup(TestMaps.NpcTest()));


//AudioService.Enabled = true;
//ExploringGame.GameDebug.Debug.NoScene = true;
//using var game = new Game1(new SingleSegmentGroup(TestMaps.JunctionTest(ExploringGame.GeometryBuilder.HAlign.Left, ExploringGame.GeometryBuilder.DoorDirection.Push)));


//scene test
//AudioService.Enabled = true;
//ExploringGame.GameDebug.Debug.NoScene = false;
//ExploringGame.GameDebug.Debug.UseDebugScene = false;
//ExploringGame.GameDebug.Debug.SceneManagerDebugInit = new Action<SceneManager>(p => p.FastForwardToAct<ActTwo>());
//using var game = new Game1(new HomeWorldSegmentGroup());


//var testMap = TestMaps.CircleCutoutTest();
//var testMap = TestMaps.SkyboxTest();
//var testMap = TestMaps.SkyDomeTest();
//using var game = new Game1(testMap);

//var testMap = TestMaps.PathfindingTest();
//testMap.PlayerStart = testMap.TraverseAllChildren().OfType<Room>().First(p => p.Tag == "Room C").Position;
//LightIntensity.DefaultAmbientLight = LightIntensity.Bright;
//using var game = new Game1(testMap);


game.Run();
