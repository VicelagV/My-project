# Movement prototype

Open `Assets/Movement/Scenes/MovementPlayground.unity` and press Play.
If the scene is missing, use **All Aboard > Create Movement Playground**.

- WASD: walk; mouse: look; Space: jump.
- Hold either Shift key to sprint (8 metres/second).
- C toggles crouch: slower movement, a lower camera, and a shorter collider.
  Standing is blocked under low ceilings; leave the tunnel and press C again.
  Sprinting while crouched stays at crouch speed (2.5 metres/second).
- Aim the center dot at the blue cube: a white outline indicates it can be picked up.
- E: pick up. The dot disappears and the hands grip either side of the cube.
- Tap Q: drop. Hold Q: charge the small bar; release Q: throw in the look direction.
- After dropping or throwing, the hands return to rest and do not follow the cube.
- Full throw charge takes 1.2 seconds. Charge changes launch speed from 3 to 12 m/s.
- Escape: release the mouse; left click in the Game view: capture it again.
- Two floating spheres represent the hands, with subtle walking bob and mouse sway.
- Hands and held objects lag slightly behind movement and turns, then settle.
  The lag is bounded; it does not move the camera or change your aiming direction.
  Crouch eye-height animation does not add inertia. While carrying, the cube and
  gripping hands share the current camera pose; free-object interpolation resumes
  after release. Shape-aware carry clearance keeps floor aiming from pulling the
  cube into the camera.
- Only the hands have a black outline. Adjust Outline Thickness and Outline Color
  on `Assets/Movement/Materials/HandOutline.mat` (zero thickness hides the border).
- The playground has steps, a ramp, jump blocks, walls, and a low ceiling.
- The low tunnel on the right near the start requires crouching to pass through.
- Falling below the playground respawns the player.
- Light prototype sounds cover footsteps, jump/landing, pickup, throwing, and
  cube impacts. Crouch footsteps are quieter; resting objects do not chatter.

The reusable player prefab is in `Assets/Movement/Prefabs/PrototypePlayer.prefab`.
Tune speed, acceleration, jump height, gravity, and jump forgiveness on
`FirstPersonMotor`. Tune sensitivity and hand motion on `FirstPersonView`.
Movement Lag, Turn Lag, Inertia Response, and Maximum Inertia control the trailing
feel. Crouch Height, Crouch Speed, and Crouch Camera Speed control crouching.
Tune or mute the sounds in `Assets/Movement/Audio/LightPrototypeAudio.asset`.
Master Volume controls all prototype effects. Source links and CC0 licences are
in the Audio folder. Footsteps currently use one soft sound set for all surfaces.
Defaults: 5 metres/second, 1.2 metre jump, 75 degree field of view.

Runtime gameplay is C#. Python is reserved for offline development tools.
The video reference informs the first-person presentation; these controls are
a starting point, not a measured recreation of its movement.

This is a single-player movement and object physics foundation. Train platform
movement, inherited train velocity, networking, and Steam integration are future
work. Hands are cosmetic and have no colliders. Character movement uses Unity's
CharacterController; the player does not push Rigidbody objects by walking yet.
Loose items use Rigidbody gravity and collision. The cube has mass, friction,
slight bounce, damping, and continuous collision detection. Carrying constrains
the body and checks walls; releasing restores simulation and applies the player's
velocity plus the charged throw velocity. Dropping stays at the current hand
position. `ObjectPhysics.physicMaterial` controls friction and bounce.

Play Mode tests cover jump/landing, diagonal speed, sprint, wall/ceiling collision,
pickup/drop gravity, charged throws, reach limits, and hands returning after a throw.
They also cover crouch clearance, camera height, crouch speed, bounded inertia,
settling after stopping, held-object lag, repeated crouching while holding, and
looking down near the floor while crouched.
Audio checks verify the imported clips and prefab references, and that jump/landing
feedback fires once instead of repeating while the player rests on the ground.
Run them from **Window > General > Test Runner > PlayMode**.
The existing SampleScene and build scene list are preserved; open the playground
explicitly for now.
