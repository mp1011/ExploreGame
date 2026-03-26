# ExploreGame Project Map

## Solution Overview
This solution contains a 3D exploration game built with MonoGame (.NET 8), featuring procedurally generated geometry, collision detection, pathfinding, and interactive entities.
-
---

## ExploringGame (Main Project)

### Root Files
- **Program.cs** - Application entry point. Modify when changing startup configuration or initialization order.
- **Game1.cs** - Main game class that inherits from MonoGame's Game. Central orchestration of game loop, initialization, rendering pipeline. Modify for high-level game flow changes, graphics device setup, or component wiring.
- **ServiceContainer.cs** - Dependency injection container. Add/modify when introducing new services or dependencies that need to be shared across the application.

---

### Agent Docs/
Documentation for AI agents and development guidance.
- **project_map.md** - This file. Update when adding new major folders, projects, or architectural patterns.
- **current_todo.txt** - Current development tasks and TODOs. Update when planning features or tracking progress.

---

### Entities/
Core game entities and interfaces for objects in the game world.
- **Player.cs** - Player entity with position, camera, collision properties. Modify for player attributes, stats, or core capabilities.
- **LightSpirit.cs** - The light spirit entity (appears to be a key game mechanic). Modify for spirit behavior, properties, or interaction rules.
- **LightSpiritSphere.cs** - Visual/physical representation of the light spirit as a sphere. Modify for rendering or collision specifics.
- **GateMark.cs** - Waypoint or marker entities used by the light spirit. Modify for marker behavior or properties.
- **TestEntity.cs** - Test entity for development/debugging. Add test behaviors here.
- **ICamera.cs** - Camera interface defining view/projection matrices. Modify when changing camera contract.
- **IWithPosition.cs** - Interface for entities with 3D position. Implement when creating new positioned entities.

---

### Extensions/
Extension methods for common types.
- **NumberExtensions.cs** - Extensions for numeric types (float, int, etc.). Add utility methods for mathematical operations.
- **VectorExtensions.cs** - Extensions for Vector2/Vector3 types. Add spatial/geometric utility methods.
- **JitterExtensions.cs** - Extensions for Jitter physics library types. Add when bridging between MonoGame and Jitter types.

---

### GameDebug/
Debugging tools and visualization utilities.
- **Debug.cs** - Core debug utilities and flags. Add global debug features or toggles.
- **DebugController.cs** - Controller for debug input and commands. Modify for new debug hotkeys or commands.
- **DebugFixedCamera.cs** - Fixed camera for debugging. Modify camera positioning or controls.
- **DebugBirdsEyeCamera.cs** - Top-down camera view for debugging. Modify for overhead perspective needs.
- **DebugTriangleWithTexture.cs** - Visual triangle debugging with textures. Add for geometry visualization.
- **PolygonVisualizer.cs** - Visualizes polygon shapes. Modify for collision or geometry debugging.
- **MovingEntityDebugger.cs** - Debug tools for entities with motion. Add motion tracking or visualization.
- **IDebugControllable.cs** - Interface for objects that can be debug-controlled. Implement for new debug targets.

---

### GeometryBuilder/
Procedural geometry generation system.

#### Root Files
- **Shape.cs** - Base class for all shapes. Modify when changing core shape behavior, triangle generation, or texture mapping.
- **Measure.cs** - Measurement units and conversions (feet to world units). Modify for scale changes.
- **Side.cs** - Enum or class defining sides (Top, Bottom, North, South, East, West). Modify when adding directional features.
- **Axis.cs** - Enum for 3D axes (X, Y, Z). Modify when adding axis-specific logic.
- **Angle.cs** - Angle utilities and conversions. Add rotation or orientation helpers.
- **WallQuad.cs** - Represents a wall quad with position and dimensions. Modify for wall-specific features.
- **TriangleGroup.cs** - Collection of triangles with common properties (texture, material). Modify for rendering groups.
- **VertexOffset.cs** - Vertex offset data for geometry manipulation. Modify for vertex transformations.
- **2DProjection.cs** - Projects 3D geometry to 2D planes. Modify for wall decal placement or 2D operations.
- **Basic.cs** - Basic geometric primitives and utilities. Add fundamental geometry helpers.
- **ICutoutShape.cs** - Interface for shapes that can cut out from other shapes (doors, windows). Implement for new cutout types.

#### Shapes/
Concrete shape implementations organized by category.

##### SimpleShapes/
Primitive geometric shapes.
- **Box.cs** - Rectangular box/cube shape. Modify for basic box geometry needs.
- **Cylinder.cs** - Cylindrical shape. Modify for column-like structures.
- **Ellipsoid.cs** - Ellipsoid/sphere shape. Modify for rounded objects.

