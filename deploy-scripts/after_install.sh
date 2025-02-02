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
APP_SETTINGS_FILE="/var/www/appsettings.json"

# Read DatabaseFileName and DatabaseBackupFileName from appsettings.json
DATABASE_FILE_NAME=$(jq -r '.DatabaseBackup.DatabaseFileName' $APP_SETTINGS_FILE)
DATABASE_BACKUP_FILE_NAME=$(jq -r '.DatabaseBackup.DatabaseBackupFileName' $APP_SETTINGS_FILE)

# Log the extracted values
log_message "INFO" "Extracted DatabaseFileName: $DATABASE_FILE_NAME"
log_message "INFO" "Extracted DatabaseBackupFileName: $DATABASE_BACKUP_FILE_NAME"

# Check if the extracted values are valid
if [ -z "$DATABASE_FILE_NAME" ] || [ -z "$DATABASE_BACKUP_FILE_NAME" ]; then
    log_message "ERROR" "DatabaseFileName or DatabaseBackupFileName is missing in appsettings.json"
    exit 1
fi

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
