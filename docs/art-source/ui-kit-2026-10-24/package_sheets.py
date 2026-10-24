"""Slice generated sheets and pack named sprites; preserve original artwork and alpha."""
from pathlib import Path
import json
from PIL import Image

SOURCE = Path(__file__).resolve().parent
ROOT = SOURCE.parents[2]
DEST = ROOT / 'Assets/_Project/Art/UI/GeneratedKit20261024'
SIZES = {
    'Icons': (256, 256), 'Buttons': (512, 192), 'Avatars': (512, 512),
    'Panels': (512, 384), 'Cards': (512, 384), 'Controls': (256, 160),
    'Decor': (256, 256), 'Backgrounds': (1024, 768),
}


def grid_cuts(image, count, vertical):
    """Find transparent gutters near requested grid lines before deterministic repacking."""
    mask = image.getchannel('A').point(lambda value: 255 if value >= 16 else 0)
    length = image.width if vertical else image.height
    cuts = [0]
    for index in range(1, count):
        ideal = index * length / count
        radius = length / count * 0.28
        start, stop = max(cuts[-1] + 1, int(ideal - radius)), min(length, int(ideal + radius))
        runs, run_start = [], None
        for pos in range(start, stop):
            stripe = (pos, 0, pos + 1, image.height) if vertical else (0, pos, image.width, pos + 1)
            empty = mask.crop(stripe).getbbox() is None
            if empty and run_start is None:
                run_start = pos
            if not empty and run_start is not None:
                runs.append((run_start, pos))
                run_start = None
        if run_start is not None:
            runs.append((run_start, stop))
        # A broad clear gutter is more reliable than an isolated antialiasing hole.
        runs = [run for run in runs if run[1] - run[0] >= 3]
        if runs:
            chosen = min(runs, key=lambda run: abs((run[0] + run[1]) / 2 - ideal))
            cuts.append((chosen[0] + chosen[1]) // 2)
        else:
            cuts.append(round(ideal))
    cuts.append(length)
    return cuts


def package():
    plan = json.loads((SOURCE / 'plan.json').read_text())
    extra = SOURCE / 'extra-plan.json'
    if extra.exists():
        plan['sheets'].append(json.loads(extra.read_text()))
    manifest = {'coordinate_system': 'Unity bottom-left pixels', 'sheets': [], 'warnings': []}
    for spec in plan['sheets']:
        source = SOURCE / (spec['key'] + '.png')
        corrected = SOURCE / (spec['key'] + '-v2.png')
        if corrected.exists():
            source = corrected
        if not source.exists():
            manifest['warnings'].append(f'Missing sheet: {source.name}')
            continue
        image = Image.open(source).convert('RGBA')
        w, h = image.size
        cols, rows = spec['cols'], spec['rows']
        cw, ch = SIZES[spec['category']]
        opaque = spec.get('opaque', False)
        xs = [round(i * w / cols) for i in range(cols + 1)] if opaque else grid_cuts(image, cols, True)
        ys = [round(i * h / rows) for i in range(rows + 1)] if opaque else grid_cuts(image, rows, False)
        ys_by_col = [ys if opaque else grid_cuts(image.crop((xs[col], 0, xs[col + 1], h)), rows, False)
                     for col in range(cols)]
        alpha_range = image.getchannel('A').getextrema()
        if not opaque and alpha_range[0] == 255:
            manifest['warnings'].append(f'{spec["key"]}: no alpha transparency')
        atlas = Image.new('RGBA', (cols * cw, rows * ch))
        folder = DEST / spec['category']
        singles = folder / spec['key']
        singles.mkdir(parents=True, exist_ok=True)
        entries = []
        for index, name in enumerate(spec['names']):
            row, col = divmod(index, cols)
            col_ys = ys_by_col[col]
            rect = (xs[col], col_ys[row], xs[col + 1], col_ys[row + 1])
            sprite = image.crop(rect)
            alpha = sprite.getchannel('A')
            bounds = alpha.point(lambda value: 255 if value >= 16 else 0).getbbox()
            if bounds is None:
                manifest['warnings'].append(f'{spec["key"]}/{name}: empty cell')
                bounds = (0, 0, sprite.width, sprite.height)
            if not opaque:
                left, top, right, bottom = bounds
                if left == 0 or top == 0 or right == sprite.width or bottom == sprite.height:
                    manifest['warnings'].append(f'{spec["key"]}/{name}: artwork touches source cell edge; inspect')
                # Preserve a small alpha fringe without including invisible PNG speckles.
                bounds = (max(0, bounds[0] - 2), max(0, bounds[1] - 2),
                          min(sprite.width, bounds[2] + 2), min(sprite.height, bounds[3] + 2))
                sprite = sprite.crop(bounds)
                scale = min(1.0, (cw - 32) / sprite.width, (ch - 32) / sprite.height)
                size = (max(1, round(sprite.width * scale)), max(1, round(sprite.height * scale)))
                if size != sprite.size:
                    sprite = sprite.resize(size, Image.Resampling.LANCZOS)
                tile = Image.new('RGBA', (cw, ch))
                tile.alpha_composite(sprite, ((cw - sprite.width) // 2, (ch - sprite.height) // 2))
            else:
                tile = sprite.resize((cw, ch), Image.Resampling.LANCZOS)
            tile.save(singles / (name + '.png'))
            atlas.alpha_composite(tile, (col * cw, row * ch))
            bbox = tile.getchannel('A').getbbox()
            border = [0, 0, 0, 0]
            if spec['category'] in ('Buttons', 'Panels', 'Cards'):
                # Suggested 9-slice border; review in Sprite Editor at the final UI size.
                border = [min(cw // 3, (bbox[0] if bbox else 0) + 24),
                          min(ch // 3, (ch - bbox[3] if bbox else 0) + 24),
                          min(cw // 3, (cw - bbox[2] if bbox else 0) + 24),
                          min(ch // 3, (bbox[1] if bbox else 0) + 24)]
            entries.append({'name': name, 'row': row, 'column': col,
                            'rect': {'x': col * cw, 'y': (rows - row - 1) * ch, 'width': cw, 'height': ch},
                            'pivot': [0.5, 0.5], 'border_suggested_lbrt': border,
                            'source_cell_top_left': list(rect), 'source_content_top_left': list(bounds),
                            'file': (singles / (name + '.png')).relative_to(ROOT).as_posix()})
        atlas_path = folder / (spec['key'] + '-sheet.png')
        atlas.save(atlas_path)
        manifest['sheets'].append({'name': spec['key'], 'category': spec['category'],
                                  'source': source.relative_to(ROOT).as_posix(),
                                  'file': atlas_path.relative_to(ROOT).as_posix(),
                                  'source_size': [w, h], 'size': list(atlas.size),
                                  'grid': [cols, rows], 'cell_size': [cw, ch],
                                  'transparent': not opaque, 'source_alpha_range': list(alpha_range),
                                  'sprites': entries})
    DEST.mkdir(parents=True, exist_ok=True)
    (DEST / 'manifest.json').write_text(json.dumps(manifest, indent=2) + '\n')
    print(json.dumps({'sheets': len(manifest['sheets']),
                      'sprites': sum(len(s['sprites']) for s in manifest['sheets']),
                      'warnings': manifest['warnings']}, indent=2))


if __name__ == '__main__':
    package()