##### Rooms/
Room definitions representing playable spaces.

###### UpstairsRooms/
- **LivingRoom.cs** - Living room layout and furniture. Modify for main living area changes.
- **Kitchen.cs** - Kitchen layout and appliances. Modify for kitchen-specific features.
- **Bedroom.cs** - Bedroom layout. Modify for bedroom design.
- **KidsBedroom.cs** - Children's bedroom. Modify for kid room specifics.
- **Bathroom.cs** - Main bathroom. Modify for bathroom fixtures.
- **HalfBathroom.cs** - Half bath/powder room. Modify for half bath layout.
- **Den.cs** - Den/study room. Modify for den furnishings.
- **SpareRoom.cs** - Spare/guest room. Modify for spare room layout.
- **UpstairsHall.cs** - Upstairs hallway connecting rooms. Modify for upstairs navigation.

###### BasementRooms/
- **Basement.cs** - Main basement space. Modify for basement layout.
- **BasementOffice.cs** - Office in basement. Modify for office features.
- **BasementCloset.cs** - Basement storage closet. Modify for closet contents.
- **BasementStairs.cs** - Stairs connecting basement to upstairs. Modify for stairwell geometry.
- **Garage.cs** - Garage space. Modify for garage layout.
- **OilTankRoom.cs** - Utility room with oil tank. Modify for mechanical room features.

###### ExteriorRooms/
- **FrontDeck.cs** - Outside deck/porch area. Modify for exterior spaces.

##### Room.cs & RoomPart.cs
- **Room.cs** - Base class for all room types. Modify for shared room functionality, door placement, or lighting zones.
- **RoomPart.cs** - Sub-component of a room (alcove, closet). Modify for room subdivision logic.

##### WorldSegments/
Large areas that contain multiple rooms, used for level streaming/activation.
- **WorldSegment.cs** - Base class for world segments. Modify for segment loading, activation, or transition logic.
- **UpstairsWorldSegment.cs** - Upstairs level segment containing upstairs rooms. Modify for upstairs room composition.
- **BasementWorldSegment.cs** - Basement level segment. Modify for basement room composition.
- **OutsideWorldSegment.cs** - Exterior world segment. Modify for outdoor areas.
- **WorldSegmentTransition.cs** - Transition zones between segments (stairs, doors). Modify for loading triggers or transition effects.

##### Furniture/
Furniture and architectural elements.
- **Door.cs** - Standard door shape with cutout. Modify for door geometry or opening mechanics.
- **DoorJunction.cs** - Door frame/junction connecting rooms. Modify for door frame geometry.
- **DoubleDoorJunction.cs** - Double door frame. Modify for wide doorways.
- **WindowJunction.cs** - Window frame and cutout. Modify for window placement or design.
- **Couch.cs** - Couch furniture. Modify for seating geometry.
- **OfficeDesk.cs** - Desk furniture. Modify for desk design.
- **ElectricFireplace.cs** - Fireplace object. Modify for fireplace features.

##### Appliances/
Electrical and mechanical appliances.
- **HighHatLight.cs** - Recessed ceiling light. Modify for ceiling light placement or appearance.
- **LightSwitch.cs** - Wall-mounted light switch. Modify for switch geometry or interaction.
- **OilTank.cs** - Utility oil tank. Modify for mechanical equipment.

##### Decals/
2D textures applied to 3D surfaces.
- **WallDecal.cs** - Decal applied to walls (pictures, posters). Modify for decal rendering or application.
- **WallDecalStamp.cs** - Stamped/repeated wall decals. Modify for pattern application.

##### TestShapes/
Development and testing shapes.
- **FaceCutoutTest.cs** - Test shape for face cutout logic. Add for testing geometry operations.
- **PhysicsTestShape.cs** - Test shape for physics/collision. Add for physics debugging.
- **SingleFaceTest.cs** - Single face geometry test. Add for basic rendering tests.
- **TestMover.cs** - Shape with test movement. Add for motion testing.

##### Other Shapes/
- **PlaceableShape.cs** - Base for shapes that can be placed in the world with position/rotation. Extend for new placeable objects.
- **PlaceholderShape.cs** - Temporary placeholder geometry. Use during development before final art.
- **ComplexShapePart.cs** - Component of a complex multi-part shape. Modify for composite shapes.
- **ShapeStamp.cs** - Shape that can be stamped/repeated in patterns. Modify for procedural decoration.
- **StampedShape.cs** - Shape created via stamping. Modify for stamp application logic.
- **MengerSponge.cs** - Fractal Menger sponge shape (likely for testing). Modify for complex geometry tests.
- **SurfaceIndent.cs** - Indentation/depression in a surface. Modify for surface detail features.

