using ExploringGame.Entities;
using ExploringGame.Logics.Pathfinding;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExploringGame.GameDebug;


/// <summary>
/// Helper class which takes a snapshot of the position and target waypoint of a moving object at regular intervals
/// </summary>
public class MovingEntityDebugger
{
    private readonly TimeSpan LogInterval = TimeSpan.FromSeconds(1);

    private readonly IWithPosition _entity;
    private readonly PathFinder _pathFinder;
    
    private TimeSpan _lastLogTime = TimeSpan.Zero;

    public record LogEntry(Vector3 Position, float Distance, PathFinderTarget CurrentTarget);

    public List<LogEntry> Logs = new List<LogEntry>();

    public MovingEntityDebugger(IWithPosition entity, PathFinder pathfinder)
    {
        _entity = entity;
        _pathFinder = pathfinder;
    }

    public void Update(GameTime gameTime)
    {
        if (gameTime.TotalGameTime - _lastLogTime < LogInterval)
            return;

        _lastLogTime = gameTime.TotalGameTime;

        var delta = 0f;
        if(Logs.FirstOrDefault() != null)
            delta = (_entity.Position - Logs.LastOrDefault().Position).Length();

        Logs.Add(new LogEntry(_entity.Position, delta, _pathFinder.CurrentTarget));
    }

}
