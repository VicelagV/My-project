# Service, maintenance and village roads — 9 October 2026

Open **Assets/Movement/Scenes/ProceduralRailwayPlayground.unity** in Unity and press Play. Open the service ledger with **Tab**. The onboard ledger button and station service consoles also open it.

## Passengers

The starting travellers now have destinations on all three route families, plus Bluefort and Terminum. Additional travellers wait at Redfort, villages, towns and Bluefort. Isolated stations are drop-off only. Board with E on a traveller, a station boarding button, or the ledger. Choose travellers individually if seats are scarce. The capacity is 16; disconnected coaches cannot receive passengers.

The ledger shows destinations, expected fares, happiness and delays. Rough driving, emergency braking, poor train condition, late journeys and missed destinations reduce happiness. Unhappy travellers leave at the next stop without paying. Missing their preferred stop pays only the 100-Gold fallback at Terminum. Rare bankers at high-risk settlements pay a much higher fare and have less tolerance.

## Freight

Station consoles offer physical contracts. Accepting creates a labelled, carryable crate. Load it on the train and bring it within four metres of the correct arrival desk, then press Deliver Freight. Arriving with a crate aboard is not enough. Intact crates pay 100%, damaged crates 75%; lost or unfinished contracts create a 35% obligation, once. Terminum waits for delivery or your explicit settlement while contracts remain outstanding. Freight mass now affects train weight.

## Repairs and upgrades

A starting wrench is stored on the right of the coal carriage. Boiler, brake and coach service panels show their condition. Hovering previews the repair patch. Hold the wrench and press E at a damaged panel to mount it. Hold M1 and move the mouse forward/back for eight alternating strokes. Press E to retrieve it.

A matching replacement part restores full condition and uses one part. Without it, one Scrap and one Screws restore 35 points; these temporary repairs wear twice as quickly. Supplies are consumed on completion. Starting supplies include a boiler part, brake part, two coach parts, two Scrap, four Screws and two Wood. Repair merchants carry eleven products; discount merchants have four varied products. Boiler parts cost 275 Gold; brake and coach parts 150; the wrench 25.

Pressure above 90 while using throttle wears the boiler; hard braking above 3 m/s wears the brakes. Overspeed damaged-track impacts damage the train. Condition reduces engine and brake effectiveness and passenger comfort. Damaged panels turn rusty; completed repairs leave visible patches. The existing day-end damage bill accounts for these parts.

At Redfort, the Run page sells four permanent upgrades for this run: steam efficiency (500 Blumt base, 8% less consumption per level), brakes (400, +15% per level), comfort (400, 20% less rough-driving loss per level), reinforcement (600, 25% less wear per level). Maximum level three; the next level costs its base price times the level number. Upgrades save with run progression. The unmodified full-throttle drain remains 0.5 Steam/second and coal still supplies 40 Steam.

You can hire one temporary storage carriage for 500 Gold at Redfort. It adds a connected open cargo wagon for this day, with no duplicated coal or tools.

## World changes

Damaged rails now have actual cracks, bends or missing metal. Sawing removes the affected rail; replacement steel fills it in stages, with visible joints. The warning sign updates when repaired. Brittle and strained rails retain the 3 m/s limit.

Villages have a bent main street, back lane, house entry paths, a marketplace link and a road to the station steps. Mining and lumber villages also get a work-yard road. Short frontage boardwalks, a trough and hitching rails add detail. The roads stay clear of the railway and retain the simple chunky style.

## Saves and limits

Successful settlement saves the next day, Blumt, carried repair debt, seed, difficulty and upgrade levels. Starting the next day is manual in the Run page. Upgrade purchases save during Redfort preparation. Saves are written atomically and an invalid primary file falls back to its backup.

This is day-boundary progression, not a mid-journey world save: loose objects, current cash and train positions are not restored. Temporary carriage hire resets each day. A failed quota or derailment ends the run. Basic weapons, healing and one hostile bandit camp are now available; see WEAPONS_AND_ENCOUNTERS.md. Kidnapping, multiplayer, persistent physical warehouses and full weather systems remain later game systems. The economy numbers are centralized for further playtesting; their presence is not a claim that the complete economy is finally balanced.

## Preview

The accompanying images are real Unity camera renders of the generated scene, not concept art. They cover roads, physical service consoles, travellers, freight, rail damage/repair and the mounted wrench. They do not capture the Tab GUI panel itself.
