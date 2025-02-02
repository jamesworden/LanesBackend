#!/usr/bin/bash

# Function to log messages in a uniform format
log_message() {
    local log_level="$1"
    local message="$2"
    local timestamp
    timestamp=$(date '+%Y-%m-%d %H:%M:%S')
    
    # Print the log message in the format: [timestamp] [log_level] - message
    echo "[$timestamp] [$log_level] - $message"
}

log_message "INFO" "Starting the after-installation process..."

# Define the appsettings.json file path
APP_SETTINGS_FILE="appsettings.json"

# Read DatabaseFileName and DatabaseBackupFileName from appsettings.json
DATABASE_FILE_NAME=$(jq -r '.DatabaseBackup.DatabaseFileName' $APP_SETTINGS_FILE)
DATABASE_BACKUP_FILE_NAME=$(jq -r '.DatabaseBackup.DatabaseBackupFileName' $APP_SETTINGS_FILE)

# Define the backup file path in a safe location
BACKUP_DIR="/var/backups"
DATABASE_BACKUP_FILE_PATH="$BACKUP_DIR/$DATABASE_BACKUP_FILE_NAME"
DATABASE_FILE_PATH="/var/www/$DATABASE_FILE_NAME"

# Check if the backup file exists
if [ -f "$DATABASE_BACKUP_FILE_PATH" ]; then
    log_message "INFO" "Restoring database from backup: $DATABASE_BACKUP_FILE_PATH to $DATABASE_FILE_PATH"
    sudo cp "$DATABASE_BACKUP_FILE_PATH" "$DATABASE_FILE_PATH"

    # Check if the copy succeeded
    if [ $? -eq 0 ]; then
        log_message "INFO" "Database restored successfully from $DATABASE_BACKUP_FILE_PATH"
    else
        log_message "ERROR" "Failed to restore database from $DATABASE_BACKUP_FILE_PATH"
        exit 1
    fi
else
    log_message "ERROR" "Backup file not found at $DATABASE_BACKUP_FILE_PATH! Cannot restore database."
    exit 1
fi

log_message "INFO" "After-installation process completed."
