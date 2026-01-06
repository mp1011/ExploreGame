using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.Appliances;

public class OilTank : PlaceableShape
{
    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public override IColliderMaker ColliderMaker => ColliderMakers.BoundingBox(this);

    public OilTank(Shape room)
    {
        room.AddChild(this);
        Width = 5.0f;
        Height = 2.0f;
        Depth = 1.3f;
        Position = room.Position;
        MainTexture = new TextureInfo(Key: TextureKey.Ceiling, Color: Color.DarkRed);

        SideTextures[Side.West] = new TextureInfo(Key: TextureKey.Ceiling, Color: Color.LightGray);
        SideTextures[Side.East] = new TextureInfo(Key: TextureKey.Ceiling, Color: Color.LightBlue);
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return TriangleMaker.BuildCylinder(this, detail: 500, Axis.X);
    }
}
