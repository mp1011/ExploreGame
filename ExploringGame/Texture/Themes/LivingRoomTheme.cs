using ExploringGame.GeometryBuilder;

namespace ExploringGame.Texture;

public class LivingRoomTheme : UpstairsHallTheme
{
    public LivingRoomTheme()
    {
        SideTextures[Side.North] = new TextureInfo(TextureKey.Wood);
    }
}
