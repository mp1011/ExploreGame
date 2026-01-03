using ExploringGame.GeometryBuilder;
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
        var colliderMakers = worldSegment.TraverseAllChildren()
            .Select(p => p.ColliderMaker)
            .Where(p => p != null)
            .ToArray();

        foreach (var maker in colliderMakers)
            maker.CreateColliders(_physics);
    }
}
