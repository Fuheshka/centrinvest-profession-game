"""Progress screenshots for the defense reports: store them, then turn them into timelapses.

    python3 tools/progress.py add combat "living background v1" screenshots/bg_check_0.png
    python3 tools/progress.py build --feature combat
    python3 tools/progress.py build --all          # every feature: mp4 + storyboard
    python3 tools/progress.py build --master       # one video of every capture ordered by time

`add` saves docs/progress/<feature>/<UTC yyyymmdd-HHMMSS>__<caption>.jpg (max 1600 px wide)
and appends a line to docs/progress/log.jsonl. `build` writes to docs/progress/_out/ (gitignored).
Needs Pillow; `build` also needs ffmpeg on PATH. Based on make_timelapse.py from the World of Oldcraft build kit.
"""
import argparse
import json
import re
import shutil
import subprocess
import sys
import tempfile
from datetime import datetime, timezone
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parent.parent / "docs" / "progress"
IMAGE_TYPES = {".png", ".jpg", ".jpeg", ".webp"}
MAX_WIDTH = 1600


def slug(text: str) -> str:
    # ASCII only: Cyrillic file names break between macOS and Windows clones
    s = re.sub(r"[^a-z0-9-]+", "-", text.strip().lower()).strip("-")[:60]
    if not s:
        sys.exit(f"name must contain latin letters or digits: {text!r}")
    return s


def caption_of(path: Path) -> str:
    stamp, _, caption = path.stem.partition("__")
    if len(stamp) == 15:  # yyyymmdd-HHMMSS
        stamp = f"{stamp[:4]}-{stamp[4:6]}-{stamp[6:8]} {stamp[9:11]}:{stamp[11:13]}"
    return f"{stamp}  {caption.replace('-', ' ')}".strip()


def font(size: int):
    for name in ("Arial.ttf", "arial.ttf", "segoeui.ttf", "DejaVuSans.ttf"):
        try:
            return ImageFont.truetype(name, size)
        except OSError:
            continue
    return ImageFont.load_default()


def git_head() -> str:
    try:
        return subprocess.run(["git", "rev-parse", "--short", "HEAD"], capture_output=True, text=True, check=True).stdout.strip()
    except (OSError, subprocess.CalledProcessError):
        return ""


def add(feature: str, caption: str, src: Path):
    if not src.is_file():
        sys.exit(f"no such file: {src}")
    now = datetime.now(timezone.utc)
    folder = ROOT / slug(feature)
    folder.mkdir(parents=True, exist_ok=True)
    dst = folder / f"{now:%Y%m%d-%H%M%S}__{slug(caption)}.jpg"
    for i in range(2, 100):  # same second and caption: keep both
        if not dst.exists():
            break
        dst = folder / f"{now:%Y%m%d-%H%M%S}__{slug(caption)}-{i}.jpg"
    im = Image.open(src).convert("RGB")
    if im.width > MAX_WIDTH:
        im = im.resize((MAX_WIDTH, round(im.height * MAX_WIDTH / im.width)), Image.LANCZOS)
    im.save(dst, quality=85)
    rel = dst.relative_to(ROOT.parent.parent).as_posix()
    entry = {"time": now.isoformat(timespec="seconds"), "feature": folder.name, "caption": caption, "file": rel, "commit": git_head()}
    with (ROOT / "log.jsonl").open("a", encoding="utf-8") as log:
        log.write(json.dumps(entry, ensure_ascii=False) + "\n")
    print(rel)


def frames_for(folder: Path):
    return sorted(p for p in folder.iterdir() if p.suffix.lower() in IMAGE_TYPES)


