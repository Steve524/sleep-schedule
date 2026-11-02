"""Rebuild release art: python Docs/make_art.py (Pillow 12.3).

Original geometric art; uses local Segoe UI / Arial, then DejaVu / Pillow fallback.
Assignment RGBs: installed Core/Defs/Misc/TimeAssignmentDefs/TimeAssignments.xml
and Royalty/Defs/TimeAssignmentDefs/TimeAssignments.xml. Rows match Startup.cs.
Icon support verified by decompiling installed Assembly-CSharp.dll:
Verse.ModMetaData.ModMetaDataInternal.modIconPath exists; Icon uses
ContentFinder<Texture2D>.Get(ModIconPath), hence the Textures/ runtime copy.
Neighbor example: Mods/HugsLib/About/About.xml:19 uses modIconPath=HLModIcon.
"""
from pathlib import Path
import math
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
SCALE = 2
COLORS = {'Sleep': (51, 51, 127), 'Work': (133, 133, 61),
          'Recreation': (115, 74, 138), 'Meditate': (85, 185, 85),
          'Anything': (114, 114, 114)}
INK = '#eeeadd'
MUTED = '#a6b4bc'
GOLD = '#d8bd80'


def font(size, bold=False):
    candidates = ([r'C:\Windows\Fonts\segoeuib.ttf', r'C:\Windows\Fonts\arialbd.ttf']
                  if bold else [r'C:\Windows\Fonts\segoeui.ttf', r'C:\Windows\Fonts\arial.ttf'])
    candidates += ['DejaVuSans-Bold.ttf' if bold else 'DejaVuSans.ttf']
    for candidate in candidates:
        try:
            return ImageFont.truetype(candidate, round(size * SCALE))
        except OSError:
            pass
    return ImageFont.load_default(size=round(size * SCALE))


class Art:
    def __init__(self, width, height, transparent=False):
        self.w, self.h = width, height
        self.im = Image.new('RGBA' if transparent else 'RGB',
                            (width * SCALE, height * SCALE), (0, 0, 0, 0) if transparent else '#11191f')
        self.d = ImageDraw.Draw(self.im)
        if not transparent:
            for y in range(height * SCALE):
                t = y / (height * SCALE)
                self.d.line((0, y, width * SCALE, y), fill=(int(24-9*t), int(34-11*t), int(42-13*t)))
            for x in range(24, width, 48):
                self.line((x, 0, x, height), '#202b32')
            for y in range(24, height, 48):
                self.line((0, y, width, y), '#202b32')

    def box(self, xy, fill, outline=None, radius=0, width=1):
        coords = tuple(round(v * SCALE) for v in xy)
        self.d.rounded_rectangle(coords, radius=round(radius*SCALE), fill=fill,
                                 outline=outline, width=round(width*SCALE))

    def line(self, xy, fill, width=1):
        self.d.line(tuple(round(v*SCALE) for v in xy), fill=fill, width=round(width*SCALE))

    def text(self, xy, text, size, fill=INK, bold=False, anchor='lt'):
        f = font(size, bold)
        pos = tuple(round(v*SCALE) for v in xy)
        bounds = self.d.textbbox(pos, text, font=f, anchor=anchor)
        assert bounds[0] >= 0 and bounds[1] >= 0 and bounds[2] <= self.w*SCALE and bounds[3] <= self.h*SCALE, (text, bounds)
        self.d.text(pos, text, font=f, fill=fill, anchor=anchor)

    def ellipse(self, xy, fill=None, outline=None, width=1):
        self.d.ellipse(tuple(round(v*SCALE) for v in xy), fill=fill, outline=outline, width=round(width*SCALE))

    def save(self, name):
        path = ROOT / name
        path.parent.mkdir(parents=True, exist_ok=True)
        self.im.resize((self.w, self.h), Image.Resampling.LANCZOS).save(path, optimize=True)
        print(f'{name}: {self.w}x{self.h}, {path.stat().st_size:,} bytes')


def moon(a, cx, cy, r, fill=INK):
    # Cut the second circle out of a mask, not the background: works over any panel.
    layer = Image.new('RGBA', a.im.size)
    mask = Image.new('L', a.im.size)
    d = ImageDraw.Draw(mask)
    def circle(x, y, radius, value):
        d.ellipse(tuple(round(v*SCALE) for v in (x-radius, y-radius, x+radius, y+radius)), fill=value)
    circle(cx, cy, r, 255)
    circle(cx+r*.48, cy-r*.32, r*.90, 0)
    layer.paste(fill, (0, 0, *a.im.size), mask)
    a.im.paste(layer, (0, 0), layer)


