# Seeded railway prototype

Open `Assets/Movement/Scenes/ProceduralRailwayPlayground.unity` and press Play.
The existing TrainPlayground remains the separate physics/control test scene.
Use **All Aboard > Preview Current Procedural Railway** for an editor-only preview
of the generated world. Generated meshes are rebuilt at startup and are not saved
as scene assets. The creation menu preserves an existing procedural scene.

## Journey

Redfort -> forest and marked damaged-track site -> safe/risky starter split ->
forest supply stop -> Safe/Mixed/Hard routes with adjacent-route crossovers ->
Bluefort -> Terminum entry or bypass. The bypass follows a short return section
and stops the train at the day-end signs. It does not generate the next day or
physically loop back to Redfort. Restart Play to test another trip.

Safe includes Brasswood town and a guaranteed station. Other route families have
seeded station placement; both starter branches reunite at the supply stop.
Difficulty names establish the route structure; hazards, combat, passenger and
cargo rewards, repairs and economy are future work. Damaged track is marked and
remains drivable. Terminum entry/bypass is manually selected; no payment is charged.
The proposed 1000 Gold entry fee is retained as future route metadata only.

## Controls and resources

Stop before a junction, leave the conductor seat, approach the trackside lever
with empty hands, aim at it and press E to cycle its mounted route label. The
lever moves, highlights white and receives the player's hand interaction.
Switches lock while the train moves or any car, including a detached car, reaches
the protected junction area. Changing upcoming points preserves train position,
resources and coupler connections. The default itinerary follows Safe to Terminum.

Aim at a station's COAL crate and press E to take a physical coal piece. Supplies
are unlimited and free for prototype testing. The existing furnace accepts held
or thrown coal; each piece adds 40 Steam once. Full throttle consumes 0.5 Steam
per second, with lower throttle using proportionally less. Existing driver,
movement, welding, coupler and ladder controls remain available; see TRAIN.md.

## Generation and timing

On Procedural Train Systems, edit the ProceduralRailway seed, day and driveMinutes
before Play. The same seed/day reproduces the graph, curves, stops and terrain;
generation uses its own RNG without consuming gameplay randomness. Changing day
selects a new deterministic seed; the fixed opening and required stops remain.

The default Safe itinerary is calibrated to approximately 15 minutes at 12 m/s,
excluding stops and repairs. Acceleration, throttle, Steam boost/shortage, route
choices and brakes change actual travel time. Landscape is faceted terrain with
trees, rail corridors and simple walkable station rooms. Spatial mesh batches
support culling; this prototype generates the whole network at startup rather
than streaming it. Stations align with the train floor and provide boarding steps.

## Validation

PlayMode tests cover repeatable seeds, required graph connections, route
crossovers, nominal journey length, terrain clearance, station boarding height,
physical switch highlighting and lever motion, occupied-point protection,
physical supply coal, and a simulated full bypass trip that stops at day end.
The existing movement, hands, train controls and resource tests remain included.
