#!/usr/bin/env bash
# Portable build+run for MiniChain.
#
# On a NORMAL machine with a working .NET SDK, just use:
#     dotnet run -c Release
#
# This script exists only for locked-down/offline environments where the
# MSBuild worker process hangs (no localhost sockets / no NuGet). It invokes
# the Roslyn compiler (csc.dll) directly against the runtime reference
# assemblies, which needs no network and no MSBuild nodes.
set -euo pipefail

# Point this at your dotnet install root (the dir containing ./dotnet, ./sdk, ./packs, ./shared)
DOTNET_ROOT="${DOTNET_ROOT:-$(dirname "$(command -v dotnet)")}"
export DOTNET_CLI_HOME="${DOTNET_CLI_HOME:-/tmp}"
export DOTNET_CLI_TELEMETRY_OPTOUT=1

SDK_VER="$(ls "$DOTNET_ROOT/sdk" | sort -V | tail -1)"
RT_VER="$(ls "$DOTNET_ROOT/shared/Microsoft.NETCore.App" | sort -V | tail -1)"
REF_VER="$(ls "$DOTNET_ROOT/packs/Microsoft.NETCore.App.Ref" | sort -V | tail -1)"

CSC="$DOTNET_ROOT/sdk/$SDK_VER/Roslyn/bincore/csc.dll"
REFDIR="$DOTNET_ROOT/packs/Microsoft.NETCore.App.Ref/$REF_VER/ref/net6.0"

cd "$(dirname "$0")"
mkdir -p out
REFS=""; for d in "$REFDIR"/*.dll; do REFS="$REFS -r:$d"; done

"$DOTNET_ROOT/dotnet" exec "$CSC" -nologo -nostdlib -langversion:10 -nullable:enable \
    -out:out/MiniChain.dll -target:exe $REFS \
    GlobalUsings.cs Crypto.cs MerkleTree.cs Wallet.cs Transaction.cs Block.cs Blockchain.cs Program.cs

cat > out/MiniChain.runtimeconfig.json <<JSON
{ "runtimeOptions": { "tfm": "net6.0",
  "framework": { "name": "Microsoft.NETCore.App", "version": "6.0.0" } } }
JSON

echo "=== running ==="
"$DOTNET_ROOT/dotnet" out/MiniChain.dll
