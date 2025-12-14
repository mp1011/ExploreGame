using Microsoft.Xna.Framework;
using System;
using System.ComponentModel.DataAnnotations;

namespace ExploringGame.Logics;

/// <summary>
/// simulates head bobbing effect when the player moves
/// </summary>
class HeadBob
{
    private const float _bobSpeed = 8f; 
    private const float _bobAmount = 0.07f;

    private float _bobPhase = 0f;
    private float _defaultY;

    public Vector3 Update(bool isMoving, GameTime gameTime, Vector3 cameraPosition)
    {
        // need to redo to be based on room player is in
        if(_defaultY == 0f)
            _defaultY = cameraPosition.Y;

        if (isMoving)
            _bobPhase += (float)gameTime.ElapsedGameTime.TotalSeconds * _bobSpeed;
        else
            _bobPhase = 0f; // Reset when not moving
        float bobOffset = (float)Math.Sin(_bobPhase) * _bobAmount;
        cameraPosition.Y = _defaultY + bobOffset;

        return cameraPosition;
    }
}
