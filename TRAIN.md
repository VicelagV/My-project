# Train prototype

Open `Assets/Movement/Scenes/TrainPlayground.unity` and press Play. The consist is
**locomotive -> open coal tender -> passenger -> passenger**. You start on the
station platform beside the tender. The original movement playground is separate.

## Balance

| Setting | Value |
| --- | --- |
| Starting Steam / Pressure | 50 / 40 |
| Steam / Pressure per coal | +40 / +5, capped at 100 |
| Starting coal | 10 pieces |
| Steam drain at full throttle | 0.5 per second (1 every 2 seconds) |
| Steam drain at half throttle | 0.25 per second |
| Steam drain at quarter throttle | 0.125 per second |
| Steam drain at zero throttle or while braking | 0 |
| Vent cost | 10 Steam, removes 20 Pressure |

A complete 40-Steam coal refill supplies 80 seconds at full throttle. Starting
Steam lasts 100 seconds at full throttle. Values drain smoothly over time; high
Steam no longer multiplies consumption above the full-throttle rate. Pressure
generation still increases at high Steam. Weight slows acceleration without
increasing Steam use. Normal Steam range remains 20â€“80; low Steam reduces pulling
power, and high Steam allows overspeed. At 100 Steam the gauge warns CRITICAL.

## Controls and coal

WASD walk, mouse look, Space jump, Shift sprint, hold C crouch.

- Aim at the conductor seat and E to sit. The throttle lever is at the front of
  the left armrest. Aim at its round handle, **hold M1**, and move the mouse forward
  to increase throttle or back to decrease it. One hand holds the moving lever;
  the camera stays fixed during the drag. Release M1 to let go; throttle remains
  at the selected position. A full stroke uses about 300 pixels of mouse travel.
  The lever only responds to the seated occupant, and cannot be used while carrying.
- Look away from cab controls and E to stand. E during a lever drag releases the
  grip first. Escape or loss of focus also lets go. The speed gauge shows actual
  train speed as the locomotive accelerates toward the throttle's requested speed.
- The **brake lever** sits on the right armrest. Hold M1 and move the mouse forward
  to apply more braking, or back to release it. Like the throttle, it only works
  from the driver's seat, holds its setting after release, and uses one hand.
  Half travel gives half braking force; full travel gives full braking force.
- The **reverse button** sits behind the brake on the right armrest and uses E
  while seated. It physically sinks into the armrest, with a hand reaching onto it.
- A side-mounted **pressure valve** uses E to vent. The wheel turns with a hand on
  its rim, then closes again. Whistle, bell, door and speedometer damage-test buttons
  are on the driver's front panel; E presses them with a short inward stroke and
  hand reach. Carriage door and coupler buttons also have physical press feedback.
- Steam, pressure and speed have round faces, tick marks and moving needles, with
  small titles and numerical readouts underneath. Steam/pressure dials span 0–100;
  speed spans 0–15 m/s. Broken speedometers lose their needle and read BROKEN.
  Controls and gauges highlight white when aimed at. Contextual hover hints show
  E or hold-M1 instructions. The debug HUD stays hidden.
- The black open tender has low side guards and a clear center aisle. Coal stock
  stays fixed to it through acceleration, braking and turns. Look at a coal piece
  to highlight it, then E to pick it up. Taking it out restores ordinary item
  physics. Bring it to the cab, aim inside the enlarged open furnace and E to
  feed held coal. Alternatively, throw coal through its opening with Q: a free
  piece entering the intake supplies the same +40 Steam / +5 Pressure, once.
  The intake ignores held coal until E or release, welded coal and other objects.
- Tap Q to drop, or hold Q and release to throw. Taken coal and purple ScrapTest
  items behave as ordinary physical items. German Z (physical Y on a US keyboard)
  welds a held item against a nearby suitable surface. Welded items cannot be
  picked up with E: aim at the highlighted item and Z to release it first.
- The locomotive's rear-right ladder uses E to climb to its roof and return.
  Both hands alternate onto the rungs during the climb and return to their resting
  pose afterwards. Rung positions follow the moving locomotive.
  The tender intentionally has no roof. The two passenger roofs remain connected
  by a short roof walkway; all four floor decks remain connected and walkable.
- Orange switches at carriage ends disconnect couplers. E on a free switch
  reconnects nearby compatible ends after stopping the train. Detached cars stay
  on their track positions. Brakes work without Steam, using Pressure; reverse
  is intentionally slower. The green platform strip marks the stop area.

Steam hiss fades with throttle and stops when braking or out of Steam. Each car
has a soft rolling rhythm that changes with speed and fades away when stopped;
uncoupled stationary cars stop producing it. Whistle and bell remain separate E
controls. Volumes, fade time and reference speed are configurable on TrainDriveAudio.
All four train clips are original synthesized placeholders generated with the
Python offline tool `Tools/create_train_sounds.py`.

## Technical scope

Runtime gameplay is C#. Single-player prototype with primitive geometry and a
straight/curved test railway. Rail motion, resources, couplers, weight, sound,
lever input and damage are separate modules under `Assets/Movement/Runtime/Train`.
The speedometer can still be broken with four presses of its test button; it then
reads BROKEN. Networking, Steam publishing, repairs and final train artwork are
future work. The supplied train screenshots guide later visual design.

Run **Window > General > Test Runner > PlayMode**. Checks cover movement on moving
cars, coal storage/pickup/firebox, precise Steam balance, one-hand lever input and
camera lock, brake strength, button/valve and ladder hand poses, thrown-coal intake,
hover outlines, gauge needles, throttle limits, sound response, couplers, weight and item physics.
The existing movement and object regression suite remains included.
