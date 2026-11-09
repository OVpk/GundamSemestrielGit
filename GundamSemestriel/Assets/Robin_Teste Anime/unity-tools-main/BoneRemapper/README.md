# UnityBoneRemapper
This tool was designed for those who would want to add a mesh to an existing skinned mesh rendering system. It's designed to allow you to minimalistically create a new skinned Renderer, assign a mesh, and attach the new skinned mesh renderer to the existing systems bone mapping.

## Installation
It's just a script, so you can download it and put it in your Unity Projects Asset folder. However; If you are more comfortable with it, you can download the Unity Package created in released.

## Usage
Add the skinned mesh render component to your game object. Then select the mesh from your project files to be added to your skinned mesh rendering system. Then assign the root bone from your system, matching other skinned mesh renderers you are using.
With the mesh and root bone assigned, Add the Bone Remapper component to your skinned mesh renderer component. You may want to create any missing bones, if needed, then click the "Remap Bones" button on the component.

## Support and other ideas
If you have any questions join the [StudioJAW discord](https://discord.studiojaw.com/). I am also willing to adjust this with any other features.
I'm thinking of other possible features:
- Prompt to create bones (And children of) if they don't exist.
- toggle to just do it without asking
- expose the bones for manual remapping.
  - button to reset bones to imported prefab, copy transforms to a child of the mesh component.
  - option to just spawn the original mesh

## Contributing
If you want to pull this and make a push request, go ahead. Open to any modifications.

## Authors and acknowledgment
Show your appreciation to those who have contributed to the project.

## License
This project is MIT licensed.

## Project status
This is done, unless you have a need for a feature or one of the above features.
Feel free to message us in the discord!