---

### Instruction/
Development instructions and task management.
- **current_todo.txt** - Current development tasks. Update with active work items.

---

### LevelControl/
Level management, loading, and world segment activation.
- **LevelData.cs** - Defines level structure, rooms, segments. Modify when adding new levels or changing level composition.
- **LoadedLevelData.cs** - Runtime representation of loaded level data. Modify for level state management.
- **GameState.cs** - Overall game state management. Modify for game progression, state transitions, or game-wide flags.
- **WorldSegmentAnchorProcessor.cs** - Processes anchor points for world segments. Modify for segment positioning or alignment logic.
- **WorldSegmentActivationManager.cs** - Manages loading/unloading of world segments based on player position. Modify for streaming logic or performance optimization.

---

### Logics/
Game logic, AI, and interactive systems.

#### Root Files
- **PlayerInput.cs** - Player input handling and controls. Modify for control schemes or input mappings.
- **IPlayerInput.cs** - Interface for player input. Mock for testing.
- **PlayerMotion.cs** - Player movement logic (walking, jumping, gravity). Modify for movement feel or mechanics.
- **EntityMover.cs** - Moves entities through the world. Modify for entity movement logic.
- **TestEntityController.cs** - Controller for test entities. Add test behaviors.
- **LogicInterfaces.cs** - Common logic interfaces. Add new behavioral contracts.
- **ILightSource.cs** - Interface for objects that emit light. Implement for new light-emitting entities.
- **LightIntensity.cs** - Light intensity calculations. Modify for lighting formulas.

#### Collision/
Collision detection and response system using Jitter physics.
- **ICollidable.cs** - Interface for objects with collision. Implement for new collidable entities.
- **CollisionResponder.cs** - Handles collision responses and events. Modify for collision reaction logic.
- **DetectFloorCollision.cs** - Specialized floor collision detection. Modify for ground detection or walking mechanics.
- **SetupColliderBodies.cs** - Initializes Jitter physics bodies for shapes. Modify for physics setup or body properties.

##### ColliderMakers/
Factory classes for creating collision shapes from geometry.
- **IColliderMaker.cs** - Interface for collider factories. Implement for new collider types.
- **BoxColliderMaker.cs** - Creates box colliders. Modify for box collision generation.
- **SphereColliderMaker.cs** - Creates sphere colliders. Modify for sphere collision.
- **CapsuleColliderMaker.cs** - Creates capsule colliders (player). Modify for capsule collision.
- **RoomColliderMaker.cs** - Creates colliders for room geometry. Modify for room boundary collision.
- **DoorColliderMaker.cs** - Creates door-specific colliders. Modify for door collision behavior.
- **StepColliderMaker.cs** - Creates colliders for stairs/steps. Modify for stair navigation.

#### Controllers/
Entity behavior controllers.
- **LightSpiritController.cs** - Main controller for light spirit AI. Modify for spirit behavior, phases, or AI logic.
- **TimedAction.cs** - Utility for time-delayed actions. Modify for timing or scheduling needs.

##### LightSpiritPhases/
State machine phases for light spirit behavior.
- **IPhaseHandler.cs** - Interface for phase handlers. Implement for new spirit phases.
- **AbsentPhaseHandler.cs** - Spirit is not present. Modify for absence state.
- **BreakInPhaseHandler.cs** - Spirit breaking into the house. Modify for initial appearance.
- **HalfPresencePhaseHandler.cs** - Spirit partially manifested. Modify for partial presence state.
- **FullPresencePhaseHandler.cs** - Spirit fully manifested. Modify for full presence state.
- **GateMarkManager.cs** - Manages gate marks/waypoints for spirit. Modify for waypoint logic.
- **LightSpiritFlickerEffect.cs** - Flickering light effect for spirit. Modify for visual effects.

#### Pathfinding/
AI navigation and pathfinding system.
- **PathFinder.cs** - A* pathfinding algorithm. Modify for pathfinding logic or optimization.
- **PathFinderTarget.cs** - Target destination for pathfinding. Modify for goal representation.
- **Waypoint.cs** - Navigation waypoint. Modify for waypoint data or behavior.
- **WaypointGraph.cs** - Graph of connected waypoints. Modify for navigation graph structure.
- **RoomGraph.cs** - Graph representing room connectivity. Modify for room-to-room navigation.
- **RoomGraphEdge.cs** - Edge between rooms in the graph. Modify for room connection data.
- **RoomLightData.cs** - Lighting information for rooms (for AI decisions). Modify for light-based pathfinding.
- **AnnotatedGraph.cs** - Graph with additional annotations. Modify for enriched navigation data.

