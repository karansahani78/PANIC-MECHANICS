#!/usr/bin/env bash
# Runs the standalone Unity-free simulation tests (Tools/SimVerify).
set -euo pipefail

here="$(cd "$(dirname "$0")" && pwd)"

export DOTNET_ROOT="${DOTNET_ROOT:-/opt/homebrew/opt/dotnet/libexec}"
export DOTNET_CLI_TELEMETRY_OPTOUT=1

exec dotnet test "$here/SimVerify/SimVerify.csproj" "$@"
