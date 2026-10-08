"""Offline, original placeholder whistle and bell sounds for the train prototype."""
import math
from pathlib import Path
import struct
import wave

root = Path(__file__).resolve().parents[1] / "Assets/Movement/Audio"
rate = 22050
for name, duration in (("TrainWhistle", 0.65), ("TrainBell", 0.8)):
    frames = bytearray()
    for sample in range(int(duration * rate)):
        t = sample / rate
        attack = min(1.0, t / 0.03)
        if name == "TrainBell":
            value = (math.sin(2 * math.pi * 660 * t) + 0.35 * math.sin(2 * math.pi * 1324 * t)) * math.exp(-6 * t)
        else:
            value = (math.sin(2 * math.pi * 880 * t) + 0.2 * math.sin(2 * math.pi * 1100 * t)) * min(1.0, (duration - t) / 0.12)
        frames.extend(struct.pack("<h", int(7000 * value * attack)))
    with wave.open(str(root / f"{name}.wav"), "wb") as sound:
        sound.setnchannels(1)
        sound.setsampwidth(2)
        sound.setframerate(rate)
        sound.writeframes(frames)
