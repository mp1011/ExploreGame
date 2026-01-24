using ExploringGame.Extensions;
using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Services;
using ExploringGame.Texture;
using Jitter2.Dynamics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.GeometryBuilder;

public abstract class Shape
{
    public virtual IColliderMaker ColliderMaker => null;

    public RigidBody[] ColliderBodies { get; set; }

    public Shape Parent { get; private set; }

    private List<Shape> _children = new();

    public IEnumerable<Shape> Children => _children.AsReadOnly();

    public abstract ViewFrom ViewFrom { get; }
    public Vector3 Position { get; set; }
    public Vector3 Size { get; set; }

    #region Theme
    public virtual Theme Theme { get; } = Theme.Missing;

    public TextureInfo MainTexture
    {
        get => Theme.MainTexture; 
        set => Theme.MainTexture = value;
    }
    public TextureInfo TextureInfoForSide(Side side) => Theme.TextureInfoForSide(side);
    public Dictionary<Side, TextureInfo> SideTextures => Theme.SideTextures;

    public bool HasValidSize => Size.X.IsNonZeroNumber() && Size.Y.IsNonZeroNumber() && Size.Z.IsNonZeroNumber();

    #endregion

    public float X
    {
        get => Position.X; set => Position = new Vector3(value, Position.Y, Position.Z);
    }
    public float Y
    {
        get => Position.Y; set => Position = new Vector3(Position.X, value, Position.Z);
    }
    public float Z
    {
        get => Position.Z; set => Position = new Vector3(Position.X, Position.Y, value);
    }
    public float Width
    {
        get => Size.X; set => Size = new Vector3(value, Size.Y, Size.Z);
    }
    public float Height
    {
        get => Size.Y; set => Size = new Vector3(Size.X, value, Size.Z);
    }
    public float Depth
    {
        get => Size.Z; set => Size = new Vector3(Size.X, Size.Y, value);
    }

    public float GetAxisPosition(Axis axis) => axis switch
    {
        Axis.X => Position.X,
        Axis.Y => Position.Y,
        Axis.Z => Position.Z,
        _ => throw new ArgumentException("invalid axis")
    };

    public float SideLength(Side side) => side switch
    {
        Side.West => GetAxisSize(Axis.Z),
        Side.East => GetAxisSize(Axis.Z),
        Side.North => GetAxisSize(Axis.X),
        Side.South => GetAxisSize(Axis.X),
        _ => GetAxisSize(Axis.Y)
    };

    public float GetAxisSize(Axis axis) => axis switch
    {
        Axis.X => Size.X,
        Axis.Y => Size.Y,
        Axis.Z => Size.Z,
        _ => throw new ArgumentException("invalid axis")
    };

    public void SetAxisPosition(Axis axis, float value) 
    {
        switch(axis)
        {
            case Axis.X: X = value; return;
            case Axis.Y: Y = value; return;
            case Axis.Z: Z = value; return;
        }
    }

    /// <summary>
    /// Sets the top Y coordinate while preserving height
    /// </summary>
    public float TopAnchored
    {
        get => Position.Y + Size.Y / 2f;
        set
        {
            var currentTop = this.TopAnchored;
            var delta = value - currentTop;
            Y += delta;
        }
    }

    /// <summary>
    /// Sets the bottom Y coordinate while preserving height
    /// </summary>
    public float BottomAnchored
    {
        get => Position.Y - Size.Y / 2f;
        set
        {
            var currentBottom = this.BottomAnchored;
            var delta = value - currentBottom;
            Y += delta;
        }
    }

    /// <summary>
    /// Sets the bottom Y coordinate while leaving the top as is, thereby changing the height
    /// </summary>
    public float BottomUnanchored
    {
        get => Position.Y - Size.Y / 2f;
        set
        {
            var originalTop = this.TopAnchored;
            Height = originalTop - value;
            TopAnchored = originalTop;
        }
    }

    public float GetSide(Side side)
    {
        return side switch
        {
            Side.North => Position.Z - Size.Z / 2f,
            Side.South => Position.Z + Size.Z / 2f,
            Side.West => Position.X - Size.X / 2f,
            Side.East => Position.X + Size.X / 2f,
            Side.Top => Position.Y + Size.Y / 2f,
            Side.Bottom => Position.Y - Size.Y / 2f,
            _ => throw new ArgumentException("Only singular sides can be used")
        };
    }

    public void SetSide(Side side, float value)
    {
        switch(side)
        {
            case Side.North:
                Z = value + Size.Z / 2f;
                return;
            case Side.South:
                Z = value - Size.Z / 2f;
                return;
            case Side.West:
                X = value + Size.X / 2f;
                return;
            case Side.East:
                X = value - Size.X / 2f;
                return;
            case Side.Top:
                Y = value - Size.Y / 2f;
                return;
            case Side.Bottom:
                Y = value + Size.Y / 2f;
                return;
            default:
                throw new ArgumentException("Only singular sides can be used");
        }
    }

