#!/usr/bin/env bash
# Relay smoke test: host creates a Unity Multiplayer Services (Relay) session on
# an APFS clone of the project; the client joins by the published join code.
# Requires the project to be linked to a Unity Cloud project (see
# Docs/RUNTIME_SETUP.md). Unity forbids two instances on the SAME project path.
# Usage: Tools/relay-smoke.sh [repo]
set -uo pipefail

REPO="${1:-$(pwd)}"
UNITY="${UNITY:-/Applications/Unity/Hub/Editor/6000.3.26f1/Unity.app/Contents/MacOS/Unity}"
CODE_FILE="${PM_RELAY_CODE:-/tmp/pm-relay-code}"
WORK="$(mktemp -d)"
HOST_DIR="$WORK/host"

rm -f "$CODE_FILE"
export PM_RELAY=1
export PM_SESSION_BACKEND=ugs
export PM_RELAY_CODE="$CODE_FILE"

echo "=== cloning project for host instance ==="
mkdir -p "$HOST_DIR"
cp -cR "$REPO/Assets" "$REPO/Packages" "$REPO/ProjectSettings" "$REPO/Library" \
  "$REPO/UserSettings" "$HOST_DIR/"
echo "clone ready: $HOST_DIR"

cleanup() { rm -rf "$WORK"; }
trap cleanup EXIT

echo "=== starting relay host process (clone) ==="
"$UNITY" -batchmode -nographics -projectPath "$HOST_DIR" -runTests -testPlatform PlayMode \
  -testFilter "PanicMechanics.Tests.PlayMode.RelayHostSmokeTests" \
  -testResults "$WORK/host.xml" -logFile "$WORK/host.log" &
HOST_PID=$!

echo "=== waiting for relay host join code ==="
READY=0
for _ in $(seq 1 300); do
  if [ -f "$CODE_FILE" ]; then READY=1; break; fi
  if ! kill -0 "$HOST_PID" 2>/dev/null; then break; fi
  sleep 1
done

if [ "$READY" -ne 1 ]; then
  echo "RELAY_HOST_NOT_READY"
  tail -40 "$WORK/host.log" 2>/dev/null || true
  wait "$HOST_PID" 2>/dev/null || true
  exit 1
fi

echo "=== relay host ready (code $(cat "$CODE_FILE")); starting client process (real repo) ==="
"$UNITY" -batchmode -nographics -projectPath "$REPO" -runTests -testPlatform PlayMode \
  -testFilter "PanicMechanics.Tests.PlayMode.RelayClientSmokeTests" \
  -testResults "$WORK/client.xml" -logFile "$WORK/client.log"
CLIENT_EXIT=$?

echo "=== client exited ($CLIENT_EXIT); waiting for host ==="
wait "$HOST_PID" 2>/dev/null
HOST_EXIT=$?

summary() {
  local xml="$1"
  if [ ! -f "$xml" ]; then echo "missing"; return; fi
  grep -o 'result="[A-Za-z()/]*" total="[0-9]*" passed="[0-9]*" failed="[0-9]*"' "$xml" | head -1
}

echo "=== HOST   : $(summary "$WORK/host.xml") (exit $HOST_EXIT)"
echo "=== CLIENT : $(summary "$WORK/client.xml") (exit $CLIENT_EXIT)"
echo "=== host log:   $WORK/host.log"
echo "=== client log: $WORK/client.log"

rm -rf /tmp/pm-relay-smoke-latest && cp -R "$WORK" /tmp/pm-relay-smoke-latest 2>/dev/null || true
echo "debug copy: /tmp/pm-relay-smoke-latest"

if [ "$HOST_EXIT" -eq 0 ] && [ "$CLIENT_EXIT" -eq 0 ]; then
  echo "RELAY_SMOKE_PASS"
  exit 0
fi
echo "RELAY_SMOKE_FAIL"
exit 1
