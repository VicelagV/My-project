# Border railway: world and repair prototype

## Established design

The setting is a loosely American Wild West border region: mostly flat countryside, forests, deserts, hills and occasional mountains. Redfort and Terminum occupy low flatlands; Bluefort has hills around a level station site. Towns have attractive but simple house facades. Stations, markets and locations that plausibly contain valuables are accessible. The capital has its own clean Victorian London style, with an extensive approach skyline, masonry terraces, wider streets, open squares and a grand station.

The railway retains its existing opening forest, first junction, supply stop, three route families, crossovers, Bluefort convergence and Terminum entry/bypass choice. A direct safe journey remains about fifteen minutes at full throttle. Optional settlement counts are one on safe routes, three on high-risk routes, and a seeded random one to three on mixed routes. Frontier, mining, lumber, isolated and abandoned settlement types are included in the generator. Villages have one shop plus a 50% chance of a second. Towns have two guaranteed shops plus two independent one-third rolls. An isolated station has no shops and is a passenger drop-off destination, with a fare multiplier of five relative to Terminum.

The three rail damage types are brittle, strained and missing. Brittle and strained rails have a maximum safe speed of 3 m/s. Brittle rails are crossed slowly. Strained rails can be crossed slowly or cut out with the onboard saw and replaced. Missing rails require steel. Each replacement operation uses at most five metres; several pieces can bridge a longer gap. Both parallel rails must be repaired. Sleepers do not consume repair boards.

## Playable in this pass

- Deterministic settlement placement and asymmetric frontier streets with six building forms: false-front shops, gabled cottages, balcony shops, log cabins, workshops and stone bay-window cottages. Individual shops and detailed station platforms/rooms.
- Terminum has 32 masonry terrace blocks in offset rows, streets, promenades, a square and a clock-tower station hall. Its skyline begins before arrival at the station.
- Plains, forest, desert/cactus, substantial hills and mountains; river bridges, canyons, farms, mines, logging camps, ruins, a border office and a camp location. Tunnels only appear at eligible straight stretches within large mountain regions, away from settlements; some seeds have no tunnel. Their mountain shell is at least 120 m high. Terrain is carved beneath bridge crossings and the deck remains walkable.
- Physical salvage crates at plausible supply/loot locations. Carry a crate to a vendor and press a purchase button to sell it.
- Standard vendors sell steel, coal, a saw and a connector through four labelled physical purchase buttons. Extra all-purpose town shops offer discounted mixed stock. M1 or E presses a button; a brief press animation prevents duplicate purchases and brings the hand to it. Purchases spawn handheld goods when appropriate.
- Each day starts with 500 Gold and 20 metres of steel. The saw and connector are in the coal wagon's labelled repair locker. Its plaque shows the wallet and remaining steel.
- Look at a strained rail's marked mount while holding the saw and press E to place it. Hold M1 on the saw and alternate forward/backward mouse strokes; eight alternating strokes complete a cut. E takes the saw back. Repeat for the other rail.
- Hold the connector and press E on a missing rail's starting endpoint. The mint-green preview shows the next piece, up to five metres long. Walk to the far endpoint and press E to commit it. Steel is spent only on a successful connection. Repeat from the new starting endpoint for the remainder and then repair the second rail. Dropping the connector cancels its preview.
- Saw mounts reject connectors, and missing-rail connections reject saws or ordinary objects. A far endpoint rejects an unaligned connector.
- Missing rails derail the train. Brittle/strained rails can derail it above 3 m/s. A derailment stops this prototype's day and leaves the consist tilted; restart Play mode to try again.
- Passenger/cab doors rotate about a fixed hinge, rather than sliding. Onboard push buttons use brass plates, enamel insets, collars and screws. Signs and lettering are mounted and depth-tested so they cannot show through opaque walls.

## Current tuning and interpretation

| Item | Price |
| --- | ---: |
| 5 m steel | 50 Gold |
| Coal lump | 20 Gold |
| Replacement saw | 25 Gold |
| Replacement connector | 15 Gold |

Steel pricing was delegated to implementation. Fifty Gold is 10% of the starting daily budget. The train starts with its repair tools, so their prices cover replacements rather than mandatory starting purchases. Free prototype coal remains at the forest supply stop; Redfort's market charges for its supplies.

The two saw cuts are interpreted as **one per parallel rail**, rather than four cuts or two cuts across the entire pair. This is the one design interpretation to confirm later if a different physical motion was intended.

The provisional derail roll rises linearly from zero at 3 m/s to certainty at 12 m/s. Each car rolls once when crossing a damaged section at unsafe speed; accelerating while on the section triggers a check too. Missing track always derails. This is configurable gameplay tuning, not a confirmed probability curve.

## Systems that are still future work

See ECONOMY_PROTOTYPE.md for the implemented passenger spawning, fare payouts, capital entry fee, credit and day settlement. Passenger boarding, physical freight, happiness, day-boundary persistence, train repairs and run upgrades are now implemented; see SERVICE_AND_MAINTENANCE.md. A first bandit encounter, weapons and medical supplies are implemented; see WEAPONS_AND_ENCOUNTERS.md. Full risk balancing, border inspections, severe weather effects and co-op/Steam networking remain future work. The camp and border office are environment foundations.

The route still stops at day end, as requested for this prototype. No new day is automatically generated. Day accounting now runs at Terminum or the bypass return; saved progression and a manual next-day button are now available.

## Validation

Runtime and editor code is validated in a separate Unity 6000.6.4f1 checkout. The Play mode suite covers existing movement/train behavior and the new economy, settlement counts, correct tool requirements, alternating saw strokes, multi-piece repairs, hinged doors, purchase controls, derailment and landmark/skyline placement. In-engine renders show the actual generated scenes, purchase buttons, city signage, capital approach and repair before/preview/after states.

The completed pass passed all 92 Play mode tests. The default seed's previews have no tunnel: generation correctly rejected its unsuitable tunnel sites rather than placing tunnels in small hills.

## Village roads update — 9 October 2026

Villages use a bent main street with house entry paths, a back lane, a marketplace connection and a road to the station steps. Mining/lumber yards get another road. Short boardwalks, hitching rails and a water trough replace the identical long town boardwalk slabs in villages. Road ribbons are closed meshes and stay clear of the railway.
