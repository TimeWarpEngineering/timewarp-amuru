# Task 157 research files

- `slice-forms-scan.py` counts head and tail forms (Q3), the `2>&1`, capture, and `pipefail` counts (Q1), and how many piped numeric slices had a tool result of at least N lines (Q5). Run: `python3 -I slice-forms-scan.py <claude projects folder> --until 2026-11-27`.
- `rerun-after-cut-scan.py` counts re-runs of the same upstream command after a slice that returned at least N lines, with no edit in between. It prints a loose block and a strict block (Q8, Q9). Run: `python3 -I rerun-after-cut-scan.py <claude projects folder> --until 2026-11-27`. Pass `--rows FILE` to write the matched rows as JSON.
- Both scripts count lines so a trailing newline ends the last line (`str.splitlines`).
- `proto-tail.sh` is the throwaway prototype used to check stdout identity and the `2>&1` cases. It keeps its `stream.*` file only when lines were cut. It is not the product.
