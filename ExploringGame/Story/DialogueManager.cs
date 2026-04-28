using ExploringGame.Logics.Controllers;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Story;

public record Word(string[] Letters, Color Color)
{
    public Word(string text, Color color) : this(text.ToCharArray().Select(p=>p.ToString()).ToArray(), color)
    {}
}

public class DialogueManager
{
    private static readonly TimeSpan NextLetterTime = TimeSpan.FromMilliseconds(50);

    private Queue<DialogueEntry> _lines = new();

    private TimedAction _nextLetter;

    private Word[] _currentWords = null;
    private int _wordIndex = 0;
    private int _letterIndex = 0;

    public bool HasDialogue => _currentWords != null;

    public IEnumerable<Word> PreviousWords => _currentWords.Take(_wordIndex);

    public Word CurrentWord => _currentWords[_wordIndex];
    public int LetterIndex => _letterIndex;

    public void Enqueue(DialogueEntry entry) => _lines.Enqueue(entry);

    public DialogueManager()
    {
        _nextLetter = new TimedAction(NextLetterTime, NextLetter);
    }

    private void NextLetter()
    {
        if (_currentWords == null)
            return;

        var word = _currentWords[_wordIndex];

        var newLetterIndex = _letterIndex + 1;
        int newWordIndex = _wordIndex;

        if (newLetterIndex >= word.Letters.Length)
        {
            newWordIndex++;
            newLetterIndex = 0;
        }
        
        if(newWordIndex >= _currentWords.Length)
        {
            newWordIndex = _currentWords.Length - 1;
            newLetterIndex = word.Letters.Length - 1;
        }

        _letterIndex = newLetterIndex;
        _wordIndex = newWordIndex;
    }

    public void Update(GameTime gameTime)
    {
        if(_currentWords == null && _lines.Any())
        {
            var entry = _lines.Dequeue();
            _currentWords = LoadWords(entry).ToArray();
            _wordIndex = 0;
            _letterIndex = 0;
        }

        _nextLetter.Update(gameTime);
    }

    private static IEnumerable<Word> LoadWords(DialogueEntry dialogueEntry)
    {
        return dialogueEntry.Line.Split(' ', System.StringSplitOptions.RemoveEmptyEntries)
                                 .Select(p => new Word(p, dialogueEntry.Actor.TextColor));
    }
}
