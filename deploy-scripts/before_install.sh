#!/usr/bin/bash

param (
    [string]$AppSettingsPath = "/var/www/appsettings.json"
)

$LogFile = "/var/log/before_install.log"
Start-Transcript -Path $LogFile -Append

Write-Host "========== [START] $(Get-Date) =========="

# Validate appsettings.json exists
if (-Not (Test-Path $AppSettingsPath)) {
    Write-Host "[Error] appsettings.json not found! Exiting..."
    Stop-Transcript
    exit 1
}

# Read database paths from appsettings.json
try {
    $AppSettings = Get-Content $AppSettingsPath | ConvertFrom-Json
    $DatabaseFilePath = $AppSettings.DatabaseBackup.DatabaseFilePath
    $DatabaseBackupFilePath = $AppSettings.DatabaseBackup.DatabaseBackupFilePath
} catch {
    Write-Host "[Error] Failed to parse appsettings.json. Exiting..."
    Stop-Transcript
    exit 1
}

if (-Not $DatabaseFilePath) {
    Write-Host "[Error] DatabaseFilePath is empty! Exiting..."
    Stop-Transcript
    exit 1
}

if (-Not $DatabaseBackupFilePath) {
    Write-Host "[Error] DatabaseBackupFilePath is empty! Exiting..."
    Stop-Transcript
    exit 1
}

$TempBackupPath = "/tmp/" + (Split-Path -Leaf $DatabaseBackupFilePath)

Write-Host "[Before Install] Backing up database file: $DatabaseFilePath"

# Create a SQLite backup
if (Test-Path $DatabaseFilePath) {
    try {
        sqlite3 $DatabaseFilePath "VACUUM INTO '$DatabaseBackupFilePath';"
        Move-Item -Path $DatabaseBackupFilePath -Destination $TempBackupPath -Force
        Write-Host "[Info] Database backup created at $TempBackupPath"
    } catch {
        Write-Host "[Error] Failed to create database backup. Exiting..."
        Stop-Transcript
        exit 1
    }
} else {
    Write-Host "[Info] Database file not found. Proceeding..."
}

# Remove installed app
Write-Host "[Before Install] Removing installed code and systemd service..."
Remove-Item -Path "/var/www/*" -Recurse -Force
Remove-Item -Path "/etc/systemd/system/webapi.service" -Force

# Restore the database file from backup
if (Test-Path $TempBackupPath) {
    Move-Item -Path $TempBackupPath -Destination $DatabaseFilePath -Force
    Write-Host "[After Install] Restored database file: $DatabaseFilePath"
} else {
    Write-Host "[Warning] Database backup file was not found after install!"
}

Write-Host "========== [END] $(Get-Date) =========="
Stop-Transcript
