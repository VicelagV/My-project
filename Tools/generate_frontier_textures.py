"""Generate the original, tileable surface grain used by the frontier style.
Requires numpy and Pillow. Existing Unity texture GUIDs are preserved.
"""
from pathlib import Path
import argparse
import uuid
import numpy as np
from PIL import Image


def generate(output: Path) -> None:
    output.mkdir(parents=True, exist_ok=True)
    rng = np.random.default_rng(20261010)
    size = 128
    y, x = np.mgrid[0:size, 0:size]
    for name in ["Earth", "Gravel", "Wood", "Foliage", "Metal"]:
        noise = rng.normal(0, 1, (size, size))
        broad = np.sin(x * 2 * np.pi / size * 3) * np.cos(y * 2 * np.pi / size * 4)
        if name == "Wood":
            value = 237 + 7 * np.sin(x * .24 + np.sin(y * 2 * np.pi / size) * 1.5) + noise * 2 + 3 * broad
        elif name == "Gravel":
            value = 228 + noise * 12 + broad * 4
        elif name == "Earth":
            value = 233 + noise * 7
        elif name == "Foliage":
            value = 236 + noise * 5
        else:
            value = 246 + noise * 1.8 + broad * 1.5
        grey = np.clip(value, 190, 255).astype("uint8")
        path = output / (name + ".png")
        Image.fromarray(np.stack([grey, grey, grey], axis=-1)).save(path)
        meta = Path(str(path) + ".meta")
        if not meta.exists():
            meta.write_text("fileFormatVersion: 2\nguid: " + uuid.uuid4().hex + "\n", encoding="utf-8")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", type=Path, default=Path(__file__).resolve().parents[1] / "Assets/Movement/Resources/FrontierTextures")
    generate(parser.parse_args().output)
