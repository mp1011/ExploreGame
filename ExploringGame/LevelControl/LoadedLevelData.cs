using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Logics.Collision;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.LevelControl;

public class LoadedLevelData
{
    private readonly Game _game;
    private readonly ServiceContainer _serviceContainer;
    private readonly SetupColliderBodies _setupColliderBodies;
    private readonly Physics _physics;
    private readonly LoadedTextureSheets _loadedTextureSheets;

    public List<LevelData> LoadedSegments { get; } = new();

    public LoadedLevelData(Game game, SetupColliderBodies setupColliderBodies, Physics physics, 
        LoadedTextureSheets loadedTextureSheets, ServiceContainer serviceContainer)
    {
        _game = game;
        _physics = physics;
        _loadedTextureSheets = loadedTextureSheets;
        _serviceContainer = serviceContainer;
        _setupColliderBodies = setupColliderBodies;
    }

    public void Update(GameTime gameTime)
    {
        foreach(var segment in LoadedSegments)
            segment.Update(gameTime);
    }

    public void LoadSegment(WorldSegment worldSegment)
    {
        List<WorldSegment> addedSegments = new();
        addedSegments.Add(worldSegment);
        _serviceContainer.BindSingleton(worldSegment, worldSegment.GetType());

        foreach (var transition in worldSegment.Transitions)
        {
            var nextSegment = _serviceContainer.Get(transition.WorldSegmentType) as WorldSegment;
            addedSegments.Add(nextSegment);
            _serviceContainer.BindSingleton(nextSegment, nextSegment.GetType());
        }

        foreach (var addedSegment in addedSegments)
        {
            // Create waypoint graph before building so DebugMarkers are included
            addedSegment.WaypointGraph = new Logics.Pathfinding.WaypointGraph(addedSegment);
            
            var triangles = addedSegment.Build((QualityLevel)8); //todo, quality level
            var shapeBuffers = new ShapeBufferCreator(triangles, _loadedTextureSheets, _game.GraphicsDevice).Execute();
            var activeObjects = _serviceContainer.CreateControllers(addedSegment.TraverseAllChildren());

            var newLevelData = new LevelData(addedSegment, shapeBuffers, activeObjects);
            _setupColliderBodies.Execute(newLevelData.WorldSegment);
            newLevelData.Initialize();

            LoadedSegments.Add(newLevelData);
        }       
    }
    
    public void SwapActive()
    {
        //if (Next == null)
        //    return;

        //if (Current != null)
        //{
        //    Current.Stop();

        //    foreach (var body in Current.WorldSegment.TraverseAllChildren()
        //                                             .Where(p=>p.ColliderBodies != null)
        //                                             .SelectMany(p=>p.ColliderBodies))
        //    {                                                      
        //        _physics.Remove(body);
        //    }
        //}

        //Current = Next;

        //_setupColliderBodies.Execute(Current.WorldSegment);
        //Current.Initialize();

        //Next = null;
    }

    public LevelData FindLevelDataForWorldSegment(WorldSegment worldSegment)
    {
        return LoadedSegments.FirstOrDefault(ld => ld.WorldSegment == worldSegment);
    }

    public void AddStampedShape<TStamp>(WorldSegment worldSegment, StampedShape<TStamp> stampedShape)
        where TStamp : ShapeStamp
    {
        var levelData = FindLevelDataForWorldSegment(worldSegment);
        if (levelData == null)
        {
            throw new InvalidOperationException($"WorldSegment not found in loaded segments");
        }

        levelData.AddStampedShape(stampedShape);
    }

    public void AddWallDecal(WorldSegment worldSegment, GeometryBuilder.Shapes.Decals.WallDecal wallDecal)
    {
        AddStampedShape<GeometryBuilder.Shapes.Decals.WallDecalStamp>(worldSegment, wallDecal);
    }
}

