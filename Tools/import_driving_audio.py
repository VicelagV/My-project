"""Prepare CC0 train rumble and engine foley. Credits in TRAIN_SOUND_CREDITS.md."""
from pathlib import Path
import numpy as np
import soundfile as sf
p=Path(__file__).resolve().parents[1]
for number,target in [(3016,"TrainRollingLoop"),(3311,"TrainChuffLoop")]:
    a,r=sf.read(p/"Tools/audio_sources"/("driving-"+str(number)+".mp3"))
    if a.ndim>1:a=a.mean(axis=1)
    a=a[int(8*r):int(24*r)] if number==3016 else a[int(4*r):int(20*r)]
    a-=a.mean()
    if number==3016:
        freq=np.fft.rfftfreq(len(a),1/r)
        band=np.clip((freq-25)/30,0,1)*np.clip((1100-freq)/400,0,1)
        a=np.fft.irfft(np.fft.rfft(a)*band,n=len(a))
    a*=0.75/max(0.001,float(np.max(abs(a))))
    fade=int(0.5*r);out=a[fade:].copy();blend=np.linspace(0,1,fade)
    out[-fade:]=a[-fade:]*(1-blend)+a[:fade]*blend
    sf.write(p/"Assets/Movement/Audio"/(target+".wav"),out,r,subtype="PCM_16")
    print(target,len(out)/r)
