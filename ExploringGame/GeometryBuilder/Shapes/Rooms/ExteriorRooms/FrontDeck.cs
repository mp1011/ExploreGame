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
    public Shape WestPart { get; private set; }
    protected override Side OmitSides => Side.Top;

    public override Theme Theme => new FrontPorchTheme();
    public FrontDeck(WorldSegment worldSegment) : base(worldSegment)
    {
    }

    public override void LoadChildren()
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

        // posts
        var northWestPost = CreatePost().Place()
            .OnSideInner(Side.North, WestPart, PostInset)
            .OnSideInner(Side.West, WestPart, PostInset);

        var northEastPost = CreatePost().Place()
         .OnSideInner(Side.North, WestPart, PostInset)
         .OnSideInner(Side.East, this, -PostInset);

        var southWestPost = CreatePost().Place()
          .OnSideInner(Side.South, WestPart, -PostInset)
          .OnSideInner(Side.West, WestPart, PostInset);

        var southEastPost = CreatePost().Place()
          .OnSideInner(Side.South, WestPart, -PostInset)
          .OnSideInner(Side.East, WestPart, -PostInset);

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
