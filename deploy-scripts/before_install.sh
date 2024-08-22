# Remove installed code and the systemd service file
rm -rf /var/www/*
rm -rf /etc/systemd/system/webapi.service

# Install AWS CLI if not already installed
sudo apt-get update
sudo apt-get install -y awscli

# Fetch the environment variables from SSM Parameter Store
clientId=$(aws ssm get-parameter --name "/ClassroomGroups/Google/ClientId" --query "Parameter.Value" --output text)
clientSecret=$(aws ssm get-parameter --name "/ClassroomGroups/Google/ClientSecret" --query "Parameter.Value" --output text)

# Set the environment variables
export ClassroomGroups__Authentication__Google__ClientId=$clientId
export ClassroomGroups__Authentication__Google__ClientSecret=$clientSecret