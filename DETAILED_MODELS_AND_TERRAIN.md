# Detailed models and terrain — 9 October 2026

Open `Assets/Movement/Scenes/ProceduralRailwayPlayground.unity` and press Play. New worlds apply the building, prop, equipment and ground-cover layers automatically. Train detail generators are installed on all three carriage prefabs; the bright character prefabs have updated eyes. Controls and economy remain the same.

## Model pass

- Locomotive: smokebox bolts, boiler-band clasps, feedwater plumbing, running-board treads, cylinder drainage pipes, headlamp cage, vent louvres and a driver footrest.
- Coaches and tender: brass panel mouldings, exterior cover plates and fasteners, bearing covers, brake rigging, tender reinforcement, door joinery, buttoned seat backs, cushion piping, carved seat caps, luggage rack slats, strapped cases and a patterned aisle runner. Door details attach to the existing hinge.
- Six frontier house forms: rear/side clapboard laps, weather strips, framed side sash windows, shutter louvres, sills, porch collars and knee braces, floor-plank joints, raised door panels, hinge hardware, standing roof seams, gutters/downpipes and chimney courses. Inhabited houses have small window planters where appropriate.
- Stations and Terminum: platform joints, iron canopy detailing fitted to each canopy height, detailed benches and a luggage trolley. Capital terraces gain door surrounds, transoms, kick plates, sash frames, keystones, corbels, cornice dentils and individual chimney pots.
- Markets and outfitters: counter joinery, brass hardware, canopy stays and canvas valances. Purchase plates have bevelled edges and slotted screw heads.
- Weapons: bevelled receivers, grips and stocks, sights, screw heads, stock/grip carving, retaining bands, revolver chambers/fluting and ejector rod, rifle bolt handle, shotgun action latch, wrapped melee grips and head fittings.
- Repair gear, freight and landmarks: saw blade/spine and grip detailing, connector jaw pads/teeth, forged wrench detail, crate plank joints/corner battens/handles, bridge splice plates/rivets/ties, tunnel masonry ribs/lamps/drainage, shed joinery, mine equipment and hoist footings, timber end grain, crane fittings, farm bales and ruin rubble.
- Characters keep their bright colours, rounded silhouettes and no eyebrows. Small eye highlights add depth.

New decoration has no collision shapes. Existing floors, walls, roof openings, doorways, rail clearance, pickup hulls and interactive controls supply collision as before. Static decoration is combined by material; moving door geometry stays separate. Generated meshes/materials are owned and released with the world. No external model or texture downloads are required.

## Terrain generation

Terrain uses a 16 m grid instead of 32 m, regional colour patches, tapered hills and mountain transitions, and a smoother transition around Bluefort. Main cities keep their flat pads. Tunnel placement still requires mountain terrain at least 120 m high and an appropriate straight rail section.

Seeded scatter creates meadow/forest grass, dry grass, shrubs, weathered rocks and sandstone clusters. Trees mix conifers and broadleaf forms. Scatter stays away from rails, settlement footprints, capital approaches and landmark sites. Grass blades and tree trunks use the triangulated terrain surface so they do not float above sloping ground. Rock bases follow the terrain triangles and steep faces are excluded from scatter. Tunnel mountain footprints stay clear. A height cache avoids recalculating shared terrain vertices. Ground-cover meshes are divided into spatial cells for culling; the cover adds no individual grass/rock physics colliders.

## Preview and checks

The preview gallery contains actual Unity camera renders of the generated models and terrain. Its weapon display bench is capture-only. The terrain statistics file records the preview seed's cluster and mesh counts. Generation is denser and the detail increases geometry; loading and frame rate on a packaged build still need playtesting. Automatic checks cover the rail corridor, terrain surface/collider agreement, decoration batching and collision isolation, plus the existing movement, train, interaction, repair, route, economy and combat suite. The previews inspect the playable seed, rather than claiming every possible generated world is visually perfect.

To rebuild train/character asset setup after modifying source, run **All Aboard > Refine All Model Details**. Cosmetic generators remain in C#; offline capture and asset coordination use Python.
