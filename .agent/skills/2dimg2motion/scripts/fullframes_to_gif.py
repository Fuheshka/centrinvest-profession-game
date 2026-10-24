#!/usr/bin/env python3
"""Сборка набора прозрачных PNG-кадров в анимированный GIF и спрайтшит."""

from __future__ import annotations

import argparse
import re
from pathlib import Path
from PIL import Image, ImageColor


def natural_key(path: Path) -> list[int | str]:
    parts = re.split(r"(\d+)", path.name)
    return [int(p) if p.isdigit() else p.lower() for p in parts]


def assemble_gif(
    frames_dir: Path,
    output_gif: Path,
    *,
    pattern: str = "*.png",
    duration_ms: int = 80,
    background_hex: str = "#FFFFFF",
) -> Path:
    bg_rgb = ImageColor.getrgb(background_hex)[:3]
    files = sorted(frames_dir.glob(pattern), key=natural_key)
    if not files:
        raise FileNotFoundError(f"В папке {frames_dir} не найдены файлы по шаблону {pattern}")

    composite_frames: list[Image.Image] = []
    base_size = Image.open(files[0]).size

    for f in files:
        img = Image.open(f).convert("RGBA")
        if img.size != base_size:
            img = img.resize(base_size, Image.Resampling.LANCZOS)
        bg = Image.new("RGBA", base_size, (*bg_rgb, 255))
        bg.alpha_composite(img)
        composite_frames.append(bg.convert("RGB"))

    output_gif.parent.mkdir(parents=True, exist_ok=True)
    composite_frames[0].save(
        output_gif,
        save_all=True,
        append_images=composite_frames[1:],
        duration=duration_ms,
        loop=0,
        optimize=True,
    )
    return output_gif


def assemble_spritesheet(
    frames_dir: Path,
    output_sheet: Path,
    *,
    pattern: str = "*.png",
    columns: int = 7,
) -> Path:
    files = sorted(frames_dir.glob(pattern), key=natural_key)
    if not files:
        raise FileNotFoundError(f"В папке {frames_dir} не найдены кадры для спрайтшита")

    first = Image.open(files[0]).convert("RGBA")
    cell_w, cell_h = first.size
    total_frames = len(files)
    cols = min(columns, total_frames)
    rows = (total_frames + cols - 1) // cols

    sheet = Image.new("RGBA", (cols * cell_w, rows * cell_h), (0, 0, 0, 0))
    for i, path in enumerate(files):
        row = i // cols
        col = i % cols
        frame = Image.open(path).convert("RGBA")
        sheet.paste(frame, (col * cell_w, row * cell_h), frame)

    output_sheet.parent.mkdir(parents=True, exist_ok=True)
    sheet.save(output_sheet, "PNG")
    return output_sheet


def main() -> None:
    parser = argparse.ArgumentParser(description="Сборка PNG кадров в GIF и спрайтшит")
    parser.add_argument("frames_dir", type=Path, help="Папка с PNG кадрами")
    parser.add_argument("-o", "--output-gif", type=Path, default=None, help="Путь к результирующему GIF")
    parser.add_argument("-s", "--output-sheet", type=Path, default=None, help="Путь к результирующему спрайтшиту")
    parser.add_argument("--fps", type=int, default=12, help="Частота кадров (по умолчанию 12 FPS)")
    parser.add_argument("--bg", type=str, default="#FFFFFF", help="Цвет фона для превью GIF")
    args = parser.parse_args()

    dur = round(1000 / args.fps)
    out_gif = args.output_gif or (args.frames_dir / "preview.gif")
    assemble_gif(args.frames_dir, out_gif, duration_ms=dur, background_hex=args.bg)
    print(f"GIF создан: {out_gif}")

    out_sheet = args.output_sheet or (args.frames_dir / "spritesheet.png")
    assemble_spritesheet(args.frames_dir, out_sheet)
    print(f"Спрайтшит создан: {out_sheet}")


if __name__ == "__main__":
    main()
