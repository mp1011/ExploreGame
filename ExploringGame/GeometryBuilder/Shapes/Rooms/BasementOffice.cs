using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms;

public class BasementOffice : Room
{
    public BasementOffice(WorldSegment worldSegment)
    {
        Width =  8f;
        Height = OfficeDesk.DeskHeight + Measure.Inches(19);
        Depth = OfficeDesk.DeskWidth + Measure.Inches(39 + 39 + 36);
        SetSide(Side.Bottom, 0f);

        SideTextures[Side.Top] = new TextureInfo(TextureKey.Ceiling);
        SideTextures[Side.Bottom] = new TextureInfo(TextureKey.Floor);
        MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);

        #region room sections
        var exit = new Room();
        exit.Depth = Measure.Inches(39);
        exit.Width = Measure.Inches(50);
        exit.Height = Height;
        exit.SideTextures[Side.Top] = new TextureInfo(TextureKey.Ceiling);
        exit.SideTextures[Side.Bottom] = new TextureInfo(TextureKey.Floor);
        exit.MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);
        AddConnectingRoom(new RoomConnection(exit, Side.West, 0.9f));

        var eastPart = Copy();
        eastPart.Height = Height;
        eastPart.Depth = Depth;
        eastPart.Width = 2.0f;
        eastPart.SetSide(Side.Bottom, 0f);
        AddConnectingRoom(new RoomConnection(eastPart, Side.East, 0.5f));

        var eastPart2 = Copy();
        eastPart2.Height = Height;
        eastPart2.Depth = 2.0f;
        eastPart2.Width = 2.0f;
        eastPart2.SetSide(Side.Bottom, 0f);
        eastPart.AddConnectingRoom(new RoomConnection(eastPart2, Side.North, 0.5f));

        var oilTankRoom = new OilTankRoom();
        oilTankRoom.Height = Height;
        oilTankRoom.Width = Width - 1.9f;
        oilTankRoom.Depth = 1.9f;

        // todo this needs to be cleaner
        var junction = new DoorJunction(
            doorClose: new Angle(Side.North), 
            doorOpen: new Angle(Side.East), 
            hingePosition: HingePosition.Left,
            height: Height);

        junction.Depth = Measure.Inches(30.5f);
        junction.Width = 0.1f;

        eastPart2.AddConnectingRoom(new RoomConnection(junction, Side.West, 0.5f));
        junction.AddConnectingRoom(new RoomConnection(oilTankRoom, Side.West, 0.5f));

        worldSegment.AddChild(this);
        worldSegment.AddChild(exit);
        worldSegment.AddChild(eastPart);
        worldSegment.AddChild(eastPart2);
        worldSegment.AddChild(oilTankRoom);
        worldSegment.AddChild(junction);

        #endregion

        var ceilingBar = AddChild(new Box());
        ceilingBar.MainTexture = new TextureInfo(TextureKey.Ceiling);
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
