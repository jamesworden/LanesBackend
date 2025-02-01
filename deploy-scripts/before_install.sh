#!/usr/bin/bash

LOG_FILE="/var/log/before_install.log"
APP_SETTINGS_PATH="/var/www/appsettings.json"

echo "========== [START] $(date) ==========" | tee -a $LOG_FILE

# Check if appsettings.json exists
if [ ! -f "$APP_SETTINGS_PATH" ]; then
    echo "[Error] appsettings.json not found! Exiting..." | tee -a $LOG_FILE
    exit 1
fi

# Extract values from JSON
DATABASE_FILE_PATH=$(jq -r '.DatabaseBackup.DatabaseFilePath' "$APP_SETTINGS_PATH")
DATABASE_BACKUP_FILE_PATH=$(jq -r '.DatabaseBackup.DatabaseBackupFilePath' "$APP_SETTINGS_PATH")
TEMP_BACKUP_PATH="/tmp/$(basename $DATABASE_BACKUP_FILE_PATH)"

echo "[Before Install] Backing up database file: $DATABASE_FILE_PATH" | tee -a $LOG_FILE

# Create SQLite backup if the file exists
if [ -f "$DATABASE_FILE_PATH" ]; then
    sqlite3 "$DATABASE_FILE_PATH" "VACUUM INTO '$DATABASE_BACKUP_FILE_PATH';"
    mv "$DATABASE_BACKUP_FILE_PATH" "$TEMP_BACKUP_PATH"
    echo "[Info] Database backup created at $TEMP_BACKUP_PATH" | tee -a $LOG_FILE
else
    echo "[Info] Database file not found. Proceeding..." | tee -a $LOG_FILE
fi

# Remove installed app
echo "[Before Install] Removing installed code and systemd service..." | tee -a $LOG_FILE
sudo rm -rf /var/www/*
sudo rm -rf /etc/systemd/system/webapi.service

# Restore the database file from backup
if [ -f "$TEMP_BACKUP_PATH" ]; then
    mv "$TEMP_BACKUP_PATH" "$DATABASE_FILE_PATH"
    echo "[After Install] Restored database file: $DATABASE_FILE_PATH" | tee -a $LOG_FILE
else
    echo "[Warning] Database backup file was not found after install!" | tee -a $LOG_FILE
fi

echo "========== [END] $(date) ==========" | tee -a $LOG_FILE
