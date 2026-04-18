using ExploringGame.GeometryBuilder.Shapes.SimpleShapes;
using ExploringGame.Services;
using ExploringGame.Texture;
using System.Numerics;

namespace ExploringGame.GeometryBuilder.Shapes.Structures;

public class BasementWindow : Room
{
    public static readonly float Width = Measure.Feet(3f);
    public static readonly float Height = Measure.Feet(1.5f);
    private readonly Room _basementRoom, _yardArea;

    public override Theme Theme => _basementRoom.Theme;

    public BasementWindow(Room basementRoom, Room yardArea, Side basementRoomWindowSide, HAlign windowAlign, float? placement = null) 
        : base(basementRoom.WorldSegment)
    {
        Size = Vector3.One;
        _basementRoom = basementRoom;
        _yardArea = yardArea;
 
        basementRoom.AddConnectingRoomWithJunction(this, _yardArea, basementRoomWindowSide, windowAlign, placement.GetValueOrDefault(), adjustPlacement: false);

        SetSide(Side.Bottom, basementRoom.GetSide(Side.Top) - Height);
        SetSideUnanchored(Side.Top, basementRoom.GetSide(Side.Top));

        // glass pane - thin transparent glass in the window opening
        var glassPane = AddChild(new GlassPane());
        glassPane.AdjustShape()
            .SetAxis(basementRoomWindowSide.GetAxis(), 0.02f) // Very thin glass
            .SetAxis(basementRoomWindowSide.GetPerpendicularAxis(), Width)
            .SetAxis(Axis.Y, Height);
        glassPane.Place().AtParent(); // Center in the window opening
    }
}
