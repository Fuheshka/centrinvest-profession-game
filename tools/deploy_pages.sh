#!/usr/bin/env bash
# Publish Builds/Web to the gh-pages branch (GitHub Pages source). The build is gitignored, so it is pushed as an orphan commit.
# Usage: tools/deploy_pages.sh   (run after Tools > Build Web Release; asks before pushing)
set -euo pipefail
root="$(git rev-parse --show-toplevel)"
src="$root/Builds/Web"
[ -f "$src/index.html" ] || { echo "No build in $src. Run Tools > Build Web Release first."; exit 1; }
remote="$(git -C "$root" remote get-url origin)"
tmp="$(mktemp -d)"
trap 'rm -rf "$tmp"' EXIT
cp -R "$src/." "$tmp/"
touch "$tmp/.nojekyll"
rm -f "$tmp/.DS_Store"
cd "$tmp"
git init -q -b gh-pages
git add -A
git -c user.name="$(git -C "$root" config user.name)" -c user.email="$(git -C "$root" config user.email)" commit -q -m "deploy: web build $(date +%F)"
read -r -p "Force-push $(du -sh . | cut -f1) to gh-pages of $remote ? [y/N] " ok
[ "$ok" = "y" ] || { echo "Cancelled."; exit 0; }
git push -f "$remote" gh-pages
echo "Done. Settings > Pages > Source: gh-pages / root."
