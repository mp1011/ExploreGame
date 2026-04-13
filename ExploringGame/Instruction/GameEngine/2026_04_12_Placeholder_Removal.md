# PROBLEM
- We used "Placeholder" shapes when one worldsegment needed to position its shape based on a shape from another worldsegment. 
- However this requires a lot of manual work to sync the placeholders, and easily opens up the possibility of dependency loops that are hard to identify.

# NEW APPROACH
- build worldsegments in two phases
    1. create the children shapes
    2. position the children shapes

- when loading worldsegments, create the children for the group at once
- then, in a second pass, run the position code, which now has real references to the dependent shapes

# POSSIBLE PROBLEMS
- what if a needed shape is not among the loaded WorldSegments?
    - in this case, its not possible to have a position dependency. Therefor, if we cannot find a matching dependent shape, that should raise a hard error 

# IMPLEMENTATION
1. Refactor WorldSegment creation to use a two-phased approach
2. Replace occurences of Placeholders with real shapes.