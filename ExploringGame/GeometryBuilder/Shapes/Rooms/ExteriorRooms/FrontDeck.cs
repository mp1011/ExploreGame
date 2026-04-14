using ExploringGame.GeometryBuilder.Shapes.SimpleShapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Services;
using ExploringGame.Texture;
using ExploringGame.Texture.Themes;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;

public class FrontDeck : Room
{
    private readonly float PostHeight = Measure.Feet(4);
    private readonly float PostWidth = Measure.Inches(10);
    private readonly float PostInset = Measure.Inches(4);
    private readonly float RailThickness = Measure.Inches(3);
    private readonly float RailWidth = Measure.Inches(3);
    private readonly float RailingTopOffset = Measure.Inches(4);
    private readonly float BarThickness = Measure.Inches(2);
    private readonly float BarSpacing = Measure.Inches(8);

    public Shape WestPart { get; private set; }
    public override Side OmitSides => Side.North | Side.South | Side.West | Side.Top;
    public override Theme Theme => new FrontPorchTheme();
    public FrontDeck(WorldSegment worldSegment) : base(worldSegment)
    {
    }

    public void LoadChildren()
    {
        WestPart = AddChild(new Box(Theme));
        WestPart.SideTextures[Side.Top] = Theme.GetTexture(TextureKey.Wood);

        WestPart.Width = Width;
        WestPart.Height = Measure.Feet(4);
        WestPart.Depth = Depth - Measure.Feet(6);

        WestPart.Place()
            .OnSideOuter(Side.West, this)
            .OnSideInner(Side.North, this);

        WestPart.SetSide(Side.Top, GetSide(Side.Bottom));

        var northPart = AddChild(new Box(Theme));
        northPart.OmitSides = Side.Top;
        northPart.Height = Height;
        northPart.Width = Width;
        northPart.Depth = Measure.Inches(5);
        northPart.Place().OnSideInner(Side.North)
            .OnSideInner(Side.West);
        northPart.SetSide(Side.Top, WestPart.GetSide(Side.Top));
            
 
        // posts
        var northWestPost = CreatePost().Place()
            .OnSideInner(Side.North, WestPart, PostInset)
            .OnSideInner(Side.West, WestPart, PostInset)
            .Shape();

        var westMiddlePost = CreatePost().Place()
            .OnSideInner(Side.West, WestPart, PostInset)
            .Shape();
        westMiddlePost.Z = WestPart.Z;

        var northEastPost = CreatePost().Place()
         .OnSideInner(Side.North, WestPart, PostInset)
         .OnSideInner(Side.East, this, -PostInset)
         .Shape();

        var southWestPost = CreatePost().Place()
          .OnSideInner(Side.South, WestPart, -PostInset)
          .OnSideInner(Side.West, WestPart, PostInset)
          .Shape();

        var southEastPost = CreatePost().Place()
          .OnSideInner(Side.South, WestPart, -PostInset)
          .OnSideInner(Side.East, WestPart, -PostInset)
          .Shape();

        // railing
        CreateRailing(northWestPost, westMiddlePost);
        CreateRailing(westMiddlePost, southWestPost);
        CreateRailing(southWestPost, southEastPost);
        CreateRailing(northWestPost, northEastPost);
    }

    private Shape CreatePost()
    {
        var post = AddChild(new Box(new Theme(Theme.TextureSheetKey, TextureKey.Plain, Color.White)));
        post.Height = PostHeight;
        post.Width = PostWidth;
        post.Depth = PostWidth;
        post.Place().OnFloor();
        return post;
    }

    public Shape CreateRailing(Shape postFrom, Shape postTo)
    {
        var topRailing = AddChild(new Box(new Theme(Theme.TextureSheetKey, TextureKey.Plain, Color.White)));
        var bottomRailing = AddChild(new Box(new Theme(Theme.TextureSheetKey, TextureKey.Plain, Color.White)));

        // Calculate direction and distance between posts
        var direction = postTo.Position - postFrom.Position;
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
        var midpoint = (postFrom.Position + postTo.Position) / 2f;

        // Top railing - 4 inches below the top
        var postTop = postFrom.Position.Y + (PostHeight / 2f);
        var topRailingY = postTop - RailingTopOffset - (RailThickness / 2f);
        topRailing.Position = new Vector3(midpoint.X, topRailingY, midpoint.Z);

        // Bottom railing - 4 inches above the bottom
        var postBottom = postFrom.Position.Y - (PostHeight / 2f);
        var bottomRailingY = postBottom + RailingTopOffset + (RailThickness / 2f);
        bottomRailing.Position = new Vector3(midpoint.X, bottomRailingY, midpoint.Z);

        // Create vertical bars between top and bottom railings
        var barHeight = topRailingY - bottomRailingY;
        var barCenterY = (topRailingY + bottomRailingY) / 2f;

        // Calculate number of bars based on distance and spacing
        var numBars = (int)(distance / BarSpacing);

        for (int i = 1; i < numBars; i++)
        {
            var bar = AddChild(new Box(new Theme(Theme.TextureSheetKey, TextureKey.Plain, Color.White)));

            bar.Height = barHeight;
            bar.Width = BarThickness;
            bar.Depth = BarThickness;

            // Interpolate position between the two posts
            var t = (float)i / numBars;
            var barPosition = Vector3.Lerp(postFrom.Position, postTo.Position, t);
            bar.Position = new Vector3(barPosition.X, barCenterY, barPosition.Z);
        }

        return topRailing;
    }
}

public class FrontDeckStairs : Stairs
{
    public override ViewFrom ViewFrom => ViewFrom.None;
    public override Theme Theme => TopFloor.Theme;

    public FrontDeckStairs(FrontYard bottomFloor, FrontDeck topFloor) 
        : base(topFloor.WorldSegment, CalcStepSize(bottomFloor, topFloor), bottomFloor, topFloor, 
            CalcStairsWidth(topFloor),
            CalcStairsDepth(bottomFloor, topFloor))
    {
        FixedAmbientLight = topFloor.FixedAmbientLight;
    }

    private static Vector2 CalcStepSize(FrontYard yard, FrontDeck deck)
    {
        return new Vector2(CalcStairsWidth(deck) / 4.0f, CalcStairsDepth(yard, deck));
    }

    private static float CalcStairsWidth(FrontDeck deck) => deck.WestPart.Width;

    private static float CalcStairsDepth(FrontYard yard, FrontDeck deck) =>
        deck.Depth - deck.WestPart.Depth;

    protected override Side StartSide => Side.West;

    protected override StairStep CreateStep()
    {
        return new StairStep(Theme.TextureSheetKey, Theme.GetTexture(TextureKey.Wood));
    }
}
