# Remove installed code and the systemd service file
rm -rf /var/www/*
rm -rf /etc/systemd/system/webapi.service

# Fetch the environment variables from SSM Parameter Store
clientId=$(aws ssm get-parameter --name "/ClassroomGroups/Google/ClientId" --region "us-east-1" --query "Parameter.Value" --output text)
clientSecret=$(aws ssm get-parameter --name "/ClassroomGroups/Google/ClientSecret" --region "us-east-1" --query "Parameter.Value" --output text)

# Set the environment variables
export ClassroomGroups__Authentication__Google__ClientId=$clientId
export ClassroomGroups__Authentication__Google__ClientSecret=$clientSecret