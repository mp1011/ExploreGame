using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Services;
using System.Linq;

namespace ExploringGame.Logics.Collision;

public class SetupColliderBodies
{
    private Physics _physics;

    public SetupColliderBodies(Physics physics)
    {
        _physics = physics;
    }

    public void Execute(WorldSegment worldSegment)
    {
        foreach(var shape in worldSegment.TraverseAllChildren())
        {
            if (shape.ColliderMaker == null)
                continue;

            shape.ColliderBodies = shape.ColliderMaker.CreateColliders(_physics).ToArray();
        }
    }
}
