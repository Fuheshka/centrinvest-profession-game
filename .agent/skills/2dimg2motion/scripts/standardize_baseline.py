#!/usr/bin/env python3
"""Стандартизация исходного изображения персонажа для покадровой анимации.

Обрезает прозрачные или белые поля, масштабирует наибольшую сторону до 300-400px
и помещает в центр квадратного прозрачного холста с запасом полей под размах удара.
"""

from __future__ import annotations

import argparse
from pathlib import Path
from PIL import Image


def _is_near_white(pixel: tuple[int, int, int, int], threshold: int) -> bool:
    r, g, b, a = pixel
    return a > 0 and r >= threshold and g >= threshold and b >= threshold


def foreground_bbox(image: Image.Image, *, white_threshold: int = 245) -> tuple[int, int, int, int] | None:
    rgba = image.convert("RGBA")
    alpha_bbox = rgba.getchannel("A").getbbox()
    if alpha_bbox is None:
        return None

    pixels = rgba.load()
    width, height = rgba.size
    min_x, min_y = width, height
    max_x, max_y = -1, -1
    for y in range(height):
        for x in range(width):
            pixel = pixels[x, y]
            if pixel[3] == 0 or _is_near_white(pixel, white_threshold):
                continue
            min_x = min(min_x, x)
            min_y = min(min_y, y)
            max_x = max(max_x, x)
            max_y = max(max_y, y)
    if max_x < min_x or max_y < min_y:
        return alpha_bbox
    return (min_x, min_y, max_x + 1, max_y + 1)


def standardize(
    input_path: Path,
    output_path: Path,
    *,
    subject_max: int = 360,
    margin_ratio: float = 0.75,
    white_threshold: int = 245,
) -> Path:
    source = Image.open(input_path).convert("RGBA")
    bbox = foreground_bbox(source, white_threshold=white_threshold)
    if bbox is None:
        raise ValueError(f"Не найдены пиксели переднего плана в {input_path}")

    subject = source.crop(bbox)
    src_w, src_h = subject.size
    scale = subject_max / max(src_w, src_h)
    new_size = (max(1, round(src_w * scale)), max(1, round(src_h * scale)))
    resized = subject.resize(new_size, Image.Resampling.LANCZOS)

    margin = max(32, round(subject_max * margin_ratio))
    canvas_dim = max(new_size[0], new_size[1]) + margin * 2
    canvas = Image.new("RGBA", (canvas_dim, canvas_dim), (0, 0, 0, 0))

    # Центрируем персонажа по X, фиксируем нижний уровень по Y с небольшим смещением вниз
    offset_x = (canvas_dim - new_size[0]) // 2
    offset_y = canvas_dim - margin - new_size[1]
    canvas.paste(resized, (offset_x, offset_y), resized)

    output_path.parent.mkdir(parents=True, exist_ok=True)
    canvas.save(output_path, "PNG")
    return output_path


def main() -> None:
    parser = argparse.ArgumentParser(description="Стандартизация исходного изображения для 2D-анимации")
    parser.add_argument("input", type=Path, help="Путь к исходному изображению")
    parser.add_argument("-o", "--output", type=Path, default=None, help="Путь к выходному файлу")
    parser.add_argument("--size", type=int, default=360, help="Размер большей стороны персонажа (px)")
    parser.add_argument("--margin", type=float, default=0.75, help="Коэффициент полей безопасности (0.5 - 1.0)")
    args = parser.parse_args()

    out_file = args.output or args.input.with_name(f"{args.input.stem}_std.png")
    result = standardize(args.input, out_file, subject_max=args.size, margin_ratio=args.margin)
    print(f"Готово: {result}")


if __name__ == "__main__":
    main()
