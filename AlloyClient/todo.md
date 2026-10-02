# Alloy.Audio
* add device swapping
* poll for device disconnecting

# Alloy.Content
* move font json parsing to content builder to remove an extra package from client
* reduce atlas size to 2048 and add texture arrays

# Alloy.UiLib
* add rotation support to hit test/bounds
* make OverridePrimCount private also make it vertex count instead of prims, force EnsureBufferCapacity usage
* turn config into class and make a SpriteConfig base that they extend

# Alloy.Engine
* add resizable ssbos

# Rendering
* connected objects/cave walls
* add partial tile upload support