# LanesBackend

## Deploying to production

-   First we must build an artifact to deploy. Start by opening the terminal and going to the following directory: `LanesBackend\LanesBackend`.
-   Here, execute this command: `dotnet publish -o ../app`. This assumes you have the dotnet CLI tool installed. The artifact should now be built.
-   You can test that this artifact works locally with `dotnet run ./app/LanesBackend.dll`. If there's any strange behavior or errors, cleaning the contents of that `app` folder before rebuilding an artifact may help.
-   Now we must deploy this artifact; push this artifact to Github directly to master or in a new branch.
-   Open a new browser tab to login to the AWS Console. Go to the Code Deploy service.
-   Select `Applications`, `lanesbackend`, `lanesbackend-dg` (Deployment Group), and then click the `Create Deployment` button
-   Fetch the latest commit id by going to the root `LanesBackend` directory and entering the command `git log -1 --format="%H"`.
-   Fill out the steps on this page using this repository (`jamesworden/LanesBackend`) and the latest commit id. Finishing this process should take the latest build that you just pushed via git and run it on the prod EC2 instance.

## Commands

-   `dotnet restore`: reinstalls nuget packages
-   `dotnet clean`: cleans artifacts
-   `dotnet watch`: runs application for development with hot reload
