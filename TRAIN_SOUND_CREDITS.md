# Prototype train signals

The user selected these files on 8 October 2026:
- `TrainWhistle.wav`: `Tools/audio_sources/trainz-default-horn.mp3`.
- `TrainBell.wav`: `Tools/audio_sources/boxing-round-bell.mp3`.

The user reports that both files are license-free sounds. Exact source links,
authors and license text have not been supplied; retain those details when
preparing the distributed game credits.

Changes: stereo-to-mono where needed, DC removal, peak normalization to 0.82,
5 ms edge fades, 16-bit PCM WAV export. The complete clips are retained.
`Tools/import_train_signals.py` reproduces the conversion. The original source
files are retained beside the script. `Tools/create_train_sounds.py` only
regenerates only the original steam hiss.

## CountrysideLoop.wav

Countryside (#0097) by Joseph SARDIN, BigSoundBank.
Source: https://bigsoundbank.com/countryside-s0097.html
License: CC0, https://creativecommons.org/publicdomain/zero/1.0/
Wind in trees, birds and insects. Original MP3 retained in Tools/audio_sources.
Changes: DC removal, peak adjustment and a four-second loop crossfade, exported
as stereo PCM WAV. Reproduce with Tools/import_train_ambience.py.

## Driving sound layers

TrainRollingLoop.wav derives from Steam locomotive #1 (#3016), Joseph SARDIN.
Source: https://bigsoundbank.com/steam-locomotive-1-s3016.html
A recording from the first car of a steam train in Martel, France.
Changes: excerpt 8–24 seconds, mono mix, filtering to focus on wheel/rail rumble,
level adjustment and a 0.5-second loop crossfade.

TrainChuffLoop.wav derives from Steam engine, foley #2 (#3311),
Joseph SARDIN & Axeline T.
Source: https://bigsoundbank.com/steam-engine-foley-2-s3311.html
A studio performance of locomotive steam-engine chuffing.
Changes: excerpt 4–20 seconds, DC removal, level adjustment and loop crossfade.

Both sources are CC0: https://creativecommons.org/publicdomain/zero/1.0/
Original MP3 files remain under Tools/audio_sources. Reproduce with
Tools/import_driving_audio.py. Steam hiss remains an original synthesized layer;
Tools/create_train_sounds.py now regenerates only that hiss.
