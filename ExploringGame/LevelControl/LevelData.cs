using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Logics.Collision;
using ExploringGame.Rendering;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.LevelControl;

public class LevelData
{
    public IActiveObject[] ActiveObjects { get; } 

    public ShapeBuffer[] ShapeBuffers { get; }

    public bool Initialized { get; private set; }
    public WorldSegment WorldSegment { get; }

    public LevelData(WorldSegment worldSegment, ShapeBuffer[] shapeBuffers, IActiveObject[] activeObjects)
    {
        WorldSegment = worldSegment;
        ShapeBuffers = shapeBuffers;
        ActiveObjects = activeObjects;
        Initialized = false;
    }

    public void Initialize()
    {
        if (Initialized)
            return;

        foreach (var obj in ActiveObjects)
            obj.Initialize();

        Initialized = true;        
    }

    
    public void Update(GameTime gameTime)
    {
        foreach (var obj in ActiveObjects)
            obj.Update(gameTime);
    }
}
