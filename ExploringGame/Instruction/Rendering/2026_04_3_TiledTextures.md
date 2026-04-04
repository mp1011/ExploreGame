# SUMMARY
Tiled textures don't display right on angled surfaces

# BACKGROUND INFORMATION

## RELEVANT CODE
- Services\SplitTrianglesForTiling.cs

## IMPORTANT INFORMATIOM
- for tiled textures, coordinates (0,0) and (1,1) are technically the same
- however, since they represent regions on the texture, it makes a big different which was chosen
- the solution for this was to insert vertices on points where the texture would be (0,0) or (1,1)
    - then the code would decide which coordinate made the most sense based on the triangle

## DESCRIPTION OF THE PROBLEM
- when a surface is slanted, it appears to break the rule that vertices are inserted at (0,0) or (1,1) coordinates
- the resulting difference means the wrong texture coordinates get used
- it could be the texture splitting code is still using axis-aligned planes

# TECHNICAL APPROACH

1. Write a failing test (DONE)
    - Write a test to capture this behavior (place in ExploringGame.Tests\TextureCoordinateTests\)
    - unlike other tests this one won't use a Game instance
    - create a shape with only one surface, and use VertexOffsets to slant the surface
    - make the shape big enough to hold exactly four copies of the tiled texture (thus expecting eight triangles in total).
    - apply a tiled texture to this plane
    - call Build on the shape to get its triangles
    - call VertexBufferBuilder
    - explicitly test the texture coordinates of each triangle (referring to them as upper/lower left/right)


2. Adjust CalcTextureCoordinates_Tile to properly project onto the correct plane
    - Based on the above test, I've identified the root of the problem:
    - in VertexBufferBuilder -> CalcTextureCoordinates_Tile, it computes U and V by discarding one of the three dimensions
    - this is why lengths become skewed for slanted surfaces
    - the corners are already properly positioned. What we need to do is project the vertex onto the plane formed by the corners
    - this should result in U and V values that properly line up