#!/usr/bin/bash

APPSETTINGS_PATH="/var/www/appsettings.json"

if [ ! -f "$APPSETTINGS_PATH" ]; then
    echo "[Error] appsettings.json not found! Exiting..."
    exit 1
fi

# Extract DatabaseFilePath from appsettings.json
DATABASE_FILE=$(jq -r '.DatabaseBackup.DatabaseFilePath' "$APPSETTINGS_PATH")

if [ -z "$DATABASE_FILE" ]; then
    echo "[Error] Failed to read DatabaseFilePath from appsettings.json! Exiting..."
    exit 1
fi

echo "[Before Installing App] Removing installed code and the systemd service file..."
sudo rm -rf /var/www/*

# Restore the database file after removing everything
echo "[Before Installing App] Preserving database file: $DATABASE_FILE"
sudo mv "$DATABASE_FILE" /tmp/ 2>/dev/null || echo "[Info] No database file to preserve."

sudo rm -rf /etc/systemd/system/webapi.service

# Move the database file back
if [ -f "/tmp/$(basename "$DATABASE_FILE")" ]; then
    sudo mv "/tmp/$(basename "$DATABASE_FILE")" "$DATABASE_FILE"
    echo "[After Install] Restored database file: $DATABASE_FILE"
fi
