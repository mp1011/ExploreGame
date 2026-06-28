using ExploringGame.Entities;
using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.Structures;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Services;
using ExploringGame.Texture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.BasementRooms;

public class Garage : Room
{
    private Basement _basement;
    public override Theme Theme => new BasementRoomTheme();

    public Garage(WorldSegment worldSegment, Basement basement) : base(worldSegment)
    {
        _basement = basement;
        Height = basement.Height;
        this.SetWorldSide(Side.Bottom, 0f);

        Width = 10f;
        Depth = 10f;
    }

    public void LoadChildren()
    {
        _basement.AddConnectingRoomWithJunction(new DoorJunction(this, Side.South, HAlign.Right, DoorDirection.Pull, StateKey.GarageInnerDoorOpen),
            this, Side.South, HAlign.Right, offset: -0.5f);

        this.SetWorldSideUnanchored(Side.West, WorldSegment.GetWorldSide(Side.West));
        this.SetWorldSideUnanchored(Side.South, WorldSegment.GetWorldSide(Side.South));

        this.Place().AlignSideWith(Side.East, _basement);
    }
}
