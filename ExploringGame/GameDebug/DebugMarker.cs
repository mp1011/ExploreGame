using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.GameDebug;

public class DebugMarker : PlaceableShape
{
    private static readonly TextureInfo GreenTexture = new TextureInfo(Color.Green, TextureKey.Wall);

    public override CollisionGroup CollisionGroup => CollisionGroup.None;
    public override CollisionGroup CollidesWithGroups => CollisionGroup.None;

    public bool IsTargeted { get; set; }

    public DebugMarker(Vector3 position)
    {
        Width = 0.2f;
        Height = 0.2f;
        Depth = 0.2f;
        Position = position;
        MainTexture = GreenTexture;
    }


    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public override Matrix GetWorldMatrix()
    {
        // Scale to 0 when hidden, stretch Y axis when targeted
        float visibility = Debug.ShowWaypointMarkers ? 1.0f : 0.0f;
        float yScale = IsTargeted ? 2.0f : 1.0f;
        
        var scale = Matrix.CreateScale(visibility, visibility * yScale, visibility);
        var rotation = Rotation?.AsMatrix() ?? Matrix.Identity;
        return scale * rotation * Matrix.CreateTranslation(Position);
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid();
    }
}
