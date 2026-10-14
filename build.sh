#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

GAME_DIR="$HOME/Library/Application Support/Steam/steamapps/common/Cities_Skylines/Cities.app/Contents/Resources/Data/Managed"
MOD_NAME="MoreVehiclesRenew2"
MOD_DIR="$HOME/Library/Application Support/Colossal Order/Cities_Skylines/Addons/Mods/$MOD_NAME"
HARMONY_DLL="$SCRIPT_DIR/Libs/CitiesHarmony.Harmony.dll"
CITIES_HARMONY_API_DLL="$SCRIPT_DIR/Libs/CitiesHarmony.API.dll"

echo "=========================================================="
echo "🚀 Building $MOD_NAME (by NgocDuy)..."
echo "=========================================================="

# Find all cs files
CS_FILES=$(find src -name "*.cs")

echo "1️⃣ Compiling $MOD_NAME.dll with mcs..."
mcs -sdk:2 -target:library \
    -r:"$GAME_DIR/Assembly-CSharp.dll" \
    -r:"$GAME_DIR/ICities.dll" \
    -r:"$GAME_DIR/ColossalManaged.dll" \
    -r:"$GAME_DIR/UnityEngine.dll" \
    -r:"$HARMONY_DLL" \
    -r:"$CITIES_HARMONY_API_DLL" \
    -out:"$MOD_NAME.dll" \
    $CS_FILES

echo "✅ Compiled $MOD_NAME.dll successfully."

# Deploy to game mods directory
echo "2️⃣ Deploying to Cities: Skylines Addons/Mods/$MOD_NAME..."
mkdir -p "$MOD_DIR"
cp "$MOD_NAME.dll" "$MOD_DIR/"
cp "$CITIES_HARMONY_API_DLL" "$MOD_DIR/"

[ -f preview-image.png ] && cp preview-image.png "$MOD_DIR/"

echo "=========================================================="
echo "🎉 DEPLOYMENT COMPLETE!"
echo "Files in $MOD_DIR:"
ls -lh "$MOD_DIR"
echo "=========================================================="
