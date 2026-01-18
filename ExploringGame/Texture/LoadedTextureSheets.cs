using System.Collections.Generic;

namespace ExploringGame.Texture;

public class LoadedTextureSheets
{
    private Dictionary<TextureSheetKey, TextureSheet> _sheets = new Dictionary<TextureSheetKey, TextureSheet>();

    public void AddTexture(TextureSheet textureSheet)
    {
        _sheets[textureSheet.Key] = textureSheet;
    }

    public TextureSheet Get(TextureSheetKey key) => _sheets[key];

    public IEnumerable<TextureSheet> LoadedTextures => _sheets.Values;
}
