#!/usr/bin/env bash
# Two-client smoke test: host runs on an APFS clone of the project (Unity forbids
# two instances on the SAME project path), joining client runs on the real repo.
# Usage: Tools/two-client-smoke.sh [repo]
set -uo pipefail

REPO="${1:-$(pwd)}"
UNITY="${UNITY:-/Applications/Unity/Hub/Editor/6000.3.26f1/Unity.app/Contents/MacOS/Unity}"
MARKER="${PM_MARKER:-/tmp/pm-host-ready}"
WORK="$(mktemp -d)"
HOST_DIR="$WORK/host"

rm -f "$MARKER"
export PM_TWOCLIENT=1

echo "=== cloning project for host instance ==="
mkdir -p "$HOST_DIR"
cp -cR "$REPO/Assets" "$REPO/Packages" "$REPO/ProjectSettings" "$REPO/Library" \
  "$REPO/UserSettings" "$HOST_DIR/"
echo "clone ready: $HOST_DIR"

cleanup() { rm -rf "$WORK"; }
trap cleanup EXIT

echo "=== starting host process (clone) ==="
"$UNITY" -batchmode -nographics -projectPath "$HOST_DIR" -runTests -testPlatform PlayMode \
  -testFilter "PanicMechanics.Tests.PlayMode.HostProcessSmokeTests" \
  -testResults "$WORK/host.xml" -logFile "$WORK/host.log" &
HOST_PID=$!

echo "=== waiting for host ready ==="
READY=0
for _ in $(seq 1 240); do
  if [ -f "$MARKER" ]; then READY=1; break; fi
  if ! kill -0 "$HOST_PID" 2>/dev/null; then break; fi
  sleep 1
done

if [ "$READY" -ne 1 ]; then
  echo "HOST_NOT_READY"
  tail -30 "$WORK/host.log" 2>/dev/null || true
  wait "$HOST_PID" 2>/dev/null || true
  exit 1
fi

echo "=== host ready; starting client process (real repo) ==="
"$UNITY" -batchmode -nographics -projectPath "$REPO" -runTests -testPlatform PlayMode \
  -testFilter "PanicMechanics.Tests.PlayMode.ClientProcessSmokeTests" \
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

rm -rf /tmp/pm-smoke-latest && cp -R "$WORK" /tmp/pm-smoke-latest 2>/dev/null || true
echo "debug copy: /tmp/pm-smoke-latest"

if [ "$HOST_EXIT" -eq 0 ] && [ "$CLIENT_EXIT" -eq 0 ]; then
  echo "TWO_CLIENT_SMOKE_PASS"
  exit 0
fi
echo "TWO_CLIENT_SMOKE_FAIL"
exit 1