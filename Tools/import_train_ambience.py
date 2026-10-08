"""Prepare Joseph Sardin's CC0 countryside recording as a seamless loop."""
from pathlib import Path
import numpy as np
import soundfile as sf
p=Path(__file__).resolve().parents[1]
audio,rate=sf.read(p/"Tools/audio_sources/countryside-0097.mp3")
audio-=np.mean(audio,axis=0)
audio*=0.6/max(0.001,float(np.max(abs(audio))))
f=int(rate*4)
out=audio[f:].copy()
weight=np.linspace(0,1,f)[:,None] if audio.ndim>1 else np.linspace(0,1,f)
out[-f:]=audio[-f:]*(1-weight)+audio[:f]*weight
sf.write(p/"Assets/Movement/Audio/CountrysideLoop.wav",out,rate,subtype="PCM_16")
print("CountrysideLoop",len(out)/rate,"seconds",out.shape)
