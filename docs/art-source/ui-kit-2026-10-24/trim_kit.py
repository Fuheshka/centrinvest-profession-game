#!/usr/bin/env python3
"""Crop the individual kit PNGs to their opaque bounds (2 px margin).

The packager centres every sprite in a big cell, so buttons and panels carried about 40 percent of transparent
padding: 9-slice borders from manifest.json then cut into the padding and rects looked much smaller than their size.
Run once after package_sheets.py; Backgrounds are left alone. Sheets (*-sheet.png) are not touched.
"""
import glob, sys
from PIL import Image

ROOT = "Assets/_Project/Art/UI/GeneratedKit20261024"
MARGIN = 2
for path in sorted(glob.glob(ROOT + "/*/*/*.png")):
    if "/Backgrounds/" in path:
        continue
    im = Image.open(path).convert("RGBA")
    box = im.split()[3].point(lambda v: 255 if v > 20 else 0).getbbox()
    if not box:
        print("empty", path); continue
    l, t, r, b = box
    box = (max(l - MARGIN, 0), max(t - MARGIN, 0), min(r + MARGIN, im.width), min(b + MARGIN, im.height))
    if box == (0, 0, im.width, im.height):
        continue
    im.crop(box).save(path)
    print(path.split(ROOT + "/")[1], im.size, "->", (box[2] - box[0], box[3] - box[1]))
