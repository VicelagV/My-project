"""Convert the user-selected prototype signals to normalized mono PCM.
Requires numpy and soundfile. Source/license status: TRAIN_SOUND_CREDITS.md.
"""
from pathlib import Path
import numpy as np
import soundfile as sf
root=Path(__file__).resolve().parents[1]
for source,target in [("trainz-default-horn.mp3","TrainWhistle"),("boxing-round-bell.mp3","TrainBell")]:
    audio,rate=sf.read(root/"Tools/audio_sources"/source)
    if audio.ndim>1: audio=audio.mean(axis=1)
    audio-=audio.mean()
    audio*=0.82/max(0.001,float(np.max(abs(audio))))
    fade=min(int(rate*0.005),len(audio)//2)
    audio[:fade]*=np.linspace(0,1,fade);audio[-fade:]*=np.linspace(1,0,fade)
    sf.write(root/"Assets/Movement/Audio"/(target+".wav"),audio,rate,subtype="PCM_16")
    print(target,len(audio)/rate,rate)
