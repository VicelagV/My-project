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
regenerates the original steam and rolling loops.

## CountrysideLoop.wav

Countryside (#0097) by Joseph SARDIN, BigSoundBank.
Source: https://bigsoundbank.com/countryside-s0097.html
License: CC0, https://creativecommons.org/publicdomain/zero/1.0/
Wind in trees, birds and insects. Original MP3 retained in Tools/audio_sources.
Changes: DC removal, peak adjustment and a four-second loop crossfade, exported
as stereo PCM WAV. Reproduce with Tools/import_train_ambience.py.
