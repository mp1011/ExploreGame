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

        var eastPart = new Room();
        eastPart.Height = Height;
        eastPart.Depth = Depth + 2.0f;
        eastPart.Width = 2.0f;
        eastPart.SetSide(Side.Bottom, 0f);

        eastPart.SideTextures[Side.Top] = new TextureInfo(TextureKey.Ceiling);
        eastPart.SideTextures[Side.Bottom] = new TextureInfo(TextureKey.Floor);
        eastPart.MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);

        AddConnectingRoom(new RoomConnection(eastPart, Side.East, 0.37f));

        worldSegment.AddChild(this);
        worldSegment.AddChild(exit);
        worldSegment.AddChild(eastPart);
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
