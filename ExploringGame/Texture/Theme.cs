using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ExploringGame.Texture;

public class Theme
{
    public TextureInfo MainTexture { get; set; } = new TextureInfo(Color.Magenta);
    public Dictionary<Side, TextureInfo> SideTextures { get; set; } = new();

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

public class BasementRoomTheme : Theme
{
    public BasementRoomTheme()
    {
        SideTextures[Side.Top] = new TextureInfo(TextureKey.Ceiling);
        SideTextures[Side.Bottom] = new TextureInfo(TextureKey.Floor);
        MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);
    }
}