using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System.Linq;

namespace ExploringGame.GeometryBuilder.Shapes.Furniture;

/// <summary>
/// Mini "room" which joins an interior room to an exterior room, creating a window opening.
/// The junction itself represents the windowsill.
/// </summary>
public class Window : Room
{
    private Side _wallSide;
    private Room _parentRoom;
    private Room _exteriorRoom;

    public Window(Room room, Side wallSide, float width, float height, HAlign align = HAlign.Center, float offset = 0f) : base(room.WorldSegment)
    {
        _parentRoom = room;
        _wallSide = wallSide;

        // Window sill height above floor (e.g., 2.5 feet)
        float sillHeight = Measure.Feet(2.0f);

        // Set window dimensions and vertical placement
        if (wallSide.GetAxis() == Axis.Z)
        {
            Width = width;
            Depth = 0.2f;  // Thin depth for the windowsill
            Height = height;
        }
        else
        {
            Depth = width;
            Width = 0.2f;  // Thin width for the windowsill
            Height = height;
        }

        MainTexture = new TextureInfo(Color.White, TextureKey.Wall);

        // Offset window bottom above the floor
        SetSide(Side.Bottom, room.GetSide(Side.Bottom) + sillHeight);

        // Create dummy exterior room (larger than window in both axes from viewer's perspective)
        float exteriorOpeningWidth = (wallSide.GetAxis() == Axis.Z) ? Width : Depth;
        float exteriorOpeningHeight = Height;
        float exteriorThickness = 0.4f;
        float exteriorExtra = 0.8f; // How much larger the exterior is than the window

        float exteriorRoomWidth, exteriorRoomDepth, exteriorRoomHeight;
        if (wallSide.GetAxis() == Axis.Z)
        {
            exteriorRoomWidth = exteriorOpeningWidth + exteriorExtra;
            exteriorRoomDepth = Depth + exteriorThickness;
            exteriorRoomHeight = exteriorOpeningHeight + exteriorExtra;
        }
        else
        {
            exteriorRoomWidth = Width + exteriorThickness;
            exteriorRoomDepth = exteriorOpeningWidth + exteriorExtra;
            exteriorRoomHeight = exteriorOpeningHeight + exteriorExtra;
        }

        _exteriorRoom = new Room(room.WorldSegment, exteriorRoomWidth, exteriorRoomDepth, exteriorRoomHeight);
        _exteriorRoom.MainTexture = new TextureInfo(Color.White);
        _exteriorRoom.FixedAmbientLight = LightIntensity.VeryBright;

        // Placement and connection logic
        Position = _parentRoom.Position;
        this.Place().OnSideOuter(_wallSide, _parentRoom);

       

        // Place exterior just outside the window
        _exteriorRoom.Position = Position;
        _exteriorRoom.Place().OnSideOuter(_wallSide, this);

        // Connect parent room, window, and exterior
        _parentRoom.AddConnectingRoomWithJunction(this, _exteriorRoom, _wallSide, align, offset);

        // Ensure window sits above the floor
        float parentBottom = _parentRoom.GetSide(Side.Bottom);
        SetSide(Side.Bottom, parentBottom + sillHeight);
        SetSide(Side.Top, parentBottom + sillHeight + Height);
    }

    public override string ToString()
    {
        return "Window: " + string.Join(" - ", RoomConnections.Select(p => p.GetOtherRoom(this).ToString()).ToArray());
    }
}
