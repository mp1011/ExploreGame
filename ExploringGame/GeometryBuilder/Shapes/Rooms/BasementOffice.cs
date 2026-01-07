using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms;

public class BasementOffice : Room
{
    public override Theme Theme => new BasementRoomTheme();
    public BasementOffice(WorldSegment worldSegment) : base(worldSegment)
    {        
        Width =  8f;
        Height = OfficeDesk.DeskHeight + Measure.Inches(19);
        Depth = OfficeDesk.DeskWidth + Measure.Inches(39 + 39 + 36);
        SetSide(Side.Bottom, 0f);

        var exit = Copy(depth: Measure.Inches(39), width: Measure.Inches(50));
        AddConnectingRoom(new RoomConnection(this, exit, Side.West, Align: HAlign.Right));

        var eastPart = Copy(width: 2.0f);
        AddConnectingRoom(new RoomConnection(this, eastPart, Side.East, 0.5f));

        var eastPart2 = Copy(depth: 2.0f, width: 2.0f);
        eastPart.AddConnectingRoom(new RoomConnection(eastPart, eastPart2, Side.North, 0.5f));

        var oilTankRoom = new OilTankRoom(worldSegment);
        oilTankRoom.Height = Height;
        oilTankRoom.Width = Width - 1.9f;
        oilTankRoom.Depth = 1.9f;

        eastPart2.AddConnectingRoomWithJunction(
            new DoorJunction(
                worldSegment: worldSegment,
                doorClose: new Angle(Side.North),
                doorOpen: new Angle(Side.East),
                hingePosition: HAlign.Left,
                height: Height,
                depth: Measure.Inches(30.5f),
                width: 0.2f),
            oilTankRoom,
            Side.West);

        var ceilingBar = AddChild(new Box(TextureKey.Ceiling));
        ceilingBar.AdjustShape().From(this).SliceY(0.1f, 0.4f).SliceZ(4.0f, 0.4f);
        ceilingBar.SetSideUnanchored(Side.East, eastPart.GetSide(Side.East));

        var closet1 = new BasementCloset(this, Side.East);
        closet1.Place().OnFloor().OnSideInner(Side.SouthWest);

        var closet2 = new BasementCloset(eastPart, Side.West);
        closet2.Place().OnFloor().OnSideInner(Side.SouthEast);

        var desk1 = new OfficeDesk(this);
        desk1.Place().OnFloor().OnSideInner(Side.West);
        desk1.Rotation = Rotation.YawFromDegrees(-90);
        desk1.X -= 0.9f;
        desk1.Z += 0.65f;
        AddChild(desk1);

        var desk2 = new OfficeDesk(eastPart);
        desk2.Place().OnFloor().OnSideInner(Side.East);
        desk2.X += 0.9f;
        desk2.Z += 0.65f;
        desk2.Rotation = Rotation.YawFromDegrees(90);
      
        var fireplace = new ElectricFireplace(this);
        fireplace.Place().OnFloor().OnSideInner(Side.North);
    }
}
