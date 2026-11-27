#!/bin/sh
# prototype: tail -n N / -N on a pipe only; anything else -> real tail
case "$1" in -n) N=$2; shift 2;; -[0-9]*) N=${1#-}; shift;; *) exec /usr/bin/tail "$@";; esac
[ $# -eq 0 ] && [ ! -t 0 ] || exec /usr/bin/tail -n "$N" "$@"
f=$(mktemp ${TMPDIR:-/tmp}/stream.XXXXXX); cat > "$f"
/usr/bin/tail -n "$N" "$f"
l=$(wc -l < "$f"); [ "$l" -gt "$N" ] && echo "tail: showed last $N of $l lines; full output: $f" >&2
exit 0
