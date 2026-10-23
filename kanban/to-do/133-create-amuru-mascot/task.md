# Create Amuru mascot

## Goal (implementer restatement)

Put the finished Amuru art into this repo and point the readme at it. Amuru is this repo's original commander hero (the name means command in Swahili). The chest letter is A. The character is not one of the SOLID heroes.

Acceptance:

- `assets/mascot/amuru-hero.png` is the kit's 1024x1024 hero on white, copied byte for byte.
- `assets/mascot/amuru-hero-dark.png` is the kit's dark hero on Night #06161E, copied byte for byte.
- `assets/mascot/amuru-hero-transparent.png` is the kit's transparent cut-out, copied byte for byte.
- `assets/mascot/amuru-og-card.png` is the kit's 1200x630 card (words already drawn in code), copied byte for byte.
- `assets/mascot/amuru-end-card.png` is the kit's 1920x1080 video end card, copied byte for byte.
- `readme.md` keeps every existing line and gains a centered hero image at the top.
- Checklist items 1 through 5 are ticked. Steven's approval stays open.
- No product code changes, and no new claims about the character beyond this task.

## Description

Create **Amuru**, the original TimeWarp mascot hero for this repo.  Amuru means *command* in Swahili, and the hero is themed on that meaning.

Brand guide: https://github.com/TimeWarpEngineering/timewarp-intel/pull/51 (`brand/timewarp-brand-style.md`, section 0, "Repo heroes").

Proposed look (from the roster; a proposal, adjust with Steven): Lilac and Deep Violet commander/conductor with a baton that fires commands, sharp salute.  Chest letter: **A**.



## Requirements

