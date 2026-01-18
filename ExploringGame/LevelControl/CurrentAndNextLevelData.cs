using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Logics.Collision;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System.Linq;

namespace ExploringGame.LevelControl;

public class CurrentAndNextLevelData
{
    private readonly Game _game;
    private readonly ServiceContainer _serviceContainer;
    private readonly SetupColliderBodies _setupColliderBodies;
    private readonly Physics _physics;
    private readonly LoadedTextureSheets _loadedTextureSheets;

    public LevelData Next { get; private set; }

    public LevelData Current { get; private set; }

    public CurrentAndNextLevelData(Game game, SetupColliderBodies setupColliderBodies, Physics physics, 
        LoadedTextureSheets loadedTextureSheets, ServiceContainer serviceContainer)
    {
        _game = game;
        _physics = physics;
        _loadedTextureSheets = loadedTextureSheets;
        _serviceContainer = serviceContainer;
        _setupColliderBodies = setupColliderBodies;
    }

    public void PrepareNextSegment(WorldSegment worldSegment)
    {
        var triangles = worldSegment.Build((QualityLevel)8); //todo, quality level
        var shapeBuffers = new ShapeBufferCreator(triangles, _loadedTextureSheets, _game.GraphicsDevice).Execute();
        var activeObjects = _serviceContainer.CreateControllers(worldSegment.TraverseAllChildren());
        Next = new LevelData(worldSegment, shapeBuffers, activeObjects);
    }

    public void SwapActive()
    {
        if (Next == null)
            return;

        if (Current != null)
        {
            Current.Stop();

            foreach (var body in Current.WorldSegment.TraverseAllChildren()
                                                     .Where(p=>p.ColliderBodies != null)
                                                     .SelectMany(p=>p.ColliderBodies))
            {                                                      
                _physics.Remove(body);
            }
        }

        Current = Next;

        _setupColliderBodies.Execute(Current.WorldSegment);
        Current.Initialize();

        Next = null;
    }
}