def render_frames(images, workdir: Path, width: int, captions: bool, feature_label: bool):
    out = []
    f = font(max(18, width // 60))
    for i, src in enumerate(images):
        im = Image.open(src).convert("RGB")
        h = round(im.height * width / im.width)
        im = im.resize((width, h - h % 2), Image.LANCZOS)
        if captions:
            text = caption_of(src)
            if feature_label:
                text = f"[{src.parent.name}]  {text}"
            d = ImageDraw.Draw(im, "RGBA")
            pad = width // 120
            box = d.textbbox((0, 0), text, font=f)
            d.rectangle((0, im.height - (box[3] - box[1]) - 3 * pad, im.width, im.height), fill=(0, 0, 0, 150))
            d.text((2 * pad, im.height - (box[3] - box[1]) - 2 * pad), text, font=f, fill=(255, 255, 255, 235))
        dst = workdir / f"f{i:06d}.jpg"
        im.save(dst, quality=92)
        out.append(dst)
    return out


def encode(frames, target: Path, fps: float, hold_last: float):
    if not frames:
        return False
    listing = frames[0].parent / "list.txt"
    lines = [f"file '{fr.as_posix()}'\nduration {1.0 / fps:.4f}" for fr in frames]
    lines.append(f"file '{frames[-1].as_posix()}'\nduration {max(hold_last, 1.0 / fps):.4f}")
    lines.append(f"file '{frames[-1].as_posix()}'")
    listing.write_text("\n".join(lines), encoding="utf-8")
    target.parent.mkdir(parents=True, exist_ok=True)
    cmd = ["ffmpeg", "-y", "-loglevel", "error", "-f", "concat", "-safe", "0", "-i", str(listing),
           "-vf", "pad=ceil(iw/2)*2:ceil(ih/2)*2", "-c:v", "libx264", "-pix_fmt", "yuv420p", "-r", "30", str(target)]
    subprocess.run(cmd, check=True)
    return True


def storyboard(images, target: Path, cols: int = 4, thumb_w: int = 480):
    f = font(16)
    thumbs = []
    for src in images:
        im = Image.open(src).convert("RGB")
        h = round(im.height * thumb_w / im.width)
        im = im.resize((thumb_w, h), Image.LANCZOS)
        cell = Image.new("RGB", (thumb_w, h + 44), (18, 18, 22))
        cell.paste(im, (0, 0))
        ImageDraw.Draw(cell).text((8, h + 10), caption_of(src)[:70], font=f, fill=(230, 230, 230))
        thumbs.append(cell)
    cell_h = max(t.height for t in thumbs)
    rows = (len(thumbs) + cols - 1) // cols
    sheet = Image.new("RGB", (cols * thumb_w + (cols + 1) * 8, rows * cell_h + (rows + 1) * 8), (10, 10, 12))
    for i, t in enumerate(thumbs):
        r, c = divmod(i, cols)
        sheet.paste(t, (8 + c * (thumb_w + 8), 8 + r * (cell_h + 8)))
    target.parent.mkdir(parents=True, exist_ok=True)
    sheet.save(target, quality=88)


def build(args):
    if not shutil.which("ffmpeg"):
        sys.exit("ffmpeg is not on PATH")
    out = ROOT / "_out"
    features = [ROOT / slug(n) for n in args.feature or []]
    if args.all or args.master:
        features = [p for p in sorted(ROOT.iterdir()) if p.is_dir() and p.name != "_out"]
    with tempfile.TemporaryDirectory() as tmp:
        tmp = Path(tmp)
        if args.master:
            everything = sorted((img for folder in features for img in frames_for(folder)), key=lambda p: p.name)
            frames = render_frames(everything, tmp, args.width, not args.no_captions, True)
            if encode(frames, out / "master_timelapse.mp4", args.fps, args.hold_last):
                print("wrote", out / "master_timelapse.mp4", f"({len(frames)} frames)")
            return
        for folder in features:
            images = frames_for(folder) if folder.is_dir() else []
            if not images:
                print("no captures in", folder)
                continue
            work = tmp / folder.name
            work.mkdir()
            frames = render_frames(images, work, args.width, not args.no_captions, False)
            encode(frames, out / f"{folder.name}.mp4", args.fps, args.hold_last)
            storyboard(images, out / f"{folder.name}_storyboard.jpg")
            print("wrote", out / f"{folder.name}.mp4", f"({len(images)} frames)")


def main(argv=None):
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    sub = ap.add_subparsers(dest="cmd", required=True)
    a = sub.add_parser("add", help="store one screenshot")
    a.add_argument("feature")
    a.add_argument("caption")
    a.add_argument("image", type=Path)
    b = sub.add_parser("build", help="timelapses and storyboards")
    b.add_argument("--feature", action="append", help="feature folder name (repeatable)")
    b.add_argument("--all", action="store_true", help="every feature folder")
    b.add_argument("--master", action="store_true", help="one video of every capture ordered by time")
    b.add_argument("--fps", type=float, default=3.0)
    b.add_argument("--hold-last", type=float, default=2.0, help="seconds to hold the last frame")
    b.add_argument("--width", type=int, default=1600)
    b.add_argument("--no-captions", action="store_true")
    args = ap.parse_args(argv)
    if args.cmd == "add":
        add(args.feature, args.caption, args.image)
    else:
        build(args)


if __name__ == "__main__":
    main()
