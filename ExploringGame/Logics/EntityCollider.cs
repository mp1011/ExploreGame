using ExploringGame.Entities;
using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;

namespace ExploringGame.Logics;

internal class EntityCollider
{
    public Shape CurrentRoom { get; set; }

    public IWithPosition Entity { get; set; }

    public void Update()
    {
        // super basic collision, to be improved later
        var position = Entity.Position;
        float padding = 0.2f;
        position.X = MathHelper.Clamp(position.X, CurrentRoom.GetSide(Side.West) + padding, CurrentRoom.GetSide(Side.East) - padding);
        position.Z = MathHelper.Clamp(position.Z, CurrentRoom.GetSide(Side.North) + padding, CurrentRoom.GetSide(Side.South) - padding);
        Entity.Position = position;
    }
}
