using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ExploringGame.Texture;

public class Theme
{
    public TextureInfo MainTexture { get; set; } = new TextureInfo(Color.Magenta);
    public Dictionary<Side, TextureInfo> SideTextures { get; set; } = new();

    public virtual TextureSheetKey TextureSheetKey => TextureSheetKey.Basement;

    public Theme() { }

    public Theme(TextureKey key)
    {
        MainTexture = new TextureInfo(Key: key);
    }

    public Theme(Color color)
    {
        MainTexture = new TextureInfo(Color: color);
    }

    public void CopyFrom(Theme other)
    {
        MainTexture = other.MainTexture;
        SideTextures = new Dictionary<Side, TextureInfo>(other.SideTextures);
    }   

    public TextureInfo TextureInfoForSide(Side side)
    {
        if (SideTextures.TryGetValue(side, out var texture))
            return texture;
        else
            return MainTexture;
    }

    public static Theme Missing { get => new Theme { MainTexture = new TextureInfo(Color.Magenta) }; }
}

public class TestTheme : Theme
{
    public override TextureSheetKey TextureSheetKey => TextureSheetKey.Basement;

    public TestTheme()
    {
        MainTexture = new TextureInfo(Color.Pink, TextureKey.Ceiling);
        SideTextures[Side.Top] = new TextureInfo(Color.Gray, TextureKey.Ceiling);
        SideTextures[Side.Bottom] = new TextureInfo(Color.Purple, TextureKey.Ceiling);
    }
}

public class BasementRoomTheme : Theme
{
    public override TextureSheetKey TextureSheetKey => TextureSheetKey.Basement;


    public BasementRoomTheme()
    {
        SideTextures[Side.Top] = new TextureInfo(TextureKey.Ceiling);
        SideTextures[Side.Bottom] = new TextureInfo(TextureKey.Floor);
        MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);
    }
}

public class UpstairsHallTheme : Theme
{
    public override TextureSheetKey TextureSheetKey => TextureSheetKey.Upstairs;

    public UpstairsHallTheme()
    {
        SideTextures[Side.Top] = new TextureInfo(Color.White, TextureKey.Plain);      
        SideTextures[Side.Bottom] = new TextureInfo(Color.Brown, TextureKey.Floor);
        MainTexture = new TextureInfo(Color.LightGray, TextureKey.Plain);
    }
}

public class KitchenTheme : Theme
{
    public override TextureSheetKey TextureSheetKey => TextureSheetKey.Upstairs;

    public KitchenTheme()
    {
        SideTextures[Side.Top] = new TextureInfo(Color.White, TextureKey.Plain);
        SideTextures[Side.Bottom] = new TextureInfo(Color.White, TextureKey.Tile, TextureStyle.XZTile, TileSize: 1f);
        MainTexture = new TextureInfo(Color.LightGray, TextureKey.Plain);
    }
}

public class BathroomTheme : Theme
{
    public override TextureSheetKey TextureSheetKey => TextureSheetKey.Upstairs;

    public BathroomTheme()
    {
        MainTexture = new TextureInfo(Color.White, TextureKey.Plain);
    }
}