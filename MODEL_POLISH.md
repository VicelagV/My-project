# Model cleanup — 9 October 2026

This pass repairs the current generated models and adds restrained details in the established chunky style.

## Repaired geometry

- Frontier houses, station rooms, capital terraces and landmark sheds use closed pitched roof shells and closed attic gables. Roof slopes and gable heights come from one pitch calculation. Eave thickness and modest overhangs close the roof ends.
- Capital roofs rise to a centre ridge rather than away from it. Cornices and downpipes finish the sides. Grand hall windows now stay inside the wing's end walls. Terminum's old low canopy and dirt street no longer overlap the capital replacements.
- Porch posts reach their roofs or balcony decks. Workshop double doors replace the extra decorative single door and sit on the porch threshold. Steps, balcony side rails, chimney caps and gable tie beams finish the fronts.
- Train roofs are solid meshes with closed eaves, end faces and ladder-opening edges. The deliberate ladder hatch remains clear. Roof walking strips no longer share a coplanar top surface with the curved roof.
- Floating ladder interaction boxes are hidden and their targets moved onto new ladder uprights and handgrips. Their controls and climbing behavior remain available.
- Lantern corner frames expose their light cores instead of hiding them in solid boxes. Station clocks hang from the canopy. Waiting benches and cabin lamps have supports; luggage shelf brackets meet the walls.
- Repair tools sit clear of the locker face, on visible mounting pegs. The saw has a framed handgrip and rivets; the connector has a steel spine and grip bands. Salvage crates start clear of shed floors.
- Tunnel mountain sides are closed and the approach/exit face winding points outward. Continuous stone portal arches replace disconnected blocks, with fine mortar joints. The deliberate train opening remains clear.

## Small additions

Coach boarding steps, body fasteners, suspension mounts and coupler brake hoses; framed shed entrances and cladding; a four-legged mine headframe with diagonal braces, winding drum and cable; a crane cable and hook; separate stones and firewood for the camp fire; an actual rimmed fountain basin with visible water.

## Verification

Unity 6000.6.4f1 in a separate validation checkout. Play mode checks cover existing movement/train/repair interactions plus closed mesh seams for the roof shells and shared train parts. Collision ray checks verify the ladder hatch and tunnel bore remain open while the surrounding surfaces are closed and face outward. Actual Unity close-ups and world previews accompany the pass.

All 106 Play mode tests passed. The final cosmetic plaque support was then compiled and inspected with the in-engine preview pass.

The model audit covers the locomotive, tender, passenger coaches, controls, repair gear, all six frontier house forms, stations, capital buildings, market stalls and landmark structures. Doorways, glass windows, the ladder hatch, tunnel openings and ruined walls are intentional openings. Procedural terrain across every possible seed cannot be exhaustively previewed; previews use the existing playable seed and an isolated tunnel-model inspection.
