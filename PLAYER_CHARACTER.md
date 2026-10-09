# Bright character prototype

Based on Luca's attached character image: round head, pear-shaped rounded body, large white eyes with dark pupils, small orange nose, floating stubby hands, no eyebrows and no legs. Blue is the default; yellow and green variants are included.

Open `Assets/Movement/Scenes/CharacterPreview.unity` to inspect all three colours. The reusable prefabs are `Assets/Movement/Resources/BrightCharacter.prefab`, `BrightCharacterYellow.prefab` and `BrightCharacterGreen.prefab`. `All Aboard > Build Bright Player Character` rebuilds the generated assets and preview; save any current scene edits before using this editor command.

`PlayerAvatar` is installed on the existing player prefab and playground scenes. It creates the blue body at runtime. The local body casts shadows while remaining invisible to its own first-person camera. This avoids eyes/head clipping into the view. The existing first-person floating hands turn blue; their grip, control, ladder, crouch and throw behaviour is preserved. External model hands follow those existing hand transforms. Body height follows the character controller during crouching. `SetLocalVisibility(false)` exposes the full model for an external camera.

The cosmetic model has no colliders or extra rigidbodies. Existing player physics remain authoritative. The body is a closed smooth mesh; facial parts and hands are rounded primitives. This is a procedural character, not an imported skeletal animation rig or multiplayer implementation.

Passengers reuse the three models in the train's paired seats. Passenger interaction colliders are separate from the cosmetic prefab.

Final Unity 6000.6.4f1 validation: **125/125 Play mode tests passed**.
