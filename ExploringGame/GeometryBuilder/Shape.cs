using ExploringGame.Entities;
using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Rendering;
using ExploringGame.Services;
using ExploringGame.Texture;
using Jitter2.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.GeometryBuilder;

public interface IShape
{
    string Tag { get; }
    Shape[] TraverseAllChildren();
    bool ContainsPoint(Vector3 point);
    Vector3 LocalPosition { get; }
    float SideLength(Side side);
}

public abstract class Shape : IWithPosition, IShape
{
    public virtual IColliderMaker ColliderMaker => null;

    public virtual ShapeBufferType ShapeBufferType
    {
        get
        {
            if (Parent != null)
                return Parent.ShapeBufferType;
            else
                return ShapeBufferType.Static;
        }
    }

    public virtual ILightingGroup LightingGroup
    {
        get
        {
            if (Parent != null)
                return Parent.LightingGroup;
            else
                return DefaultLightingGroup.Instance;
        }
    }
    public RigidBody[] ColliderBodies { get; set; }

    public Shape Parent { get; private set; }

    private List<Shape> _children = new();

    public IEnumerable<Shape> Children => _children.AsReadOnly();

    public abstract ViewFrom ViewFrom { get; }
    public Vector3 LocalPosition { get; set; }

    public Vector3 WorldPosition
    {
        get
        {
            if (LocalParent == null)
                return LocalPosition;
            else 
                return LocalParent.WorldPosition + LocalPosition;
        }
        set
        {
            if (LocalParent == null)
                LocalPosition = value;
            else 
                LocalPosition = value - LocalParent.WorldPosition;
        }
    }

    private Shape _localParent = null;

    /// <summary>
    /// Either the nearest IPlaceable parent shape, or a root WorldSegment.
    /// </summary>
    public Shape LocalParent
    {
        get
        {
            if (_localParent == null)
            {
                var parent = Parent;
                while(parent != null)
                {
                    if(parent is IPlaceableObject || parent is WorldSegment)
                    {
                        _localParent = parent;
                        break;
                    }

                    parent = parent.Parent;
                }
            }

            return _localParent;
        }
    }

    public Vector3 Size { get; set; }
    public string Tag { get; set; }

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

    public float LocalX
    {
        get => LocalPosition.X; set => LocalPosition = new Vector3(value, LocalPosition.Y, LocalPosition.Z);
    }
    public float LocalY
    {
        get => LocalPosition.Y; set => LocalPosition = new Vector3(LocalPosition.X, value, LocalPosition.Z);
    }
    public float LocalZ
    {
        get => LocalPosition.Z; set => LocalPosition = new Vector3(LocalPosition.X, LocalPosition.Y, value);
    }

