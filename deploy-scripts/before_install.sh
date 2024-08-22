﻿#!/bin/bash

echo "[Before Installing App] Removing installed code and the systemd service file..."
rm -rf /var/www/*
rm -rf /etc/systemd/system/webapi.service

echo "[Before Installing App] Fetching app secrets from aws parameter store..."
clientId=$(aws ssm get-parameter --name "/ClassroomGroups/Google/ClientId" --region "us-east-1" --query "Parameter.Value" --output text)
clientSecret=$(aws ssm get-parameter --name "/ClassroomGroups/Google/ClientSecret" --region "us-east-1" --query "Parameter.Value" --output text)

echo "[Before Installing App] Persisting app secrets as environment variables..."
export ClassroomGroups__Authentication__Google__ClientId=$clientId
export ClassroomGroups__Authentication__Google__ClientSecret=$clientSecret