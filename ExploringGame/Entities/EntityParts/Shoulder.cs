using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes.SimpleShapes;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExploringGame.Entities.EntityParts;

public class Shoulder : EntityPart<Puppet>
{

    public Shoulder(Puppet puppet, float sizeScale) : base(puppet)
    {
        var shape = AddChild(new Ellipsoid(0.2f * sizeScale, (new Theme(Color.GreenYellow))));
        Size = shape.Size;
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return Array.Empty<Triangle>();
    }
}
