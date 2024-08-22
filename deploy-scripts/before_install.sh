#!/bin/bash

# Remove installed code and the systemd service file
rm -rf /var/www/*
rm -rf /etc/systemd/system/webapi.service

# Set the AWS region
AWS_REGION="us-east-1"

# Define the mapping of SSM parameters to environment variables
declare -A PARAMETER_MAP=(
  ["ClassroomGroups/Authentication/Google/ClientId"]="ClassroomGroups__Authentication__Google__ClientId"
  ["ClassroomGroups/Authentication/Google/ClientSecret"]="ClassroomGroups__Authentication__Google__ClientSecret"
)

# Fetch each parameter and export it as an environment variable
for PARAM_NAME in "${!PARAMETER_MAP[@]}"; do
  # Fetch the parameter value from SSM Parameter Store
  PARAM_VALUE=$(aws ssm get-parameter --name "$PARAM_NAME" --with-decryption --region "$AWS_REGION" --query "Parameter.Value" --output text)
  
  # Get the corresponding environment variable name
  ENV_VAR_NAME="${PARAMETER_MAP[$PARAM_NAME]}"
  
  # Export the parameter value as an environment variable
  export "$ENV_VAR_NAME"="$PARAM_VALUE"
done