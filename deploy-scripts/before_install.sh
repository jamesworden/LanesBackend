#!/usr/bin/bash

echo "[Before Installing App] Removing installed code and the systemd service file..."
sudo rm -rf /var/www/*
sudo rm -rf /etc/systemd/system/webapi.service