#### ShapeControllers/
Controllers for interactive shapes (doors, lights, switches).
- **IPlayerActivated.cs** - Interface for player-activated objects. Implement for new interactive elements.
- **IOnOff.cs** - Interface for on/off state objects. Implement for toggleable items.
- **DoorController.cs** - Controls door opening/closing. Modify for door interaction logic.
- **LightController.cs** - Controls lights on/off. Modify for light switching behavior.
- **LightSwitchController.cs** - Controller linking switch to lights. Modify for switch-light relationships.

---

### Motion/
Physics and motion systems.
- **AcceleratedMotion.cs** - Acceleration-based motion calculations. Modify for physics-based movement.

---

### Rendering/
Rendering pipeline and graphics systems.
- **VertexBufferBuilder.cs** - Builds vertex buffers for GPU. Modify for vertex data layout or optimization.
- **ShapeBuffer.cs** - GPU buffer containing rendered shapes. Modify for rendering batches or draw calls.
- **RenderEffect.cs** - Shader effects and rendering techniques. Modify for visual effects or shader parameters.
- **PointLights.cs** - Point light rendering system. Modify for dynamic lighting or light limits.

---

### Services/
Utility services and algorithms.
- **AudioService.cs** - Audio playback and management. Modify for sound effects or music.
- **CameraService.cs** - Camera management and controls. Modify for camera behavior or perspectives.
- **Physics.cs** - Physics calculations and utilities. Add physics helper methods.
- **EntityRoomFinder.cs** - Determines which room an entity is in. Modify for spatial queries.
- **RoomLightingCalculator.cs** - Calculates lighting levels in rooms. Modify for lighting algorithms.
- **WaypointDistanceCalculator.cs** - Calculates distances between waypoints. Modify for pathfinding costs.

#### Geometry Processing Services
- **ShapeBuilder.cs** - High-level shape construction orchestration. Modify for shape building pipeline.
- **ShapePlacer.cs** - Places shapes in the world. Modify for placement rules or validation.
- **ShapeAdjuster.cs** - Adjusts shapes after placement. Modify for post-placement transformations.
- **ShapeSplitter.cs** - Splits shapes into smaller parts. Modify for geometry subdivision.
- **ShapeBufferCreator.cs** - Creates render buffers from shapes. Modify for buffer optimization.
- **TriangleMaker.cs** - Generates triangles from geometry definitions. Modify for triangle generation logic.
- **TriangleSubdivider.cs** - Subdivides triangles for detail. Modify for LOD or tessellation.
- **TriangleSubtracter.cs** - Removes triangles from geometry (cutouts). Modify for boolean operations.
- **SierpinskiSplitter.cs** - Sierpinski fractal subdivision. Modify for fractal patterns.
- **QuadExtractor.cs** - Extracts quads from geometry. Modify for quad-based operations.
- **SideRemover.cs** - Removes specific sides from shapes. Modify for selective face removal.
- **VertexOffsetter.cs** - Applies offsets to vertices. Modify for vertex manipulation.
- **RemoveSurfaceRegion.cs** - Removes regions from surfaces. Modify for surface cutout operations.
- **SplitTrianglesForTiling.cs** - Splits triangles for texture tiling. Modify for texture mapping improvements.

---

### Texture/
Texture management and theme system.
- **TextureInfo.cs** - Metadata about textures (size, path, UV coords). Modify for texture properties.
- **TextureSheet.cs** - Sprite sheet/atlas management. Modify for texture packing.
- **LoadedTextureSheets.cs** - Registry of loaded textures. Modify for texture loading or caching.
- **Theme.cs** - Visual theme system (colors, materials). Modify for visual style variations.

---

### Testing/
In-game testing utilities (not unit tests).
- **TestMaps.cs** - Test level/map configurations. Add test scenarios.
- **TestMaps.ShapeStampTest.cs** - Partial class for shape stamp tests. Add stamp tests.
- **TestMaps.WallDecalTest.cs** - Partial class for wall decal tests. Add decal tests.
- **TestWorldSegment.cs** - Test world segment. Add segment tests.

#### Shapes/
Test shape implementations.
- **TestShapeStamp.cs** - Test stamp shape. Add for stamp testing.
- **TestShapeStampGenerator.cs** - Generates test stamps. Modify for stamp generation tests.
- **TestStampedShape.cs** - Result of stamp application. Modify for stamp result testing.

