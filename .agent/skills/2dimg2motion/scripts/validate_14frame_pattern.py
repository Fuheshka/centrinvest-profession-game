#!/usr/bin/env python3
"""Валидатор контракта 14-кадровой анимации."""

from __future__ import annotations

import argparse
from pathlib import Path
from PIL import Image, ImageChops

FULL_INDICES = tuple(range(14))
KEY_INDICES = (2, 5, 8, 11)


def _pixel_equal(img_a: Image.Image, img_b: Image.Image) -> bool:
    return img_a.size == img_b.size and ImageChops.difference(img_a, img_b).getbbox() is None


def validate(frames_dir: Path, prefix: str = "frame") -> list[str]:
    errors: list[str] = []
    expected_files = [frames_dir / f"{prefix}_{idx:02d}.png" for idx in FULL_INDICES]

    for f in expected_files:
        if not f.is_file():
            errors.append(f"Отсутствует обязательный кадр: {f.name}")

    if errors:
        return errors

    # Проверка формата RGBA
    images: list[Image.Image] = []
    for f in expected_files:
        img = Image.open(f)
        if img.mode != "RGBA":
            errors.append(f"Кадр {f.name} имеет режим {img.mode}, ожидается RGBA")
        images.append(img)

    if errors:
        return errors

    # Проверка стабильности размера
    base_dim = images[0].size
    for idx, img in enumerate(images):
        if img.size != base_dim:
            errors.append(f"Размер кадра {idx:02d} ({img.size}) не совпадает с кадром 00 ({base_dim})")

    # Проверка идеального цикла (кадр 00 == кадр 13)
    if not _pixel_equal(images[0], images[13]):
        errors.append("Кадр 13 обязан быть точной попиксельной копией кадра 00 для бесшовного зацикливания")

    # Проверка прозрачности углов
    for idx, img in enumerate(images):
        w, h = img.size
        corners = [(0, 0), (w - 1, 0), (0, h - 1), (w - 1, h - 1)]
        for cx, cy in corners:
            alpha = img.getpixel((cx, cy))[3]
            if alpha > 10:
                errors.append(f"Кадр {idx:02d}: угол ({cx},{cy}) не прозрачен (alpha={alpha})")
                break

    return errors


def main() -> None:
    parser = argparse.ArgumentParser(description="Валидатор 14-кадрового пакета анимации")
    parser.add_argument("frames_dir", type=Path, help="Папка с кадрами")
    parser.add_argument("--prefix", type=str, default="frame", help="Префикс имен файлов (по умолчанию frame)")
    args = parser.parse_args()

    errs = validate(args.frames_dir, args.prefix)
    if errs:
        print("Ошибки валидации:")
        for e in errs:
            print(f"  - {e}")
        raise SystemExit(1)
    print("Валидация успешно пройдена: 14 кадров строго соответствуют контракту.")


if __name__ == "__main__":
    main()
