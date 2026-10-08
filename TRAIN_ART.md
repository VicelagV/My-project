# Train visual pass

Open `Assets/Movement/Scenes/TrainPlayground.unity`. The train retains the chunky silhouettes of Luca's train screenshots and the first movement video's faceted, matte shading and strong black hand outlines. Navy bodies, brass frames, red roofs and wheels, wood floors and upholstered seats keep the controls easy to distinguish.

Original generated meshes cover the boiler, chimney, domes, curved roofs, tender guards, coach windows, doors, spoked wheels, moving rods, buffers, springs and coupling links. Wheels and rods follow actual signed distance travelled. Smoke follows throttle and steam supply; cylinder steam responds to movement. These are visual effects; the existing train sounds remain in use.

The driver has two front windows and windows on both sides. The raised chair and lower boiler give a forward sightline above the boiler. Throttle, brake and reverse move with the raised armrests. Small bold ivory labels sit on brass-edged enamel plaques attached to the panel, armrests, valve wall and firebox frame. Idle hands sit lower while seated to keep the dials readable. Front safety glass has collision; decorative controls and frames do not add collision. The firebox still accepts held and thrown coal. Doors slide along the carriage instead of above its roof.

`All Aboard > Apply Train Visual Pass` updates only the owned decoration roots, train prefabs, coal shape, hand material and cab in the existing playground. It preserves the control components and references. Fresh train playground creation also applies the train appearance. Meshes and materials are project assets; no purchased or copied photo textures are included.

Real train photographs were used only as shape references:

- [Bluebell Railway SECR C-class](https://bluebell-railway.co.uk/bluebell/pics/c.html): boiler, bands and running gear.
- [North Norfolk Railway steam cab](https://nnrailway.co.uk/portfolio-items/steam-driver-experience-silver/): gauge housings, pipework and valve placement.
- [Swanage Railway Bulleid coach restoration](https://www.swanagerailwaytrust.org/post/bulleid-corridor-composite-s5761s): coach windows and body proportions.

The test playground environment is still the original physics test area. Route generation is the next phase; this visual pass does not implement route choices, stations, track repair or the Terminum economy.
