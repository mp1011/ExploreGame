using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;

class FrontYard : Room
{
    protected override Side OmitSides => Side.North | Side.South | Side.East | Side.West | Side.Top;

    public override Theme Theme => new ExteriorTheme();

    public FrontYard(WorldSegment worldSegment, FrontDeck deck) : base(worldSegment)
    {
        Depth = deck.Depth + Measure.Feet(10);
        Width = Measure.Feet(20);
        Height = deck.Height + Measure.Feet(4);

        SetSide(Side.Bottom, deck.GetSide(Side.Bottom) - Measure.Feet(4));
        SetSide(Side.South, deck.GetSide(Side.South) + Measure.Feet(5));
        SetSide(Side.East, deck.GetSide(Side.West));

        AddConnectingRoom(deck, Side.East);

        FixedAmbientLight = LightIntensity.Bright;
    }

    public override void LoadChildren()
    {
        
    }
}
