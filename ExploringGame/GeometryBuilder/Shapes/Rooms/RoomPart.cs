namespace ExploringGame.GeometryBuilder.Shapes.Rooms;

public class RoomPart : Room
{
    public Room Main { get; }

    public override Side OmitSides => Main.OmitSides;

    public RoomPart(Room main, float? height = null, float? width = null, float? depth = null)
        : base(main.WorldSegment, theme: main.Theme)
    {
        Main = main;
        Position = main.Position;
        Size = main.Size;

        if (height.HasValue)
            Height = height.Value;

        if (width.HasValue)
            Width = width.Value;

        if (depth.HasValue)
            Depth = depth.Value;
    }

    public override string ToString() => $"{Tag ?? Main.ToString()} (part)";

    public override Room LightingGroup => Main;
}
