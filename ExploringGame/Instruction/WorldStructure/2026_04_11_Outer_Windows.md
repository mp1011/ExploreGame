# GOAL
Allow for windows to show through OuterWall shapes.

# BACKGROUND INFO
- OuterWall is a thin shape meant to represent the exterior wall of a house.
- OuterWall is created as a child of an exterior section
- the Window class is a junction between two rooms that displays a window
	- junctions already have code to make cutouts for windows, doors, and other connections

# WHAT IS NEEDED
- restrict OuterWall to be a child of a Room specifically
- since Outerwall is only supposed to represent an outside wall, omit the opposite Side. 
    - in other words, if this is a North facing wall, we don't need to render the South side (you can keep East/West though)
- read the RoomConnection of the OuterWall's parent and use that to determine what shapes to cut out of the wall
