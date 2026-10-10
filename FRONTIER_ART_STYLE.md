# Frontier art style â€” first playable pass

The supplied recording is the visual reference: broad shapes, dark train bodies, restrained trim, fine surface grain, warm light, cool distance haze and clustered vegetation. Our rounded colourful characters and 1800s frontier setting remain.

## Implemented

- Shared soft daylight profile, a hazy procedural sky, neutral tone mapping, modest bloom and camera anti-aliasing.
- Continuous shading replaces the custom train shaderâ€™s three abrupt light bands. Terrain and train materials support object-space triplanar surface grain, avoiding stretched UVs and texture swimming on moving cars.
- Five small original, deterministic grain textures generated offline in Python: earth, gravel, wood, foliage and painted metal. No assets are extracted from the reference game.
- Navy train bodies are slightly lighter; brass and red accents are less harsh. Repeated rivets, running-board treads, rack slats and upholstery buttons are reduced so the larger forms read more clearly.
- Taller five-tier pines, denser forest groups and broad low shrub clusters. The forest and meadow ground colours are greener. Existing rail/settlement/tunnel clearances remain.
- Materials, sky and post-processing profile are owned by the scene. Imported material assets are preserved, and rebuilding the generated world reuses the style component and material copies.

## Scope and review

This is the first train-and-forest style pass, active when the procedural railway world builds in Play Mode or from its existing preview command. It establishes the lighting/material direction; towns, weapons and landmarks still use their existing model shapes. Screenshots are actual Unity renders, not concept art. The train is moved along the existing route for capture only.

Open `Assets/Movement/Scenes/ProceduralRailwayPlayground.unity` and press Play to try it. Controls and gameplay collision hulls are unchanged. Forests now contain more trees and will need a packaged-build performance check.

Regenerate the grain with `python Tools/generate_frontier_textures.py` (NumPy and Pillow required). Existing Unity asset GUIDs are preserved.
