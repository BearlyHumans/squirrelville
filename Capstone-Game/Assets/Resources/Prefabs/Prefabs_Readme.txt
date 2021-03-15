ALL Prefabs should go in here, preferably in a well named sub-folder

What are Prefabs?
-A Prefab is a GameObject that is saved in a folder (e.g. on your hard-drive) instead of in a scene. Otherwise most of the ways they behave is the same.
-A Prefab should be thought of as the 'plan' for an object, since you can use them to create instances of the object in a scene.
-While editing you can also update any copies of a prefab by changing the parent, rather than changing them all individually.

Why do they need to be here?
-To copy a prefab mid-game you need to either save a link to it somewhere - which is messy and easy to break - or load it using a directory.
-Unity only allows you to load files at runtime from the 'Resources' folder, and for clarity all prefabs should also go in the 'Prefabs' sub-folder.