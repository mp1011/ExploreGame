using ExploringGame.Entities;
using ExploringGame.GeometryBuilder;
using ExploringGame.Story;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Rendering.RenderPasses;

public class DialogueRenderPass : IRenderPass
{
    private const float _horizontalPaddingPercent = 0.1f;
    private const float _bottomPaddingPercent = 0.1f;
    private const int _maxLines = 3;

    private readonly DialogueManager _dialogueManager;
    private SpriteFont _font;
    private SpriteBatch _spriteBatch;

    private float _leftMargin, _rightMargin, _bottomMargin, _lineHeight;

    public ShapeBufferType ShapeBufferType => ShapeBufferType.UI;


    public DialogueRenderPass(DialogueManager dialogueManager)
    {
        _dialogueManager = dialogueManager;
    }

    private void UpdateMargins(GraphicsDevice graphicsDevice)
    {
        _leftMargin = graphicsDevice.Viewport.Width * _horizontalPaddingPercent;
        _rightMargin = graphicsDevice.Viewport.Width - _leftMargin;
        _bottomMargin = graphicsDevice.Viewport.Height - (graphicsDevice.Viewport.Height * _bottomPaddingPercent);
        _lineHeight = _font.MeasureString("T").Y;
    }

    public void Draw(GraphicsDevice graphicsDevice, IReadOnlyList<ShapeBuffer> shapeBuffers, Matrix view, Matrix projection)
    {
        if (!_dialogueManager.HasDialogue)
            return;

        UpdateMargins(graphicsDevice);

        _spriteBatch.Begin(blendState: BlendState.NonPremultiplied);

        float linesHeight = _font.MeasureString("T").Y * _maxLines;

        var cursor = new Vector2(graphicsDevice.Viewport.Width * _horizontalPaddingPercent, _bottomMargin - linesHeight);

        int index = 0;
        foreach (var word in _dialogueManager.PreviousWords)
        {
            cursor = DrawWord(word, cursor);
            if (cursor.X == 0 && cursor.Y == 0)
            {
                _dialogueManager.LimitReached(index);
                break;
            }
            index++;
        }

        cursor = DrawWord(_dialogueManager.CurrentWord, cursor, _dialogueManager.LetterIndex+1);

        if (cursor.X == 0 && cursor.Y == 0)
            _dialogueManager.LimitReached();

        _spriteBatch.End();
    }


    private Vector2 DrawWord(Word word, Vector2 cursor, int maxLetters = int.MaxValue)
    {
        //can this word fit?
        var measureCursor = MeasureWord(word, cursor);
        if(measureCursor.X > _rightMargin)
        {
            cursor.X = _leftMargin;
            cursor.Y += _lineHeight;

            if (cursor.Y >= _bottomMargin)
                return Vector2.Zero;
        }

        foreach (var letter in word.Letters.Take(maxLetters))
            cursor = DrawLetter(letter, word.Color, cursor);

        cursor += new Vector2(_font.MeasureString(" ").X, 0f);

        return cursor;
    }

    private Vector2 MeasureWord(Word word, Vector2 cursor)
    {
        foreach (var letter in word.Letters)
            cursor = MeasureLetter(letter, cursor);

        return cursor;
    }

    private Vector2 DrawLetter(string character, Color color, Vector2 cursor)
    {
        var g = _font.GetGlyphs()[character[0]];
        var size = g.Cropping.Size.ToVector2();

        for (int x = -2; x <= 2; x++)
        {
            for (int y = -2; y <= 2; y++)
            {
                float alpha = 1.0f - (Math.Abs(x) + Math.Abs(y)) / 6f;
                _spriteBatch.DrawString(_font, character, cursor - new Vector2(x,y), origin: Vector2.Zero, rotation: 0f, 
                    color: color * alpha, 
                    scale: 1.0f, effects: SpriteEffects.None, layerDepth: 1f);
            }
        }

        _spriteBatch.DrawString(_font, character, cursor, origin: Vector2.Zero, rotation: 0f, color: Color.White, scale: 1.0f, effects: SpriteEffects.None, layerDepth: 1f);

        return cursor + new Vector2(g.WidthIncludingBearings, 0);
    }

    private Vector2 MeasureLetter(string character, Vector2 cursor)
    {
        var g = _font.GetGlyphs()[character[0]];
        var size = g.Cropping.Size.ToVector2();
        return cursor + new Vector2(g.WidthIncludingBearings, 0);
    }

    public void LoadContent(Game game, LoadedTextureSheets textureSheets)
    {
        _font = game.Content.Load<SpriteFont>("DialogueFont");
        _spriteBatch = new SpriteBatch(game.GraphicsDevice);
    }
}
