#!/bin/bash

# Build script for SqueezeItGood mod
# Requires VINTAGE_STORY environment variable to be set

set -e

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m'

if [ -z "$VINTAGE_STORY" ]; then
    echo -e "${RED}Error: VINTAGE_STORY environment variable not set${NC}"
    echo "Please set it to your Vintage Story installation path, e.g.:"
    echo "  export VINTAGE_STORY=~/.local/share/vintagestory"
    exit 1
fi

echo -e "${GREEN}Building SqueezeItGood mod...${NC}"

dotnet build -c Release

if [ $? -eq 0 ]; then
    echo -e "${GREEN}Build successful!${NC}"

    # Extract version from modinfo.json
    VERSION=$(grep -o '"version":\s*"[^"]*"' modinfo.json | grep -o '[0-9][^"]*')
    MOD_NAME="SqueezeItGood"
    ZIP_NAME="${MOD_NAME}-v${VERSION}.zip"

    # Create release package
    RELEASE_DIR="Releases"
    STAGING_DIR="$RELEASE_DIR/$MOD_NAME"
    mkdir -p "$STAGING_DIR"

    # Copy mod files
    cp bin/SqueezeItGood.dll "$STAGING_DIR/"
    cp modinfo.json "$STAGING_DIR/"

    # Create zip package (files at root, not in subfolder)
    cd "$STAGING_DIR"
    zip -r "../$ZIP_NAME" ./*
    cd ../..

    echo -e "${GREEN}Release package created: $RELEASE_DIR/$ZIP_NAME${NC}"
else
    echo -e "${RED}Build failed!${NC}"
    exit 1
fi
