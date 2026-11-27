#!/bin/sh
# prototype: tail -n N / -N on a pipe only; anything else -> real tail
# The saved file is kept only when lines were cut; otherwise it is removed before exit.
case "$1" in -n) N=$2; shift 2;; -[0-9]*) N=${1#-}; shift;; *) exec /usr/bin/tail "$@";; esac
[ $# -eq 0 ] && [ ! -t 0 ] || exec /usr/bin/tail -n "$N" "$@"
f=$(mktemp "${TMPDIR:-/tmp}/stream.XXXXXX"); cat > "$f"
/usr/bin/tail -n "$N" "$f"
l=$(awk 'END { print NR }' "$f")
if [ "$l" -gt "$N" ]; then
  echo "tail: showed last $N of $l lines; full output: $f" >&2
else
  rm -f "$f"
fi
exit 0
