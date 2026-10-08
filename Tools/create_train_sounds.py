"""Offline original steam and rolling loops. Recorded signals use import_train_signals.py."""
import math
from pathlib import Path
import struct
import wave
import random

root = Path(__file__).resolve().parents[1] / "Assets/Movement/Audio"
rate = 22050
# Periodic filtered noise gives seamless, quiet loops without a sharp seam click.
duration = 4.0
count = int(duration * rate)
rng = random.Random(1729)
noise = [rng.uniform(-1, 1) for _ in range(count)]
filtered = []
low = 0.0
for index in range(count * 2):
    low += 0.12 * (noise[index % count] - low)
    if index >= count:
        filtered.append(low)
for name in ("TrainSteamLoop", "TrainRollingLoop"):
    frames = bytearray()
    for index in range(count):
        t = index / rate
        if name == "TrainSteamLoop":
            breath = 0.65 + 0.35 * math.sin(2 * math.pi * 2 * t) ** 2
            value = (noise[index] - filtered[index]) * breath * 0.24 + filtered[index] * 0.2
        else:
            cycle = (t * 4) % 1.0
            clack = math.exp(-cycle * 35) * math.sin(2 * math.pi * 160 * t) * 0.28
            value = clack + filtered[index] * 0.65 + math.sin(2 * math.pi * 55 * t) * 0.035
        frames.extend(struct.pack("<h", int(16000 * value)))
    with wave.open(str(root / f"{name}.wav"), "wb") as sound:
        sound.setnchannels(1)
        sound.setsampwidth(2)
        sound.setframerate(rate)
        sound.writeframes(frames)
