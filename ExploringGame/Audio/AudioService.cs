using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace ExploringGame.Audio;


public enum SoundEffectKey
{
     DoorOpen,
     DoorClose,
     TextBeep,
     CreepyLoop
}
public class AudioService : IDisposable
{
    /// <summary>
    /// for testing, not yet robust enough to use live during gameplay
    /// </summary>
    public static bool Enabled { get; set; }

    private Dictionary<SoundEffectKey, SoundEffect> _effects = new();

    private List<ActiveAudio> _activeSounds = new();

    private bool disposedValue;

    public void LoadContent(ContentManager contentManager)
    {
        _effects.Clear();

        if (!Enabled)
            return;

        foreach(SoundEffectKey key in Enum.GetValues(typeof(SoundEffectKey)))
        {
            _effects[key] = contentManager.Load<SoundEffect>($"Sound/{key}");
        }
    }

    public TimeSpan GetDuration(SoundEffectKey key)
    {
        return _effects[key].Duration;
    }

    public void AddActiveSound(ActiveAudio sound)
    {
        _activeSounds.Add(sound);
    }

    public void Update(GameTime gameTime)
    {
        foreach(var sound in _activeSounds)
        {
            sound.Update(gameTime);
        }
    }

    public SoundEffectInstance CreateInstance(SoundEffectKey key)
    {
        return _effects[key].CreateInstance();
    }

    public void Play(SoundEffectKey key)
    {
        if (!Enabled)
            return;

        _effects[key].Play();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                foreach (var value in _effects.Values)
                    value.Dispose();

                _effects.Clear();
            }


            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~AudioService()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