    /// <summary>
    /// 0.0 = left
    /// 1.0 = right
    /// </summary>
    /// <param name="axis"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public float RelativeAxisPoint(Axis axis, float value)
    {
        var center = GetAxisPosition(axis);
        var size = GetAxisSize(axis);
        var left = center - size / 2.0f;
        var right = center + size / 2.0f;

        return left + (right - left) * value;
    }

    public void SetSideUnanchored(Side side, float value)
    {       
        var currentOpposite = GetSide(side.Opposite());
        SetSide(side, value);

        var oppDelta = GetSide(side.Opposite()) - currentOpposite;

        switch(side)
        {
            case Side.North:
                Depth -= oppDelta;
                break;
            case Side.South:
                Depth += oppDelta;
                break;
            case Side.West:
                Width -= oppDelta;
                break;
            case Side.East:
                Width += oppDelta;
                break;
            case Side.Top:
                Height += oppDelta;
                break;
            case Side.Bottom:
                Height -= oppDelta;
                break;
            default:
                throw new System.ArgumentException("invalid side");
        }

        SetSide(side, value);
    }

    public T AddChild<T>(T child) where T:Shape
    {
        if (child.Parent == this)
            return child;

        if (child.Parent != null)
            child.Parent._children.Remove(child);

        child.Parent = this;
        _children.Add(child);
        return child;
    }

    public Rotation Rotation { get; set; }

    public bool ContainsPoint(Vector3 point)
    {
        var min = Position - Size / 2f;
        var max = Position + Size / 2f;

        return point.X >= min.X && point.X <= max.X &&
           point.Y >= min.Y && point.Y <= max.Y &&
           point.Z >= min.Z && point.Z <= max.Z;
    }

    public Shape[] TraverseAllChildren()
    {
        List<Shape> shapes = new List<Shape>();
        TraverseAllChildren(shapes);
        return shapes.ToArray();
    }

    private void TraverseAllChildren(List<Shape> shapes)
    {
        shapes.Add(this);
        foreach (var child in Children)
            child.TraverseAllChildren(shapes);
    }

    public bool SelfOrDescendantOf(Shape shape)
    {
        if (shape == this)
            return true;
        else if (Parent == null)
            return false;
        else
            return Parent.SelfOrDescendantOf(shape);
    }

    public virtual Matrix GetWorldMatrix()
    {
        var scaleMatrix = Matrix.Identity; // todo, see about this
        var rotationMatrix = Rotation?.AsMatrix() ?? Matrix.Identity;

        return scaleMatrix * rotationMatrix * Matrix.CreateTranslation(Position);
    }

    #region Build

    protected virtual void BeforeBuild()
    {
    }
    protected virtual void AfterBuild()
    {
    }


    public Dictionary<Shape, Triangle[]> Build(QualityLevel quality)
    {
        var output = new Dictionary<Shape, Triangle[]>();
        Build(quality, output);
        return output;
    }

    private void Build(QualityLevel quality, Dictionary<Shape, Triangle[]> output)
    {
        BeforeBuild();
       
        if (quality == QualityLevel.DoNotRender)
            output[this] = Array.Empty<Triangle>();
        else if (quality == QualityLevel.CuboidOnly)
            output[this] = ViewFrom == ViewFrom.None ? Array.Empty<Triangle>() : AdjustTrianglesForDisplay(BuildCuboid());
        else
        {
            output[this] = ViewFrom == ViewFrom.None ? Array.Empty<Triangle>() :
                                                       AdjustTrianglesForDisplay(BuildInternal(quality));
            foreach(var child in Children)
                child.Build(quality - 1, output);
        }

        if (Rotation != null)
            ApplyRotation(output);

        AfterBuild();
    }

    private void ApplyRotation(Dictionary<Shape, Triangle[]> output)
    {
        foreach(var key in output.Keys)
        {
            if(key.SelfOrDescendantOf(this))
                output[key] = output[key].Select(p => p.Rotate(Position, Rotation)).ToArray();
        }
    }

    /// <summary>
    /// fixes the winding order and breaks up large triangles for tiled textures
    /// </summary>
    /// <param name="triangles"></param>
    /// <returns></returns>
    private Triangle[] AdjustTrianglesForDisplay(IEnumerable<Triangle> triangles)
    {
        var adjusted = CorrectWinding(triangles);
        adjusted = new SplitTrianglesForTiling().Execute(this, adjusted);
        return adjusted;
    }

    private Triangle[] CorrectWinding(IEnumerable<Triangle> triangles)
    {
        return triangles.Select(CorrectWinding).ToArray();
    }

    private Triangle CorrectWinding(Triangle triangle)
    {
        var winding = triangle.CalcWinding(Position);
        if (winding == Winding.CounterClockwise && ViewFrom == ViewFrom.Inside)
            return triangle.Invert();
        else if (winding == Winding.Clockwise && ViewFrom == ViewFrom.Outside)
            return triangle.Invert();
        else
            return triangle;

    }

    /// <summary>
    /// Generates the triangles of the bounding volume of this shape
    /// </summary>
    /// <returns></returns>
    protected Triangle[] BuildCuboid() => TriangleMaker.BuildCuboid(this);

    protected abstract Triangle[] BuildInternal(QualityLevel quality);

    #endregion
}
