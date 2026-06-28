using ExploringGame.GeometryBuilder.Shapes.SimpleShapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;

public abstract class Deck : Room
{
    protected readonly float PostHeight = Measure.Feet(4);
    protected readonly float PostWidth = Measure.Inches(10);
    protected readonly float PostInset = Measure.Inches(4);
    protected readonly float RailThickness = Measure.Inches(3);
    protected readonly float RailWidth = Measure.Inches(3);
    protected readonly float RailingTopOffset = Measure.Inches(4);
    protected readonly float BarThickness = Measure.Inches(2);
    protected readonly float BarSpacing = Measure.Inches(8);

    public Deck(WorldSegment worldSegment) : base(worldSegment)
    {
    }

    protected Shape CreatePost() => CreatePost(Color.White);

    protected Shape CreatePost(Color color)
    {
        var post = AddChild(new Box(new Theme(Theme.TextureSheetKey, TextureKey.Plain, color)));
        post.Height = PostHeight;
        post.Width = PostWidth;
        post.Depth = PostWidth;
        post.Place().OnFloor();
        return post;
    }

    protected Shape CreateRailing(IShape postFrom, IShape postTo) => CreateRailing(postFrom, postTo, Color.White);

    protected Shape CreateRailing(IShape postFrom, IShape postTo, Color color)
    {
        var topRailing = AddChild(new Box(new Theme(Theme.TextureSheetKey, TextureKey.Plain, color)));
        var bottomRailing = AddChild(new Box(new Theme(Theme.TextureSheetKey, TextureKey.Plain, color)));

        // Calculate direction and distance between posts
        var direction = postTo.LocalPosition - postFrom.LocalPosition;
        var distance = direction.Length();

        // Determine orientation (X-axis or Z-axis)
        var isAlongX = System.Math.Abs(direction.X) > System.Math.Abs(direction.Z);

        if (isAlongX)
        {
            topRailing.Width = distance;
            topRailing.Height = RailThickness;
            topRailing.Depth = RailWidth;
            bottomRailing.Width = distance;
            bottomRailing.Height = RailThickness;
            bottomRailing.Depth = RailWidth;
        }
        else
        {
            topRailing.Width = RailWidth;
            topRailing.Height = RailThickness;
            topRailing.Depth = distance;
            bottomRailing.Width = RailWidth;
            bottomRailing.Height = RailThickness;
            bottomRailing.Depth = distance;
        }

        // Position at midpoint between posts
        var midpoint = (postFrom.LocalPosition + postTo.LocalPosition) / 2f;

        // Top railing - 4 inches below the top
        var postTop = postFrom.LocalPosition.Y + (PostHeight / 2f);
        var topRailingY = postTop - RailingTopOffset - (RailThickness / 2f);
        topRailing.LocalPosition = new Vector3(midpoint.X, topRailingY, midpoint.Z);

        // Bottom railing - 4 inches above the bottom
        var postBottom = postFrom.LocalPosition.Y - (PostHeight / 2f);
        var bottomRailingY = postBottom + RailingTopOffset + (RailThickness / 2f);
        bottomRailing.LocalPosition = new Vector3(midpoint.X, bottomRailingY, midpoint.Z);

        // Create vertical bars between top and bottom railings
        var barHeight = topRailingY - bottomRailingY;
        var barCenterY = (topRailingY + bottomRailingY) / 2f;

        // Calculate number of bars based on distance and spacing
        var numBars = (int)(distance / BarSpacing);

        for (int i = 1; i < numBars; i++)
        {
            var bar = AddChild(new Box(new Theme(Theme.TextureSheetKey, TextureKey.Plain, color)));

            bar.Height = barHeight;
            bar.Width = BarThickness;
            bar.Depth = BarThickness;

            // Interpolate position between the two posts
            var t = (float)i / numBars;
            var barPosition = Vector3.Lerp(postFrom.LocalPosition, postTo.LocalPosition, t);
            bar.LocalPosition = new Vector3(barPosition.X, barCenterY, barPosition.Z);
        }

        return topRailing;
    }
}
