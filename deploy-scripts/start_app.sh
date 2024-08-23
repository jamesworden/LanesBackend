echo "[Starting App] Remove installed code and the systemd service file..."
systemctl stop webapi.service
systemctl start webapi.service