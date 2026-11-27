# Task 157 research files

- `slice-forms-scan.py` counts head and tail forms in Claude transcripts. Run: `python3 -I slice-forms-scan.py <claude projects folder>`.
- `rerun-after-cut-scan.py` counts re-runs of the same upstream command after a slice that returned at least N lines, with no edit in between. It writes `rows.json` in the current folder.
- `proto-tail.sh` is the throwaway prototype used to check stdout identity and the `2>&1` cases. It is not the product.
