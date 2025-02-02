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

log_message "INFO" "Starting the before-installation process..."

# Define the backup file path in a safe location
BACKUP_DIR="/var/backups"
DATABASE_BACKUP_FILE_PATH="$BACKUP_DIR/classroom_groups_prod_database_backup.db"
DATABASE_FILE_PATH="./classroom_groups_prod_database.db"

# Ensure the backup directory exists
mkdir -p $BACKUP_DIR

# Check if the database file exists
if [ -f "$DATABASE_FILE_PATH" ]; then
    log_message "INFO" "Backing up database: $DATABASE_FILE_PATH to $DATABASE_BACKUP_FILE_PATH"
    
    # Delete the backup file if it exists
    if [ -f "$DATABASE_BACKUP_FILE_PATH" ]; then
        log_message "INFO" "Backup file already exists. Deleting the previous backup..."
        sudo rm -f "$DATABASE_BACKUP_FILE_PATH"
    fi

    # Run the VACUUM INTO command to backup the database
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

log_message "INFO" "Removing installed app code and systemd service..."
# Clean up app files and service file
sudo rm -rf /var/www/*
sudo rm -rf /etc/systemd/system/webapi.service

log_message "INFO" "Before-installation process completed."
