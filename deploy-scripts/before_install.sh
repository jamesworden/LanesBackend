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

log_message "INFO" "[Before Installing App] Starting backup and removal process..."

# Read the database and backup file paths from appsettings.json
DATABASE_FILE_PATH=$(jq -r '.DatabaseBackup.DatabaseFilePath' /path/to/appsettings.json)
DATABASE_BACKUP_FILE_PATH=$(jq -r '.DatabaseBackup.DatabaseBackupFilePath' /path/to/appsettings.json)

# Ensure both variables are non-empty
if [[ -z "$DATABASE_FILE_PATH" || -z "$DATABASE_BACKUP_FILE_PATH" ]]; then
    log_message "ERROR" "Failed to read database file paths from appsettings.json"
    exit 1
fi

# Check if the backup already exists, and remove it if it does
if [ -f "$DATABASE_BACKUP_FILE_PATH" ]; then
    log_message "INFO" "Backup file already exists. Deleting previous backup..."
    rm -f "$DATABASE_BACKUP_FILE_PATH"
fi

# Check if the database file exists
if [ -f "$DATABASE_FILE_PATH" ]; then
    log_message "INFO" "Backing up database: $DATABASE_FILE_PATH"
    sudo sqlite3 "$DATABASE_FILE_PATH" "VACUUM INTO '$DATABASE_BACKUP_FILE_PATH';"

    # Check if the sqlite3 command succeeded
    if [ $? -eq 0 ]; then
        log_message "INFO" "Backup completed successfully: $DATABASE_BACKUP_FILE_PATH"
    else
        log_message "ERROR" "Failed to backup database: $DATABASE_FILE_PATH"
        exit 1
    fi
else
    log_message "WARNING" "Database file not found at $DATABASE_FILE_PATH! Skipping backup..."
fi

# Remove installed app files and systemd service
log_message "INFO" "Removing installed code from /var/www/ and systemd service file..."
sudo rm -rf /var/www/*
sudo rm -f /etc/systemd/system/webapi.service

log_message "INFO" "[Before Installing App] Completed backup and removal process."
