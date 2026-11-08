#!/usr/bin/env python3
"""JSON-RPC child used by the Amuru client tests.

Speaks a JSON-RPC request/response stream on stdin/stdout. initialize and tools/list
follow the MCP stdio shapes. Standard error is a separate line and is never written to stdout.
"""

import json
import os
import signal
import sys
import time


def write(payload):
    sys.stdout.write(json.dumps(payload) + "\n")
    sys.stdout.flush()


def respond(request_id, result):
    write({"jsonrpc": "2.0", "id": request_id, "result": result})


def fail(request_id, code, message):
    write(
        {
            "jsonrpc": "2.0",
            "id": request_id,
            "error": {"code": code, "message": message, "data": {"ok": False}},
        }
    )


for raw in sys.stdin:
    line = raw.strip()
    if not line:
        continue
    try:
        message = json.loads(line)
    except json.JSONDecodeError:
        write({"jsonrpc": "2.0", "id": None, "error": {"code": -32700, "message": "Parse error"}})
        continue

    method = message.get("method")
    request_id = message.get("id")
    params = message.get("params") or {}

    if method == "initialize":
        sys.stderr.write("stderr:initialize\n")
        sys.stderr.flush()
        respond(
            request_id,
            {
                "protocolVersion": "2024-11-05",
                "capabilities": {"tools": {}},
                "serverInfo": {"name": "amuru-json-rpc-fixture", "version": "1"},
            },
        )
        continue

    if method == "tools/list":
        respond(request_id, {"tools": [{"name": "add"}]})
        continue

    if method == "add":
        sys.stderr.write("stderr:add\n")
        sys.stderr.flush()
        write({"jsonrpc": "2.0", "method": "notifications/message", "params": {"level": "info"}})
        respond(request_id, {"sum": int(params.get("left", 0)) + int(params.get("right", 0))})
        continue

    if method == "fail":
        fail(request_id, -32000, "nope")
        continue

    if method == "sleep":
        time.sleep(30)
        respond(request_id, {"ok": True})
        continue

    if method == "exit":
        respond(request_id, {"ok": True})
        sys.stdout.flush()
        os._exit(3)

    if method == "crash":
        os.kill(os.getpid(), signal.SIGKILL)

    fail(request_id, -32601, "Method not found")
