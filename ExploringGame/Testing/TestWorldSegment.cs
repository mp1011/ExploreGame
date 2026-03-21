using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace ExploringGame.Testing;

public class TestWorldSegment : WorldSegment
{
    public override Vector3 DefaultPlayerStart => PlayerStart;

    public Vector3 PlayerStart { get; set; }

    private readonly List<WorldSegmentTransition> _transitions = new();
    public override IReadOnlyList<WorldSegmentTransition> Transitions => _transitions;

    public TestWorldSegment(Vector3? playerStart = null)
    {
        if (playerStart.HasValue)
            PlayerStart = playerStart.Value;
    }

    public void AddTransition(Type worldSegmentType)
    {
        _transitions.Add(new WorldSegmentTransition(worldSegmentType));
    }

    public void ClearTransitions()
    {
        _transitions.Clear();
    }
}

public class TestWorldSegmentA : TestWorldSegment
{
    public TestWorldSegmentA() : base() { }
}

public class TestWorldSegmentB : TestWorldSegment
{
    public TestWorldSegmentB() : base() { }
}

public class TestWorldSegmentC : TestWorldSegment
{
    public TestWorldSegmentC() : base() { }
}

public class TestWorldSegmentD : TestWorldSegment
{
    public TestWorldSegmentD() : base() { }
}

public class TestWorldSegmentE : TestWorldSegment
{
    public TestWorldSegmentE() : base() { }
}
