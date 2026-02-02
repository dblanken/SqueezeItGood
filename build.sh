#!/bin/bash

# Build script for SqueezeItGood mod
# Builds the mod and optionally packages it for distribution

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}Building SqueezeItGood mod...${NC}"

# Try to find Vintage Story installation
VS_PATHS=(
    "$HOME/.local/share/vintagestory"
    "/opt/vintagestory"
    "$HOME/.steam/steam/steamapps/common/Vintage Story"
    "C:/Program Files/Vintagestory"
)

VS_PATH=""
for path in "${VS_PATHS[@]}"; do
    if [ -d "$path" ]; then
        VS_PATH="$path"
        break
    fi
done

if [ -z "$VS_PATH" ]; then
    echo -e "${YELLOW}Warning: Could not auto-detect Vintage Story installation.${NC}"
    echo "Please set VSPath manually in the .csproj or pass it as an argument:"
    echo "  dotnet build -p:VSPath=/path/to/vintagestory"
else
    echo -e "Found Vintage Story at: ${GREEN}$VS_PATH${NC}"
fi

# Build the mod
if [ -n "$VS_PATH" ]; then
    dotnet build -c Release -p:VSPath="$VS_PATH"
else
    dotnet build -c Release
fi

if [ $? -eq 0 ]; then
    echo -e "${GREEN}Build successful!${NC}"

    # Create release package
    RELEASE_DIR="release"
    mkdir -p "$RELEASE_DIR"

    # Copy mod files
    cp bin/SqueezeItGood.dll "$RELEASE_DIR/"
    cp modinfo.json "$RELEASE_DIR/"

    # Create zip package
    cd "$RELEASE_DIR"
    zip -r ../SqueezeItGood.zip ./*
    cd ..

    echo -e "${GREEN}Release package created: SqueezeItGood.zip${NC}"
    echo ""
    echo "To install:"
    echo "  1. Extract SqueezeItGood.zip to your Vintage Story Mods folder"
    echo "     Linux: ~/.config/VintagestoryData/Mods/"
    echo "     Windows: %appdata%/VintagestoryData/Mods/"
    echo "  2. Or copy the 'release' folder directly to your Mods folder"
else
    echo -e "${RED}Build failed!${NC}"
    exit 1
fi
