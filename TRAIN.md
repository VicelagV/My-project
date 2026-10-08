# Ugly train prototype

Open `Assets/Movement/Scenes/TrainPlayground.unity` and press Play.
If missing, use **All Aboard > Create Train Playground**. The original movement
playground stays available separately; the build scene list is unchanged.

You spawn on the station platform beside the coal wagon. Enter its open side
door, then walk through the center aisles and short connecting walkways to the
front locomotive. WASD, mouse look, Space, Shift sprint and hold C crouch apply.

## Cab and conductor seat

- Aim at the conductor seat and press E to sit. You can look around while seated.
- The two buttons on its left armrest raise/lower throttle by 25% with E.
  They only work for the seated occupant. Standing beside them cannot operate them.
- To stand, look away from cab controls and press E. Movement and jumping are
  locked while seated; leaving the seat restores walking.
- Aim at the physical cab buttons and press E: brake toggle, reverse toggle,
  firebox, vent, whistle, bell, side-door toggle, and speedometer damage test.
- Brakes override throttle. Release brakes to drive again. Reverse is slower;
  use brakes to stop before changing direction for predictable maneuvers.
- The roof ladder is at the rear right of the locomotive. Aim at its orange
  interaction block and press E to climb; the upper block returns you inside.
  Climbing follows the moving car. Flat roofs and short bridges connect all cars.

## Objects, resources and couplers

- Ten physical black coal cubes start in the storage car. Aim and E to pick up,
  carry one to the locomotive, then aim at the FIREBOX button and E to consume it.
  Each adds 10 Steam and 5 Pressure, clamped to 100. Other items are rejected.
- Tap Q to drop or hold Q and release to throw. Hands return after release.
- Two purple ScrapTest items start in the rear passenger car. Carry one near a
  solid train surface and press German Z (physical Y on a US keyboard) to weld.
  Welded items contribute their mass to the connected train and cannot be picked
  up with E. Aim at a highlighted welded item and press Z to free it first.
- The orange coupler switches near the ends of cars disconnect linked cars.
  E on a free switch reconnects a nearby compatible coupler once stopped.
  Detached cars stay on their rail positions, with their interiors and doors
  usable; reverse the locomotive toward them to reconnect.
- Steam starts at 50, Pressure at 40. Normal Steam range is 20–80. Low Steam
  reduces acceleration and speed; high Steam allows overspeed but increases
  Steam use and Pressure gain. At 100 Steam a red critical warning appears.
  Venting removes 20 Pressure and costs 10 Steam. Brakes use Pressure and still
  work without Steam, with reduced force when Pressure is exhausted.
- Connected cars, players aboard and welded items add weight. More weight slows
  acceleration without directly increasing Steam consumption. Extra Test Weight
  on TrainWeight can demonstrate this quickly in the Inspector.
- DAMAGE TEST removes 25 speedometer condition per use. At zero its display
  reads BROKEN and the debug speed readout also stops reporting accurate speed.
  Steam, Pressure and train motion keep working.

The railway includes a long straight, a gentle curve, end buffers, a station
platform with a green stop marker, and two basic rooms. Cars follow sampled
rail distance; visual wheels have no wheel physics. Stop beside the platform
using brakes, or reverse back toward it. End-of-track limits stop the consist.

## Technical scope

Single-player prototype, primitive geometry, temporary values and original soft
synthesized whistle/bell sounds. Runtime gameplay is C#; `Tools/create_train_sounds.py`
is a Python offline audio generator. There is no multiplayer, Steam publishing,
repair system or final artwork here. Doors slide upward and the ladder uses a
short guided climb; both can be replaced with designed assets later.

Movement, rail following, resources, coupling, weight, controls and component
condition are separate modules under `Assets/Movement/Runtime/Train`. Tune their
serialized values in the Inspector. `TrainPrototypeBuilder` generates the scene
and reusable car/item prefabs without overwriting an existing scene.

Run **Window > General > Test Runner > PlayMode** for the existing movement and
object regressions plus train scene tests. These cover interiors, moving roofs,
ladder travel, jumps, seated throttle restrictions, train controls, resources,
coupling, weight, damage and loose/welded items. Automated validation does not
replace your own play-through for subjective feel and final visual design.
