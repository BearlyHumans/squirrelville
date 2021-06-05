Thank you for purchasing Brute Force - Grass Shader!
If you have any questions please email me at: bruteforcegamesstudio@gmail.com

// THE README INSTRUCTIONS ARE NOW OBSOLETE, PLEASE USE THE ONLINE DOCUMENTATION INSTEAD:
https://www.bruteforcegame.com/brute-force-grass-shader
//

Please play scene "01BruteForceGrass" first for a quick introduction to the shader asset :)!
Left click to push down the grass
Right click to remove the grass

The asset is composed of 3 main Shaders:
-The "Grass" Shader and "InteractiveGrass" shader featuring light/shadows/shadowcasting support and can run on PC/Linux
-The "GrassLite" Shader and "InteractiveGrassLite" shader featuring light/shadows support and can run on PC/Linux
-The "GrassMobile" Shader and "InteractiveGrassMobile" shader featuring mobile and mac support and can run on any devices
Please use the benchmark showcase in the main scene to determine which suits best for your game.
Tick off the "Use RenderTexture Effect" parameters in the material if you do not wish to have interactive effects.

//URP and Standard RP//
Both render pipelines are in the asset, 2 separate folders exist for "Materials", "Scenes", "Shaders" and "Prefabs" choose the one accordingly.
You can delete the URP folders if you don't need it so you don't have missing shaders in your project and vice-versa if you don't need built-in.

//Interactivity//
The "Interactive" variants allow you to have a custom grass effect controlled via a mouse click particle system (prefab named: "PEMouse")
or on player tracking (prefab named: "PEPlayer")

In order for the interactivity effect to work you need to follow these important steps:
-A particle effect should be set in the same layer as the culling mask of "Main CameraEffect"
-You should have a camera in your scene (prefab named "Main CameraEffect") with the scripts "SetInteractiveShaderEffects" 
-It must renders to a Render Texture ("GrassRT") and has its culling mask set to the same as your particle system's layer 
-It should preferably follow your main camera or your player

You can use the prefab "GrassSetup.prefab" or the scene "GrassSetup.unity" as a template!

// Terrains //
Please use the scene "04TerrainSetup" provided for a good starting template.
In order to "paint" grass and ground you need to open the "paint terrain" tab inside the terrain inspector and select (or add if missing) a texture layer.
The first one ("NewLayer01") is for ground painting and all the others are for grass painting.
If you want to edit or create a new terrain/texture layer here's what you need to know:
-Diffuse is for color
-Normal is for grass pattern and will override the grass pattern of your terrain's material (do not assign one for the ground layer)
-Specular(color) is the grass color override
-Tiling settings Size x is for the grass pattern size and Size y is for grass thinness, PLEASE DO NOT MODIFY ANY OTHER VALUES, it should be Size.x = whatever you want
 Size.y = between 0 and 2, Offset.x = 1 and Offset.y = 1
If you need other ground layers for some reasons please contact me.

// MAC Users //
Please use the "02MacMobileShowcase" scene for a better showcase.

//Grass Layers//
To increase the number of grass layers, change the "NumberOfStacks" values (between 0 and 17) for the Grass and GrassLite
The Mac/Mobile shader has a script called "MeshExtrusion.cs" that lets you duplicate and extrude multiple grass layers exactly like the shader
It currently has a 100 layers limit, a compute shader is in the work for greater editor performances.
Please note that the main shader is limited to 17 layers for performance sakes and another implementation of the geometry pass is in the work
for unlimited layers.

The shader works best with a simple anti-aliasing post-process.
The shader works best in the Gamma color space for Standard RP.
The shader works best in the Linear color space for URP.


//FAQ//
Q: Why does my grass looks all grey?
A: You are using the linear color space, the grass works best in gamma but still works great in linear

Q: Why can't I use the normal and lite shader on mobile or mac?
A: Because Apple's renderer can not compile geometry shader, use the mobile/mac shader instead

Q: In what kind of game does the grass looks best?
A: It looks best on third person, orthographic, isometric and 3D RTS game

Q: How do I control the interactivity effect on the grass?
A: There are 2 templates available for you to edit named "PEMouse" and "PEPlayer", the particle will be rendered on the grass and creates the effect
Use blue colors if you want to dig in the grass and red colors if you want to darken it.

Q: What's the main difference between the normal shader and the lite one?
A: The main difference is that the lite version doesn't cast grass shadows, it will still cast the first layer's shadow and receive custom shadows.
Another thing to note is the lack of depth on the grass, this might be inconvenient if you're using DoF effects or any depth post process.
Consider using the normal one in that case

Q: How do I make my own grass?
A: You make your own grass mainly through textures:
Color Grass: Colors the grass and ground respecting your mesh uv
NoGrassTexture: Let's you mask out the grass, black = ground and white = grass
GrassPattern: You can vary the shape of the grass through this texture, it should tile and is independent from your mesh uv
NoiseColor: A noise for the wind color, you should leave a perlin noise or something light.
DistortionWind: The wind effect, the more detailed the texture the more patterns your wind is going to have.

Q: My project is set in URP but all I see is pink.
A: There is a problem with how you imported the asset, contact bruteforcegamesstudio@gmail.com
You have 2 different scenes, one for standard RP and one for URP. Check the URP scene folder.

// A visual online documentation is in the work!