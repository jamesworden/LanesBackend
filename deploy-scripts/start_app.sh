echo "Remove installed code and start the systemd service file..."
systemctl stop webapi.service
systemctl start webapi.service