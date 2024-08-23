#!/usr/bin/bash

echo "[Before Installing App] Removing installed code and the systemd service file..."
sudo rm -rf /var/www/*
sudo rm -rf /etc/systemd/system/webapi.service

echo "[Before Installing App] Fetching app secrets from aws parameter store..."
clientId=$(aws ssm get-parameter --name "/ClassroomGroups/Google/ClientId" --region "us-east-1" --query "Parameter.Value" --output text)
clientSecret=$(aws ssm get-parameter --name "/ClassroomGroups/Google/ClientSecret" --region "us-east-1" --query "Parameter.Value" --output text)

echo "[Before Installing App] /ClassroomGroups/Google/ClientId: $clientId"
echo "[Before Installing App] /ClassroomGroups/Google/ClientSecret: $clientSecret"

echo "[Before Installing App] Persisting app secrets as environment variables..."
sudo sh -c "echo 'ClassroomGroups__Authentication__Google__ClientId=\"$clientId\"' >> /etc/environment"
sudo sh -c "echo 'ClassroomGroups__Authentication__Google__ClientSecret=\"$clientSecret\"' >> /etc/environment"
source /etc/environment