- Style: in the spirit of Steven's hand-drawn SOLID heroes: cartoon, thick #06161E outline, flat colors, blue eye mask, one big chest letter, cheeky helpful pose.  Use `brand/source/x-header.jpg` as the style reference.
- Never reuse the SOLID heroes, their bodies or their moulds.  They are for the SOLID principles only.  Amuru is an original character.
- Costume colors from the logo palette (Cyan #00CCFF, Ocean #004F78, Violet #6953B1, Deep Violet #352078, Lilac #C5B4FF, Leaf #6FA749, Ribbon #50852C, Mint #D2FFB4, Hero Red #D92C31).
- Only the single chest letter **A** in the art.  Image models garble text; every other word is added with code (Pillow) afterwards.
- No real data or charts in the illustration.

## Checklist

- [x] Hero art on white (PNG, transparent or white background, at least 1024 px)
- [x] Dark variant on Night #06161E
- [x] Use in the README header
- [x] Use in the OG card (1200x630, code-drawn text)
- [x] Use in the video end card
- [ ] Steven approves the final art

## Notes

- Filed by Masoko (marketing) on 2026-10-22 at Steven's request.

## Implementation notes (Masoko, 2026-10-23)

The final art and derived cards are already built.  Do not regenerate or redraw anything.  Copy and wire them in:

- Source kit (outside the repo): `/home/steve/masoko/mascot-kit/`, files `amuru-hero.png`, `amuru-hero-dark.png`, `amuru-hero-transparent.png`, `amuru-og-card.png`, `amuru-end-card.png`.
- Copy all five into `assets/mascot/` in this repo (create the folder).  Keep the file names.  Binary files, copy byte for byte.
  - `amuru-hero.png`: 1024x1024 hero on white (checklist item 1).
  - `amuru-hero-dark.png`: hero with white sticker outline on Night #06161E (item 2).
  - `amuru-hero-transparent.png`: transparent cut-out, extra.
  - `amuru-og-card.png`: 1200x630 OG card, text drawn by code (item 4).
  - `amuru-end-card.png`: 1920x1080 still end card for videos (item 5).
- README header (item 3): add the hero at the very top of `readme.md`, centered, for example `<p align="center"><img src="assets/mascot/amuru-hero.png" alt="Amuru, the TimeWarp Amuru mascot" width="240" /></p>`.  Keep every existing README line.  Use plain ASCII, no emoji.
- Tick checklist items 1 to 5.  Leave "Steven approves the final art" unticked: that is Steven's call.
- Chest letter is A.  Do not add prose claims beyond what this task states (Swahili meaning is in the Description).
- Changes stay to `assets/mascot/`, `readme.md` and this task file.  No code changes.

## Session

- Implementation: grok `01a167a9-8796-74f1-a47b-c6aa8a1348ac` (2026-10-23) task worktree
- Art copied from `/home/steve/masoko/mascot-kit/` (Masoko, 2026-10-23). Not redrawn.
- `ganda repo audit` was red on `nuru` and `workflow-file`. `ganda repo audit --fix` pinned Nuru 4.0.0-beta.5, added `feature/**` to pull_request branches, and confirmed dev-cli self-install. The review-round and `mission.heartbeat` gitignore lines were already in the worktree and are part of this commit so `required-gitignore` stays green.

## Results

The finished Amuru kit is in `assets/mascot/`, copied byte for byte. `readme.md` leads with the white hero and keeps every previous line. The chest letter on the hero is A. Checklist items 1 to 5 are ticked. Steven's approval stays open.

| File | Size | Color | sha256 |
| --- | --- | --- | --- |
| `assets/mascot/amuru-hero.png` | 1024x1024 | RGB, corners #FFFFFF | `2f59f42f39ce7f63b54e05f6b8a98572495dd06b17a2d873df242cee7cc1d52c` |
| `assets/mascot/amuru-hero-dark.png` | 1024x1024 | RGB, corners #06161E | `62aee7d754df945276b67ae65138f7dada5c6242d3dba360df4a1d5df7cf3993` |
| `assets/mascot/amuru-hero-transparent.png` | 1024x1024 | RGBA, corner alpha 0 | `c29e6d09a579c75b1c5fee27f8d76de4757c829cbd473beb1360c1c880267462` |
| `assets/mascot/amuru-og-card.png` | 1200x630 | RGB | `f720978f1fba6d411fd8b028ff5715a123f5349e3628db43c4f5c8926f14bd6f` |
| `assets/mascot/amuru-end-card.png` | 1920x1080 | RGB, field #06161E | `bed42c7007def827af17ccc94a641078da0a0494b2d8e5d2d46d9dc140ae7c9c` |

The OG card and the end card are the kit files. Their words were already drawn in code. The readme does not add a caption.

`ganda repo audit --fix` also changed:

- `Directory.Packages.props`: `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` from `3.0.0-beta.80` to `4.0.0-beta.5`. Dev-cli self-install compiled, and the `nuru` check passed.
- `.github/workflows/workflow.yml`: `pull_request.branches` includes `feature/**`.
- `.gitignore`: `**/review/round-*/base.sha`, `**/review/round-*/closed`, and `mission.heartbeat`.

Audit after that fix: Passed 32, Failed 0, Skipped 1. No library source changed, so the product test suite was not re-run.

### How to validate

**Smoke**

```bash
python3 - <<'PY'
import hashlib, struct, zlib
from pathlib import Path

def png(path):
    data = Path(path).read_bytes()
    assert data[:8] == b"\x89PNG\r\n\x1a\n", path
    pos = 8
    w = h = color = None
    idat = b""
    while pos < len(data):
        length = struct.unpack(">I", data[pos:pos+4])[0]
        ctype = data[pos+4:pos+8]
        chunk = data[pos+8:pos+8+length]
        pos += 12 + length
        if ctype == b"IHDR":
            w, h, bit, color = struct.unpack(">IIBB", chunk[:10])
            assert bit == 8
        elif ctype == b"IDAT":
            idat += chunk
        elif ctype == b"IEND":
            break
    raw = zlib.decompress(idat)
    channels = {2: 3, 6: 4}[color]
    stride = w * channels
    rows = []
    i = 0
    prev = bytearray(stride)
    for _y in range(h):
        filt = raw[i]
        i += 1
        row = bytearray(raw[i:i+stride])
        i += stride
        if filt == 1:
            for x in range(stride):
                left = row[x-channels] if x >= channels else 0
                row[x] = (row[x] + left) & 255
        elif filt == 2:
            for x in range(stride):
                row[x] = (row[x] + prev[x]) & 255
        elif filt == 3:
            for x in range(stride):
                left = row[x-channels] if x >= channels else 0
                row[x] = (row[x] + ((left + prev[x]) // 2)) & 255
        elif filt == 4:
            def paeth(a, b, c):
                p = a + b - c
                pa, pb, pc = abs(p-a), abs(p-b), abs(p-c)
                if pa <= pb and pa <= pc: return a
                if pb <= pc: return b
                return c
            for x in range(stride):
                a = row[x-channels] if x >= channels else 0
                b = prev[x]
                c = prev[x-channels] if x >= channels else 0
                row[x] = (row[x] + paeth(a, b, c)) & 255
        elif filt != 0:
            raise SystemExit(f"bad filter {filt} in {path}")
        rows.append(row)
        prev = row
    def pix(x, y):
        return tuple(rows[y][x*channels:(x+1)*channels])
    return w, h, color, pix, hashlib.sha256(data).hexdigest()

expect = {
    "assets/mascot/amuru-hero.png": (1024, 1024, 2, (255, 255, 255), "2f59f42f39ce7f63b54e05f6b8a98572495dd06b17a2d873df242cee7cc1d52c"),
    "assets/mascot/amuru-hero-dark.png": (1024, 1024, 2, (6, 22, 30), "62aee7d754df945276b67ae65138f7dada5c6242d3dba360df4a1d5df7cf3993"),
    "assets/mascot/amuru-hero-transparent.png": (1024, 1024, 6, (255, 255, 255, 0), "c29e6d09a579c75b1c5fee27f8d76de4757c829cbd473beb1360c1c880267462"),
    "assets/mascot/amuru-og-card.png": (1200, 630, 2, None, "f720978f1fba6d411fd8b028ff5715a123f5349e3628db43c4f5c8926f14bd6f"),
    "assets/mascot/amuru-end-card.png": (1920, 1080, 2, (6, 22, 30), "bed42c7007def827af17ccc94a641078da0a0494b2d8e5d2d46d9dc140ae7c9c"),
}
for path, (ew, eh, ecolor, corner, digest) in expect.items():
    w, h, color, pix, got = png(path)
    assert (w, h, color) == (ew, eh, ecolor), (path, w, h, color)
    assert got == digest, (path, got)
    if corner is not None:
        assert pix(0, 0) == corner and pix(w-1, 0) == corner, (path, pix(0, 0))
readme = Path("readme.md").read_text().splitlines()
assert readme[0] == '<p align="center"><img src="assets/mascot/amuru-hero.png" alt="Amuru, the TimeWarp Amuru mascot" width="240" /></p>'
assert readme[1] == ""
assert readme[2].startswith("[![Stars]")
print("ok")
PY
```

**Expect**

- The script prints `ok`.
- Hero corners are white `#FFFFFF`. Dark-hero corners and the end-card field are Night `#06161E` (6, 22, 30). The transparent cut-out has alpha 0 at the corners.
- `readme.md` line 1 is the centered hero. Line 3 is the original Stars badge. No other readme line moved.
- Checklist items 1 to 5 in this task file are ticked. "Steven approves the final art" is still open.
- `ganda repo audit` exits 0 (`nuru` and `workflow-file` pass).
