#!/usr/bin/bash

LOG_FILE="/var/log/before_install.log"
APP_SETTINGS_PATH="/var/www/appsettings.json"
TEMP_BACKUP_DIR="/tmp"
BACKUP_FILE="classroom_groups_db_backup.db"

echo "========== [START] $(date) ==========" | tee -a $LOG_FILE

# Ensure appsettings.json exists
if [ ! -f "$APP_SETTINGS_PATH" ]; then
    echo "[ERROR] appsettings.json not found! Exiting..." | tee -a $LOG_FILE
    exit 1
fi

# Extract database file path
DATABASE_FILE_PATH=$(jq -r '.DatabaseBackup.DatabaseFilePath' "$APP_SETTINGS_PATH")
TEMP_BACKUP_PATH="$TEMP_BACKUP_DIR/$BACKUP_FILE"

echo "[INFO] Backing up database: $DATABASE_FILE_PATH" | tee -a $LOG_FILE

# Create SQLite backup **before deleting files**
if [ -f "$DATABASE_FILE_PATH" ]; then
    sqlite3 "$DATABASE_FILE_PATH" "VACUUM INTO '$TEMP_BACKUP_PATH';"
    echo "[SUCCESS] Backup created at: $TEMP_BACKUP_PATH" | tee -a $LOG_FILE
else
    echo "[WARNING] Database file not found! Skipping backup..." | tee -a $LOG_FILE
fi

# Remove installed code **but NOT the database**
echo "[INFO] Removing installed app code and systemd service..." | tee -a $LOG_FILE
sudo rm -rf /var/www/*
sudo rm -rf /etc/systemd/system/webapi.service

# Restore the database **if a backup exists**
if [ -f "$TEMP_BACKUP_PATH" ]; then
    mv "$TEMP_BACKUP_PATH" "$DATABASE_FILE_PATH"
    echo "[SUCCESS] Restored database: $DATABASE_FILE_PATH" | tee -a $LOG_FILE
else
    echo "[WARNING] No backup found after install! Database may be empty." | tee -a $LOG_FILE
fi

echo "========== [END] $(date) ==========" | tee -a $LOG_FILE
