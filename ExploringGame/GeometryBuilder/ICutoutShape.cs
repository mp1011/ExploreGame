namespace ExploringGame.GeometryBuilder;

/// <summary>
/// Shape which cuts out a portion of its parent
/// </summary>
public interface ICutoutShape
{
    Side ParentCutoutSide { get; }
    Triangle[] Build();
}
