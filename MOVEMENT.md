# Movement prototype

Open `Assets/Movement/Scenes/MovementPlayground.unity` and press Play.
If the scene is missing, use **All Aboard > Create Movement Playground**.

- WASD: walk; mouse: look; Space: jump.
- Escape: release the mouse; left click in the Game view: capture it again.
- Two floating spheres represent the hands, with subtle walking bob and mouse sway.
- The playground has steps, a ramp, jump blocks, walls, and a low ceiling.
- Falling below the playground respawns the player.

The reusable player prefab is in `Assets/Movement/Prefabs/PrototypePlayer.prefab`.
Tune speed, acceleration, jump height, gravity, and jump forgiveness on
`FirstPersonMotor`. Tune sensitivity and hand motion on `FirstPersonView`.
Defaults: 5 metres/second, 1.2 metre jump, 75 degree field of view.

Runtime gameplay is C#. Python is reserved for offline development tools.
The video reference informs the first-person presentation; these controls are
a starting point, not a measured recreation of its movement.

This is a single-player movement foundation. Train platform movement, inherited
train velocity, physical grabbing, networking, and Steam integration are future
work. Hands are cosmetic and have no colliders. Character movement uses Unity's
CharacterController; the player does not push Rigidbody objects yet.

Play Mode tests cover jump/landing, diagonal speed, wall collision, and ceiling
collision. Run them from **Window > General > Test Runner > PlayMode**.
The existing SampleScene and build scene list are preserved; open the playground
explicitly for now.
