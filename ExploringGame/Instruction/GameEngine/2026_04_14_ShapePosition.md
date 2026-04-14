# PROBLEM 
- After we removed Placeholder shapes, a large number of positioning problems came up. 
- The core of the problem is that if we position Shape A relative to Shape B, we assume that Shape B is already placed

## INCONSISTENCY OF SHAPE POSITIONING
- there are a few different places where we've been setting size and position:
    - in the constructor of the Shape
    - in the "LoadChildren" method of the Shape
    - in the WorldSegment that owns the shape
- the lack of consistency means its very hard to tell when or if a shape is in its ideal position

## CLARITY ON RESPONSIBILITY
- WorldSegment constructor - creating the Shape objects that make up this segment
    - identifying the order in which the shapes should be processed
    - but NOT sizing or placing them
- WorldSegment 