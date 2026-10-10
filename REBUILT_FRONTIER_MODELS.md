# Rebuilt frontier models — 10 October 2026

The previous pass mostly changed light and materials. This pass replaces the visible geometry.

- Train: broad cream-framed windows and closed wall infills, navy barrel roofs, a heavier faceted boiler, wide boiler jackets, tapered stack, larger lamp and cylinders, six-spoke wheels and new timber doors.
- Coaches: clipped upholstered backs, broad cushions, solid timber arms, luggage racks and ceiling lamps. Tender: thicker guards, rolled lips and stays.
- Houses: six new facades with heavy porches, framed sash windows, new roofs and chimneys; false fronts, dormers, balconies, log walls, workshop doors and projecting stone bays. Abandoned windows are boarded.
- Vegetation: five jagged layers per fir, tapered trunks, rounded broadleaf crowns and shrubs, flat-topped boulders and compact bent grass tufts.

The original wheel animation, hinge pivots, control objects, ladders and collision hulls are retained. New visible decoration is batched by material and adds no train collision shapes. Weapons, character silhouettes, stations and major landmarks retain their existing geometry in this pass.

Open `Assets/Movement/Scenes/ProceduralRailwayPlayground.unity` and press Play to see the runtime rebuild. Screenshots in this folder are rendered by Unity from that scene, using the same seed and cameras as the previous train/forest preview.

## Validation

Unity 6000.6.4f1: 158/158 PlayMode tests passed, 0 failures. Full results are in playmode-results.xml. Actual scene capture completed without compile errors. Closed-mesh tests also check boundary edges, collapsed faces and outward winding for the newly authored cylinders, wheel rims and seat backs.
