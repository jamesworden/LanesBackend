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

log_message "INFO" "[After Installing App] Starting database restoration process..."

# Read the database and backup file paths from appsettings.json
DATABASE_FILE_PATH=$(jq -r '.DatabaseBackup.DatabaseFilePath' /path/to/appsettings.json)
DATABASE_BACKUP_FILE_PATH=$(jq -r '.DatabaseBackup.DatabaseBackupFilePath' /path/to/appsettings.json)

# Ensure both variables are non-empty
if [[ -z "$DATABASE_FILE_PATH" || -z "$DATABASE_BACKUP_FILE_PATH" ]]; then
    log_message "ERROR" "Failed to read database file paths from appsettings.json"
    exit 1
fi

# Check if the backup file exists
if [ -f "$DATABASE_BACKUP_FILE_PATH" ]; then
    log_message "INFO" "Restoring the database from backup..."
    sudo sqlite3 "$DATABASE_FILE_PATH" ".restore '$DATABASE_BACKUP_FILE_PATH'"

    # Check if the sqlite3 restore command succeeded
    if [ $? -eq 0 ]; then
        log_message "INFO" "Database restoration completed successfully from: $DATABASE_BACKUP_FILE_PATH"
    else
        log_message "ERROR" "Failed to restore database from: $DATABASE_BACKUP_FILE_PATH"
        exit 1
    fi
else
    log_message "ERROR" "Backup file not found at $DATABASE_BACKUP_FILE_PATH! Skipping restoration..."
    exit 1
fi

log_message "INFO" "[After Installing App] Database restoration completed."
