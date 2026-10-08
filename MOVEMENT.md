# Movement prototype

Open `Assets/Movement/Scenes/MovementPlayground.unity` and press Play.
If the scene is missing, use **All Aboard > Create Movement Playground**.

- WASD: walk; mouse: look; Space: jump.
- Hold either Shift key to sprint (8 metres/second).
- Hold C to crouch: slower movement, a lower camera, and a shorter collider.
  Release C to stand. Under low ceilings you stay crouched until there is room;
  standing then resumes automatically without pressing C again.
  Sprinting while crouched stays at crouch speed (2.5 metres/second).
- Aim the center dot at the blue cube: a white outline indicates it can be picked up.
- E: pick up. The dot disappears and the hands grip either side of the cube.
- German Z (physical Y on a US keyboard): weld the held cube to the solid surface
  you are holding it towards. Bring its face within 15 cm of the surface first.
  It snaps to a small contact gap, the hands let go, and the center dot returns.
  E picks up a welded cube again and removes its weld. Pressing weld away from
  a surface keeps the cube in your hands.
  Welds use fixed physics joints, including connections to moving bodies;
  a cube welded to the platform moves and turns with it. Removing the welded
  surface releases the cube. Welds currently last for the current Play session.
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
  Carrying allows a small retraction near walls, then blocks movement that would
  crush the grip into the camera; you can still slide sideways or back away.
  Overlapping carry volumes are cleared before casting, including thin floor
  edges. Held cubes and temporarily ignored dropped cubes do not count as the
  ground beneath the player or block standing clearance.
- Only the hands have a black outline. Adjust Outline Thickness and Outline Color
  on `Assets/Movement/Materials/HandOutline.mat` (zero thickness hides the border).
- The playground has steps, a ramp, jump blocks, walls, and a low ceiling.
- The low tunnel on the right near the start requires crouching to pass through.
- Beyond the jump blocks, the orange platform moves sideways and gently turns.
  Jump onto it from the ground or landing block. Try walking, jumping, crouching
  under its low roof, and carrying or dropping its second blue cube.
  Standing riders follow its movement and yaw. Jumping or stepping off preserves
  its velocity; dropping or throwing adds that velocity to the cube as well.
  Loose cubes ride through Rigidbody contact and friction, without parenting.
  Standing on a loose or welded cube follows that cube's motion too, including
  when it is riding on the platform. Jump momentum uses the support body's
  physical point velocity, avoiding spikes when contact changes between frames.
  Standing still on it does not trigger footsteps or movement lag in the hands.
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

This is a single-player movement and object physics foundation. The moving test
platform provides the initial support and momentum system; a full train,
networking, and Steam integration are future work.
Tune Travel, Period, and Yaw Amplitude on `MovingTestPlatform.prefab`.
Hands are cosmetic and have no colliders. Character movement uses Unity's
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
Moving-platform checks cover passive translation and rotation, jump momentum,
loose-cube friction, carrying through crouch changes, inherited drop velocity,
and standing automatically after leaving a moving low roof.
Regression checks also cover crouched floor-facing pickup on a moving deck,
pickup/drop over a thin suspended floor, and sprinting into a wall while carrying,
then sliding sideways and backing away without folding the hands into the camera.
Welding checks cover the German keyboard binding, static and moving attachments,
re-pickup, rejecting distant surfaces, and falling after removal of the welded
wall. Rider checks include standing on loose and welded cubes and jumping off.
Audio checks verify the imported clips and prefab references, and that jump/landing
feedback fires once instead of repeating while the player rests on the ground.
Run them from **Window > General > Test Runner > PlayMode**.
The existing SampleScene and build scene list are preserved; open the playground
explicitly for now.
