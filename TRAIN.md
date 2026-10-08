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
increasing Steam use. Normal Steam range remains 20–80; low Steam reduces pulling
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
- Cab buttons use E. Top row, left to right: brake, reverse, firebox, vent.
  Lower row: whistle, bell, door, speedometer damage test. Only gauges have labels;
  world signs, button labels and the default debug HUD have been removed/hidden.
- The black open tender has low side guards and a clear center aisle. Coal stock
  stays fixed to it through acceleration, braking and turns. Look at a coal piece
  to highlight it, then E to pick it up. Taking it out restores ordinary item
  physics. Bring it to the cab, aim at the firebox button and E to consume it.
- Tap Q to drop, or hold Q and release to throw. Taken coal and purple ScrapTest
  items behave as ordinary physical items. German Z (physical Y on a US keyboard)
  welds a held item against a nearby suitable surface. Welded items cannot be
  picked up with E: aim at the highlighted item and Z to release it first.
- The locomotive's rear-right ladder uses E to climb to its roof and return.
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
camera lock, throttle limits, sound response, couplers, weight and item physics.
The existing movement and object regression suite remains included.
