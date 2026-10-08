# Train visual pass

Open `Assets/Movement/Scenes/TrainPlayground.unity`. The train retains the chunky silhouettes of Luca's train screenshots and the first movement video's faceted, matte shading and strong black hand outlines. Navy bodies, brass frames, red roofs and wheels, wood floors and upholstered seats keep the controls easy to distinguish.

Original generated meshes cover the boiler, chimney, domes, curved roofs, tender guards, coach windows, doors, spoked wheels, moving rods, buffers, springs and coupling links. Wheels and rods follow actual signed distance travelled. Smoke follows throttle and steam supply; cylinder steam responds to movement. These are visual effects; the selected horn and bell are documented in TRAIN_SOUND_CREDITS.md.

The driver has two front windows and windows on both sides. The raised chair and lower boiler give a forward sightline above the boiler. Throttle, brake and reverse move with the raised armrests. Small bold ivory labels sit on brass-edged enamel plaques attached to the panel, armrests, valve wall and firebox frame. Idle hands sit lower while seated to keep the dials readable. Front safety glass has collision; decorative controls and frames do not add collision. The firebox still accepts held and thrown coal. Doors slide along the carriage instead of above its roof.

`All Aboard > Apply Train Visual Pass` updates only the owned decoration roots, train prefabs, coal shape, hand material and cab in the existing playground. It preserves the control components and references. Fresh train playground creation also applies the train appearance. Meshes and materials are project assets; no purchased or copied photo textures are included.

Real train photographs were used only as shape references:

- [Bluebell Railway SECR C-class](https://bluebell-railway.co.uk/bluebell/pics/c.html): boiler, bands and running gear.
- [North Norfolk Railway steam cab](https://nnrailway.co.uk/portfolio-items/steam-driver-experience-silver/): gauge housings, pipework and valve placement.
- [Swanage Railway Bulleid coach restoration](https://www.swanagerailwaytrust.org/post/bulleid-corridor-composite-s5761s): coach windows and body proportions.

The original physics test area remains available in TrainPlayground. The separate generated railway, route choices and prototype stations are documented in ROUTES.md; track repair and the Terminum economy remain future work.

## Coach and track refinement

Wall infills now fill the exact spaces between windows and the sliding doorway;
flat panel meshes close seams while seats and trim retain bevelled edges. Front
cab corner jambs, centre mullion and a wider bulkhead close the remaining front gaps.
The passenger layout has eight two-person seat pairs (sixteen seats), a centre
aisle, clear side-door access, armrests, brass piping and overhead luggage shelves.
Rails follow the curve continuously with visible head, web and foot profiles,
fastening plates, closer sleepers and a sloped ballast bed. Shared approach segments
are deduplicated when their sampled geometry matches.

## Janney couplers and ride feel

Chunky cast housings, hooked knuckles, vertical hinge pins, draft collars and
uncoupling rods replace the old link rings. Knuckles open when disconnected and
close when coupled. The existing E coupling controls remain available.
Reference: [McConway & Torley knuckles and coupler parts](https://mcconway.com/capabilities/knuckles-coupler-parts/).

Coal and passenger cars gently sway at speed (up to 0.35 degrees of roll,
8 mm sideways and 6 mm vertically). The whole kinematic car moves, so riders
feel the movement. Sway fades out when stopped or detached.
