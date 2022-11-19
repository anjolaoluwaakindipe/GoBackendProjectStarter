# GOBACKENDPROJECTSTARTER

This is a dotnet project that helps me with the mundane task of creating a golang backend project from scratch. It uses mostly built-in c# libraries and an external package called CliWrap to perform go commands.

### INSTALLATION

1. Clone repository into directory of choice using `git clone <https-repo-link>`.
2. Navigate to project directory.
3. If using the dotnet CLI, you can run `dotnet build` in the terminal to build the projeect, or you can use Visual Studio IDE.
4. You can run the application by running `dotnet run` with dotnet CLI, click on the run button using Visual Studio IDE, or executing the binary in `./bin/Debug/net7.0/GoBackendProjectStarter`

### GO PROJECT STRUCTURE OUTPUT

```bash
{ProjectFolder}/
┣ dtos/
┣ entites/
┣ models/
┣ routes/
┃ ┣ helloHandler.go
┃ ┣ helloRoute.go
┃ ┗ route.go
┣ server/
┃ ┗ server.go
┣ services/
┃ ┣ helloService.go
┃ ┣ helloServiceInterface.go
┃ ┗ service.go
┣ utils/
┃ ┣ config/
┃ ┃ ┗ logger.go
┃ ┗ logger/
┃   ┗ logger.go
┣ .env
┣ .env.example
┣ go.mod
┗ main.go
```

### THINGS I LEARNT

1. How to work with the Cliwrap library to execute shell/bash commands.
2. Create a tree data structure and traverse through it iteratively (using a queue) in order to create files and folders.
3. How to use c# builtin file systen and directory libraries to create, delete, write, and check existing files and folders.
4. Use the ref keyword to pass in string values by reference to a function.
5. Automate a boring task I usually have to do from scratch with C#.
