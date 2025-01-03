# LanesBackend

## Context

### What is this repository?

This repository contains 1 .NET web API for 2 of my applications, namely `ChessOfCards` and `ClassroomGroups`.

### What does "Lanes" mean?

Years ago, I was playing cards with my brother in law and for some reason, we decided to make our own card game. Our creation involved 5 lanes of cards on the table, so we decided to call this game 'Lanes'. This was eventually renamed to 'Chess of Cards' as the domain name `lanes.com` was taken, but `chessofcards.com` was not. So Lanes is the original term we used to represent the game that we made. My nerdiness took over and I decided to turn our game into a web application, whose backend server lives here.

### Why are two different applications hosted here if the project name is 'LanesBackend'?

`chessofcards.com` and `classroomgroups.com` are both new applications and have few users, so it doesn't make sense for me to spend money on multiple servers. My applications that need a backend server will live on this one to save money. Because I now call the original 'Lanes' card game 'Chess of Cards' and because this server now supports backend code for multiple applications, 'Lanes' is now the term used to represent this server in its entirety, that is, the APIs for my smaller projects.

---

## Documentation

### Getting Started Developing Locally

1. Authenticate with the AWS CLI Locally
2. Add the appropriate app secrets locally, addressed below.
3. Run the database migrations using the `dotnet ef database update` command below to prepare your local database so that the server can read and write to it.

### App Secrets & Environment Variables

All sensitive data is stored in AWS Systems Manager > Parameter Store. `Amazon.Extensions.Configuration.SystemsManager` is a package is used to inject environment variables from the Parameter Store like so `AddSystemsManager(builder.Configuration["AppSecrets:SystemsManagerPath"])`.

### Deploying to production

- First we must build an artifact to deploy. Start by opening the terminal and going to the root directory of this repository.
- Here, execute this command: `dotnet publish -o app -r linux-x64 --self-contained`. This assumes you have the dotnet CLI tool installed. The artifact should now be built. The parameters on these commands have become necessary for our Code Deploy agent to deploy the app correctly since the advent of .Net 8.
- You can test that this artifact works locally with `dotnet run ./app/LanesBackend.dll`. If there's any strange behavior or errors, cleaning the contents of that `app` folder before rebuilding an artifact may help.
- Now we must deploy this artifact; push this artifact to Github directly to master or in a new branch.
- Open a new browser tab to login to the AWS Console. Go to the Code Deploy service.
- Select `Applications`, `lanesbackend`, `lanesbackend-dg` (Deployment Group), and then click the `Create Deployment` button
- Fetch the latest commit id by going to the root `LanesBackend` directory and entering the command `git log -1 --format="%H"`.
- Fill out the steps on this page using this repository (`jamesworden/LanesBackend`) and the latest commit id. Finishing this process should take the latest build that you just pushed via git and run it on the prod EC2 instance.

### Powershell Commands for Local Development

- `dotnet restore`: reinstalls nuget packages
- `dotnet clean`: cleans artifacts
- `dotnet watch`: runs application for development with hot reload (If you're using VSCode, there's a `launch.json` script to run the services for you.)

### Application Architecture

#### Some History

As mentioned above, this repository was created under the assumption that one application's APIs would live in this codebase. This application, ChessOfCards, was written with a wishy-washy implementation of the Onion Architecture. It was necessary to write imperfect code - well, impossible not to - because I had to start the project _somehow_. As the complexity and size of ChessOfCards grew, I had this gut feeling that something wasn't right: All of my "business logic" was scattered across domain models like `./legacy/LanesBackend/Models/Game.cs`, but mostly ended up in services like `./legacy/LanesBackend/Logic/GameService.cs`. My code had all the symptoms of what Martin Fowler described as the [Anemic Domain Model](https://martinfowler.com/bliki/AnemicDomainModel.html). This feeling took me down a rabbit hole of trying to understand the _most correct_ (or more realistically, a less incorrect) approach to organizing this functionality.

Trying to find the best way to organize my code, I stumbled upon Jimmy Bogard and his 'Vertical Slice Architecture'. The original `LanesBackend` application was moved to the `legacy` folder in this repository and rewritten with my interpretation of the Vertical Slice Architecture. Out of curiosity or the pursuit to make my code the _most correct_ it could be, I decided to assign a new .NET class library project to each layer of my application so as to enforce a uni-directional dependency chain. Bogard claims to wait until it's necessary before splitting out code into new projects, but I was eager to start experimenting.

All of this spawned `src/ChessOfCards` and it's different .NET projects, each of which belonging to a 'Layer' - or position in the dependency chain. This pattern eased my concerns about not doing things _the right way_. It helped me improve upon `chessofcards.com`. So for the sake of posterity, familiarity, organization, and efficiency, i've decided to shape `src/ClassroomGroups` in a similar way.

#### Application Startup

Hosting multiple .NET applications on the same linux server is actually quite difficult; i've circumnavigated this issue by hosting 1 actual .NET web API, `src/LanesBackendLauncher`, which initializes and uses the projects defined in `src/ClassroomGroups` and `src/ChessOfCards` in `src/LanesBackendLauncher/Program.cs`.

### ClassroomGroups Developer Commentary

#### Data Access Model Conventions

##### Key Vs. Id

Private, internal primary keys of tables end with `Key` while public facing identifiers for rows of data end with `Id`. In other words, anything that is a Key is database specific and to be used internally. Alternatively, users can see Id's without an issue.

#### Domain Model Conventions

##### View Models

Often, our domain models will have properties that we don't want to expose to our API endpoints. An example of this is `Account.Key`. The only identification property that clients should have access to is `Account.Id`. For this reason, we must transform an `Account` to an `AccountView`, where every property is the same except the key property no longer exists on the view model. A domain model with "View" appended to the end of it hides properties from the initial model that the user should not see.

#### Database Migrations

Entity framework modifies the database according to a DBContext file (for example, `ClassroomGroupContext.cs`). To update the database via database migrations, make changes to your context file accordingly. Then, execute the following commands from the root directory of this repository:

- `dotnet ef migrations add YOUR_MIGRATION_NAME --startup-project ./src/LanesBackendLauncher/LanesBackendLauncher.csproj --project .\src\ClassroomGroups\ClassroomGroups.DataAccess\`
- `dotnet ef database update --startup-project ./src/LanesBackendLauncher/LanesBackendLauncher.csproj --project .\src\ClassroomGroups\ClassroomGroups.DataAccess\`
