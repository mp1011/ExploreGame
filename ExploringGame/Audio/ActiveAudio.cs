using ExploringGame.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.ComponentModel;

namespace ExploringGame.Audio;

public class ActiveAudio
{
    private readonly AudioService _audioService;
    private SoundEffectInstance _instance1, _instance2;
    

    private enum CrossfadeState
    {
        A,
        AtoB,
        B,
        BtoA
    }

    private CrossfadeState _crossfadeState;
    private TimeSpan _soundDuration;

    private TimeSpan _crossfadeDuration = TimeSpan.FromSeconds(2);

    private TimeSpan _playTime;

    private TimeSpan _nextCrossfadeStart;

    public SoundEffectKey Key { get; }

    public ActiveAudio(AudioService audioService, SoundEffectKey key)
    {
        _audioService = audioService;
        _soundDuration = audioService.GetDuration(key);
        Key = key;

    }

    public void Update(GameTime gameTime)
    {
        if(_instance1 == null)
        {
            _instance1 = _audioService.CreateInstance(Key);
            _instance1.Play();
            _playTime = gameTime.TotalGameTime;
            _crossfadeState = CrossfadeState.A;

            _nextCrossfadeStart = gameTime.TotalGameTime + (_soundDuration - _crossfadeDuration);
        }

        if (_instance2 == null)
        {
            _instance2 = _audioService.CreateInstance(Key);
        }

      //  GameDebug.Debug.Watch1 = $"{_crossfadeState} {_instance1.Volume.ToString("0.0")}  {_instance2.Volume.ToString("0.0")}";

        if (gameTime.TotalGameTime < _nextCrossfadeStart)
            return;

        if(_crossfadeState == CrossfadeState.A)
        {
            _crossfadeState = CrossfadeState.AtoB;
            _instance2.Play();
        }
        if (_crossfadeState == CrossfadeState.B)
        {
            _crossfadeState = CrossfadeState.BtoA;
            _instance1.Play();
        }

        float crossfadePercent = ((gameTime.TotalGameTime - _nextCrossfadeStart) / _crossfadeDuration).ClampF(0, 1f);

        if (_crossfadeState == CrossfadeState.AtoB)
        {
            _instance1.Volume = 1.0f - crossfadePercent;
            _instance2.Volume = crossfadePercent;
        }
        else if (_crossfadeState == CrossfadeState.BtoA)
        {
            _instance2.Volume = 1.0f - crossfadePercent;
            _instance1.Volume = crossfadePercent;
        }

        if(crossfadePercent >= 1.0f)
        {
            _nextCrossfadeStart = gameTime.TotalGameTime + (_soundDuration - (_crossfadeDuration*2));
            _crossfadeState = _crossfadeState == CrossfadeState.AtoB ? CrossfadeState.B : CrossfadeState.A;
        }
    }
}