    public float WorldX
    {
        get => WorldPosition.X; set => WorldPosition = new Vector3(value, WorldPosition.Y, WorldPosition.Z);
    }
    public float WorldY
    {
        get => WorldPosition.Y; set => WorldPosition = new Vector3(WorldPosition.X, value, WorldPosition.Z);
    }
    public float WorldZ
    {
        get => WorldPosition.Z; set => WorldPosition = new Vector3(WorldPosition.X, WorldPosition.Y, value);
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
        Axis.X => LocalPosition.X,
        Axis.Y => LocalPosition.Y,
        Axis.Z => LocalPosition.Z,
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
            case Axis.X: LocalX = value; return;
            case Axis.Y: LocalY = value; return;
            case Axis.Z: LocalZ = value; return;
        }
    }

    public float GetLocalSide(Side side)
    {
        return side switch
        {
            Side.North => LocalPosition.Z - Size.Z / 2f,
            Side.South => LocalPosition.Z + Size.Z / 2f,
            Side.West => LocalPosition.X - Size.X / 2f,
            Side.East => LocalPosition.X + Size.X / 2f,
            Side.Top => LocalPosition.Y + Size.Y / 2f,
            Side.Bottom => LocalPosition.Y - Size.Y / 2f,
            _ => throw new ArgumentException("Only singular sides can be used")
        };
    }

    public float GetWorldSide(Side side)
    {
        return side switch
        {
            Side.North => WorldPosition.Z - Size.Z / 2f,
            Side.South => WorldPosition.Z + Size.Z / 2f,
            Side.West => WorldPosition.X - Size.X / 2f,
            Side.East => WorldPosition.X + Size.X / 2f,
            Side.Top => WorldPosition.Y + Size.Y / 2f,
            Side.Bottom => WorldPosition.Y - Size.Y / 2f,
            _ => throw new ArgumentException("Only singular sides can be used")
        };
    }

    public void SetLocalSide(Side side, float value)
    {
        switch(side)
        {
            case Side.North:
                LocalZ = value + Size.Z / 2f;
                return;
            case Side.South:
                LocalZ = value - Size.Z / 2f;
                return;
            case Side.West:
                LocalX = value + Size.X / 2f;
                return;
            case Side.East:
                LocalX = value - Size.X / 2f;
                return;
            case Side.Top:
                LocalY = value - Size.Y / 2f;
                return;
            case Side.Bottom:
                LocalY = value + Size.Y / 2f;
                return;
            default:
                throw new ArgumentException("Only singular sides can be used");
        }
    }

    public void SetWorldSide(Side side, float value)
    {
        switch(side)
        {
            case Side.North:
                WorldZ = value + Size.Z / 2f;
                return;
            case Side.South:
                WorldZ = value - Size.Z / 2f;
                return;
            case Side.West:
                WorldX = value + Size.X / 2f;
                return;
            case Side.East:
                WorldX = value - Size.X / 2f;
                return;
            case Side.Top:
                WorldY = value - Size.Y / 2f;
                return;
            case Side.Bottom:
                WorldY = value + Size.Y / 2f;
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

    public void SetLocalSideUnanchored(Side side, float value)
    {       
        var currentOpposite = GetLocalSide(side.Opposite());
        SetLocalSide(side, value);

        var oppDelta = GetLocalSide(side.Opposite()) - currentOpposite;

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

        SetLocalSide(side, value);
    }

    public void SetWorldSideUnanchored(Side side, float value)
    {       
        var currentOpposite = GetWorldSide(side.Opposite());
        SetWorldSide(side, value);

        var oppDelta = GetWorldSide(side.Opposite()) - currentOpposite;

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

        SetWorldSide(side, value);
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

    public void RemoveChild(Shape child)
    {
        if(child.Parent == this)
            child.Parent = null;

        _children.Remove(child);
    }

    public Rotation Rotation { get; set; }

    public bool ContainsPoint(Vector3 point)
    {
        var min = WorldPosition - Size / 2f;
        var max = WorldPosition + Size / 2f;

        return point.X >= min.X && point.X <= max.X &&
           point.Y >= min.Y && point.Y <= max.Y &&
           point.Z >= min.Z && point.Z <= max.Z;
    }

    public TShape FindChild<TShape>() where TShape : Shape
    {
        return TraverseAllChildren().OfType<TShape>().FirstOrDefault();
    }

    public TShape FindFirstAncestor<TShape>() where TShape : Shape
    {
        var current = Parent;
        while (current != null)
        {
            if (current is TShape ancestor)
                return ancestor;
            current = current.Parent;
        }
        return null;
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

        return scaleMatrix * rotationMatrix * Matrix.CreateTranslation(WorldPosition);
    }
    public virtual RasterizerState RasterizerState { get; } = null;

    public override string ToString()
    {
        if (Tag != null)
            return $"{GetType().Name} ({Tag})";
        else
            return GetType().Name;
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
            output[this] = ViewFrom == ViewFrom.None ? Array.Empty<Triangle>() : AdjustTrianglesForDisplay(BuildCuboid(), quality);
        else
        {
            output[this] = ViewFrom == ViewFrom.None ? Array.Empty<Triangle>() :
                                                       AdjustTrianglesForDisplay(BuildInternal(quality), quality);
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
                output[key] = output[key].Select(p => p.Rotate(LocalPosition, Rotation)).ToArray();
        }
    }

    /// <summary>
    /// fixes the winding order and breaks up large triangles for tiled textures
    /// </summary>
    /// <param name="triangles"></param>
    /// <returns></returns>
    private Triangle[] AdjustTrianglesForDisplay(IEnumerable<Triangle> triangles, QualityLevel quality)
    {
        var adjusted = triangles.ToArray();

        if (quality > QualityLevel.Basic)
            adjusted = new SplitTrianglesForTiling().Execute(this, adjusted);

        adjusted = CorrectWinding(adjusted);

        return adjusted.Where(p => !p.IsDegenerate).ToArray();
    }

    private Triangle[] CorrectWinding(IEnumerable<Triangle> triangles)
    {
        var triangleArray = triangles.ToArray();

        if (triangleArray.Length == 0)
            return Array.Empty<Triangle>();

        // Calculate the geometric center of all triangles (centroid of all vertices)
        var allVertices = triangleArray.SelectMany(t => new[] { t.A, t.B, t.C }).ToArray();
        var geometricCenter = new Vector3(
            allVertices.Average(v => v.X),
            allVertices.Average(v => v.Y),
            allVertices.Average(v => v.Z)
        );

        // Check if all triangles are roughly coplanar (all on one plane)
        // If so, use the shape's Position as the reference instead
        Vector3 referenceCenter;
        if (AreTrianglesCoplanar(triangleArray))
        {
            // Triangles are on a single plane - use the shape's bounding box center
            referenceCenter = LocalPosition;
        }
        else
        {
            // Triangles span multiple planes - use their geometric center
            referenceCenter = geometricCenter;
        }

        return triangleArray.Select(t => CorrectWinding(t, referenceCenter)).ToArray();
    }

    private bool AreTrianglesCoplanar(Triangle[] triangles)
    {
        if (triangles.Length <= 1)
            return true;

        // Calculate average normal
        var avgNormal = Vector3.Zero;
        foreach (var triangle in triangles)
        {
            avgNormal += triangle.Normal;
        }
        avgNormal = Vector3.Normalize(avgNormal / triangles.Length);

        // Check if all normals are within a certain angle of the average
        const float coplanarThreshold = 0.95f; // ~18 degrees tolerance
        foreach (var triangle in triangles)
        {
            float alignment = Vector3.Dot(triangle.Normal, avgNormal);
            if (alignment < coplanarThreshold)
                return false; // Normals vary too much - not coplanar
        }

        return true; // All normals are similar - coplanar surface
    }

    private Triangle CorrectWinding(Triangle triangle, Vector3 geometricCenter)
    {
        // Determine if the normal points toward or away from the geometric center
        // by using the triangle's centroid as a reference point
        Vector3 triangleCentroid = (triangle.A + triangle.B + triangle.C) / 3f;
        Vector3 centerToTriangle = triangleCentroid - geometricCenter;

        // Dot product tells us if normal points away from center (positive) or toward (negative)
        float normalAlignment = Vector3.Dot(triangle.Normal, centerToTriangle);

        bool normalPointsOutward = normalAlignment > 0;

        // For ViewFrom.Outside: we want normals pointing outward (away from center)
        // For ViewFrom.Inside: we want normals pointing inward (toward center)
        bool shouldInvert = (ViewFrom == ViewFrom.Outside && !normalPointsOutward) ||
                           (ViewFrom == ViewFrom.Inside && normalPointsOutward);

        return shouldInvert ? triangle.Invert() : triangle;
    }

    /// <summary>
    /// Generates the triangles of the bounding volume of this shape
    /// </summary>
    /// <returns></returns>
    protected Triangle[] BuildCuboid() => TriangleMaker.BuildCuboid(this);

    protected abstract Triangle[] BuildInternal(QualityLevel quality);

    #endregion
}