def emblem(a, cx, cy, r):
    a.ellipse((cx-r, cy-r, cx+r, cy+r), outline='#6b7d87', width=2)
    for h in range(12):
        t = h*math.pi/6
        inner = r*.85 if h % 3 == 0 else r*.9
        a.line((cx+math.sin(t)*inner, cy-math.cos(t)*inner,
                cx+math.sin(t)*r*.96, cy-math.cos(t)*r*.96), GOLD, 2)
    moon(a, cx-5, cy-4, r*.56)
    a.line((cx+r*.08, cy+r*.12, cx+r*.37, cy+r*.12, cx+r*.37, cy-r*.22), GOLD, 4)


def assignments(name):
    base = ['Sleep' if h <= 5 or h >= 22 else 'Anything' for h in range(24)]
    if name == 'Night Owl':
        return ['Sleep' if 6 <= h <= 13 else 'Anything' for h in range(24)]
    if name == 'Day Worker':
        return ['Work' if 7 <= h <= 16 else 'Recreation' if 17 <= h <= 21 else base[h] for h in range(24)]
    if name == 'Child':
        return ['Sleep' if h <= 6 or h >= 20 else 'Recreation' if 8 <= h <= 11 or 13 <= h <= 16 else 'Anything' for h in range(24)]
    return ['Meditate' if 6 <= h <= 8 else base[h] for h in range(24)]


def library(a, x, y, width, compact=False):
    height = 248 if compact else 318
    a.box((x+5, y+7, x+width+5, y+height+7), '#0b1116', radius=9)
    a.box((x, y, x+width, y+height), '#1c252b', '#596871', radius=7)
    a.text((x+22, y+17), 'MY LIBRARY', 13 if compact else 16, GOLD, True)
    a.text((x+width-22, y+17), '24 HOURS', 12 if compact else 14, MUTED, anchor='rt')
    a.line((x+20, y+45, x+width-20, y+45), '#394750')
    label = 112 if compact else 164
    start = x+label+22
    cell = (width-label-44)/24
    for h in range(24):
        a.text((start+cell*(h+.5), y+57), str(h), 10 if compact else 13, MUTED, anchor='mt')
        a.line((start+cell*(h+.5), y+74, start+cell*(h+.5), y+79), '#596871')
    row_h = 36 if compact else 47
    for i, name in enumerate(('Night Owl', 'Day Worker', 'Child', 'Psycaster')):
        yy = y+87+i*row_h
        a.text((x+22, yy+3), name, 15 if compact else 22)
        for h, assignment in enumerate(assignments(name)):
            a.box((start+h*cell, yy, start+(h+1)*cell-2, yy+(24 if compact else 32)), COLORS[assignment])
        a.line((start, yy+(25 if compact else 33), start+cell*24-2, yy+(25 if compact else 33)), '#10171c')


def preview():
    a = Art(1280, 720)
    a.box((0, 0, 10, 720), GOLD)
    a.text((62, 42), 'RIMWORLD  /  SCHEDULE PRESETS', 17, GOLD, True)
    a.text((58, 90), 'Sleep Schedule', 84, bold=True)
    a.text((64, 204), 'Schedule presets that follow you across saves', 30, '#c7d1d5')
    emblem(a, 1123, 146, 78)
    library(a, 62, 294, 1156)
    xx = 64
    for name in ('Sleep', 'Work', 'Recreation', 'Meditate', 'Anything'):
        a.box((xx, 649, xx+15, 664), COLORS[name])
        a.text((xx+24, 646), name, 17, MUTED)
        xx += {'Sleep': 111, 'Work': 107, 'Recreation': 155, 'Meditate': 141, 'Anything': 140}[name]
    a.text((1218, 648), 'Your hours. Every colony.', 18, GOLD, anchor='rt')
    a.save('About/Preview.png')


def banner():
    a = Art(1280, 320)
    a.box((0, 0, 8, 320), GOLD)
    a.text((36, 30), 'RIMWORLD  /  SCHEDULE PRESETS', 13, GOLD, True)
    a.text((34, 65), 'Sleep Schedule', 51, bold=True)
    a.text((38, 139), 'Your presets. Every colony.', 23, '#c7d1d5')
    emblem(a, 78, 236, 40)
    a.text((137, 211), 'Save your hours.', 18, INK)
    a.text((137, 239), 'Bring them with you.', 18, MUTED)
    library(a, 486, 35, 754, compact=True)
    a.save('Docs/banner.png')


def icon():
    a = Art(256, 256, transparent=True)
    a.box((4, 4, 252, 252), '#182229', '#71818a', radius=43, width=3)
    moon(a, 112, 100, 69)
    # Five broad blocks retain their shape at 32px; no small text in the icon.
    for i, name in enumerate(('Sleep', 'Work', 'Recreation', 'Meditate', 'Anything')):
        a.box((33+i*38, 188, 66+i*38, 219), COLORS[name], radius=2)
    a.line((159, 116, 191, 116, 191, 77), GOLD, 10)
    # ContentFinder resolves XML paths relative to Textures/ without extension.
    a.save('Textures/SleepSchedule/Icon.png')


if __name__ == '__main__':
    preview()
    banner()
    icon()