#### Controllers/
Test controllers.
- **TestShapeStampGeneratorController.cs** - Controller for stamp generator testing. Modify for interactive stamp tests.

---

## ExploringGame.Tests (Test Project)

Unit tests and visual regression tests.

### Root Test Files
- **AssemblyInfo.cs** - Assembly configuration for tests.
- **BasicRoomTest.cs** - Tests for basic room functionality. Add tests for room creation or validation.
- **WorldSegmentTransitionTests.cs** - Tests for world segment transitions. Add tests for segment loading.
- **RoomLightingTests.cs** - Tests for room lighting calculations. Add lighting tests.
- **RoomLightingGroupTests.cs** - Tests for lighting groups. Add group lighting tests.
- **LightSpiritTests.cs** - Tests for light spirit behavior. Add spirit AI tests.
- **PathfinderTest.cs** - Tests for pathfinding algorithms. Add pathfinding tests.
- **ShaderReworkTests.cs** - Tests for shader changes. Add shader validation tests.
- **ShaderLightCountTests.cs** - Tests for shader light limits. Add light count tests.
- **ShapeStampTests.cs** - Tests for shape stamping. Add stamp functionality tests.
- **DynamicObjectLightingTests.cs** - Tests for dynamic object lighting. Add dynamic lighting tests.

### TestHelpers/
Test utilities and mocks.
- **TestGame.cs** - Mock game instance for testing. Modify for test environment setup.
- **TestResult.cs** - Test result data structures. Modify for test output formats.
- **MockPlayerInput.cs** - Mock player input for testing. Modify for input simulation.
- **InputEvent.cs** - Input event data for testing. Modify for input recording/playback.

### VisualRegressionTests/
Visual output comparison tests.
- **WallDecalTests.cs** - Visual tests for wall decal rendering. Add decal visual tests.
- **LightingVisualTests.cs** - Visual tests for lighting. Add lighting visual tests.
- **ConnectingRoomsVisualTests.cs** - Visual tests for room connections. Add room connection visual tests.

### WallDecalPlacement/
Tests for wall decal placement system.
- **WallDecalPlacementTests.cs** - Unit tests for decal placement. Add decal placement logic tests.
- **WallDecalTestShape.cs** - Test shape for decal placement. Modify for decal test scenarios.
- **WallDecalTestController.cs** - Controller for decal placement tests. Modify for interactive decal tests.
- **WallWithGapWorldSegment.cs** - Test segment with wall gaps. Modify for gap testing.
- **WallWithAsymmetricGapWorldSegment.cs** - Test segment with asymmetric gaps. Modify for complex gap scenarios.

---

## Common Change Patterns

### Adding a New Room
1. Create room class in appropriate `GeometryBuilder/Shapes/Rooms/` subfolder
2. Inherit from `Room` base class
3. Add furniture/appliances using existing shape classes
4. Update corresponding `WorldSegment` to include the new room
5. Update `LevelData.cs` if needed for level composition

### Adding Interactive Objects
1. Create shape in `GeometryBuilder/Shapes/` (appropriate subfolder)
2. Create controller in `Logics/ShapeControllers/`
3. Implement relevant interfaces (`IPlayerActivated`, `IOnOff`, etc.)
4. Register controller in `ServiceContainer.cs` if needed

### Adding New Entity Types
1. Create entity class in `Entities/`
2. Implement `IWithPosition` and other relevant interfaces
3. Create controller in `Logics/Controllers/` for behavior
4. Add collision support in `Logics/Collision/ColliderMakers/` if needed

### Modifying Rendering/Graphics
1. Shader changes: `Rendering/RenderEffect.cs`
2. Vertex data: `Rendering/VertexBufferBuilder.cs`
3. Lighting: `Rendering/PointLights.cs` or `Services/RoomLightingCalculator.cs`
4. Buffers: `Rendering/ShapeBuffer.cs`

### Adding Geometry Operations
1. Create service in `Services/` (e.g., for triangle operations, splitting, etc.)
2. Register in `ServiceContainer.cs`
3. Call from shape building pipeline in `ShapeBuilder.cs`

### Debugging Issues
1. Add debug flags or visualizations in `GameDebug/`
2. Implement `IDebugControllable` for new debug targets
3. Add debug controls in `DebugController.cs`

### Adding Tests
1. Unit tests: Add to `ExploringGame.Tests/` root
2. Visual tests: Add to `ExploringGame.Tests/VisualRegressionTests/`
3. Test helpers: Add to `ExploringGame.Tests/TestHelpers/`
4. Feature-specific tests: Create subfolder under `ExploringGame.Tests/`
