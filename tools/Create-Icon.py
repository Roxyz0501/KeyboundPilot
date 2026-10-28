from pathlib import Path
from PIL import Image, ImageDraw
r = Path(__file__).resolve().parents[1]
s = 3
im = Image.new("RGB", (512*s, 512*s), (12, 23, 39))
d = ImageDraw.Draw(im)
def rr(box, radius, fill, outline=None, width=1):
    d.rounded_rectangle(tuple(int(x*s) for x in box), radius*s, fill, outline, width*s)
def line(points, fill, width):
    d.line([(int(x*s), int(y*s)) for x,y in points], fill=fill, width=width*s, joint="curve")
rr((22,22,490,490),92,(20,39,59),(54,83,105),2)
rr((55,137,457,377),32,(8,17,30),(85,139,155),5)
for y in (165,225):
    for x in (80,140,200,260,320,380):
        rr((x,y,x+43,y+43),9,(45,94,112))
rr((82,286,130,344),10,(45,94,112))
rr((143,286,369,344),10,(76,157,167))
rr((382,286,430,344),10,(45,94,112))
rr((79,160,184,270),17,(245,188,83),(255,224,158),3)
line([(109,184),(109,244)],(30,47,63),10)
line([(153,183),(116,214),(156,246)],(30,47,63),10)
d.ellipse((319*s,315*s,455*s,451*s),fill=(12,28,46),outline=(245,188,83),width=8*s)
line([(387,336),(387,383),(417,400)],(104,216,209),8)
d.ellipse((381*s,377*s,393*s,389*s),fill=(245,188,83))
out=r/"KeyboundPilot/images/icon.png"
out.parent.mkdir(parents=True,exist_ok=True)
im.resize((512,512),Image.Resampling.LANCZOS).save(out)
print("Original icon: 512x512 PNG")
