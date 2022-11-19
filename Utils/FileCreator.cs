using System.Reflection.Metadata;
using Microsoft.VisualBasic.CompilerServices;
using GoBackendProjectStarter.Entities;
using CliWrap;
using CliWrap.Buffered;

namespace GoBackendProjectStarter.Utils;

internal class FileCreator
{
    private readonly String _workingDir;
    private readonly String _moduleName;
    private readonly String _projectFolderName;

    public FileCreator(String workingDir, String moduleName, String projectFolderName)
    {
        _workingDir = workingDir;
        _moduleName = moduleName;
        _projectFolderName = projectFolderName;
    }

    private static FileDir ProjectSetup(String moduleName, String projectFolderName)
    {
        // utils folder i.e(containse logger and appconfig)
        FileDir loggerGo = new() { PathName = "logger.go", FileText = ProjectText.LoggerGo() };
        FileDir appConfigGo = new() { PathName = "logger.go", FileText = ProjectText.AppConfigGo(moduleName) };
        FileDir loggerFolder = new() { PathName = "logger", FolderChildren = new List<FileDir>() { loggerGo } };
        FileDir configFolder = new() { PathName = "config", FolderChildren = new List<FileDir>() { appConfigGo } };
        FileDir utilsFolder = new() { PathName = "utils", FolderChildren = new List<FileDir>() { configFolder, loggerFolder } };

        // routes folder
        FileDir routeGo = new() { PathName = "route.go", FileText = ProjectText.RouteGo(moduleName) };
        FileDir helloRouteGo = new() { PathName = "helloRoute.go", FileText = ProjectText.HelloRouteGo(moduleName) };
        FileDir helloHandlerGo = new() { PathName = "helloHandler.go", FileText = ProjectText.HelloHandler(moduleName) };
        FileDir routesFolder = new() { PathName = "routes", FolderChildren = new List<FileDir>() { routeGo, helloRouteGo, helloHandlerGo } };

        // services folder
        FileDir helloServiceGo = new() { PathName = "helloService.go", FileText = ProjectText.HelloServiceGo() };
        FileDir helloServiceInterfaceGo = new() { PathName = "helloServiceInterface.go", FileText = ProjectText.HelloServiceInterface() };
        FileDir serviceGo = new() { PathName = "service.go", FileText = ProjectText.ServiceGo() };
        FileDir serviceFolder = new() { PathName = "services", FolderChildren = new List<FileDir>() { helloServiceGo, helloServiceInterfaceGo, serviceGo } };

        // server folder
        FileDir serverGo = new() { PathName = "server.go", FileText = ProjectText.ServerGo(moduleName) };
        FileDir serverFolder = new() { PathName = "server", FolderChildren = new List<FileDir>() { serverGo } };

        // main.go file
        FileDir mainGo = new() { PathName = "main.go", FileText = ProjectText.MainGo(moduleName) };

        // .env.example and .env
        FileDir envExample = new() { PathName = ".env.example", FileText = ProjectText.EnvExample() };
        FileDir env= new() {PathName= ".env", FileText=ProjectText.EnvExample()};

        // dto folder
        FileDir dtosrFolder = new() { PathName = "dtos", };

        // enities folder
        FileDir entitiesFolder = new() { PathName = "entites", };

        // models folder
        FileDir modelsFolder = new() { PathName = "models", };

        // folderName/eorking directory
        FileDir workingDirectory = new() { PathName = projectFolderName, FolderChildren = new List<FileDir>() { utilsFolder, routesFolder, serviceFolder, serverFolder, dtosrFolder, entitiesFolder, modelsFolder, mainGo, envExample, env } };
        return workingDirectory;
    }

    public async Task<bool> CreateProject()
    {
        try
        {
            // create the file tree and return the root of the project
            var projectRoot = ProjectSetup(_moduleName, _projectFolderName);

            // create the slash system dependent of the underlying platform
            var slash = _workingDir.Contains('/') ? "/" : "\\";
            // append working directory and slash the project folder name
            projectRoot.PathName = Path.Combine(Directory.GetCurrentDirectory(), projectRoot.PathName);

            // create a folder queue that would help traverse the project tree
            var folderQueue = new Queue<FileDir>();
            // add the root folder to the Queue 
            folderQueue.Enqueue(projectRoot);

            while (folderQueue.Count != 0)
            {
                // dequeue the first element in the queue
                var currentFileDir = folderQueue.Dequeue();

                // if a folder
                if (currentFileDir.FileText is null  && !Directory.Exists(currentFileDir.PathName))
                {
                    // create directory with it's edit path name
                    Directory.CreateDirectory(currentFileDir.PathName);

                    // initiates go mod init on project root directory
                    if (currentFileDir.PathName == projectRoot.PathName)
                    {
                        Directory.SetCurrentDirectory(projectRoot.PathName);
                        var goModCmd = await Cli.Wrap("go").WithArguments(new String[] { "mod", "init", $"{_moduleName}" }).ExecuteBufferedAsync();

                        Console.WriteLine(goModCmd.StandardOutput);
                        Directory.SetCurrentDirectory(_workingDir);
                    }
                    // iterate through its children if it is a foldeer
                    foreach (FileDir child in currentFileDir.FolderChildren)
                    {
                        // change the pathname of the children acccording to the parent 
                        child.PathName = Path.Combine(currentFileDir.PathName, child.PathName);
                        // enqueue the child into the queue
                        folderQueue.Enqueue(child);
                    }
                }

                // if file
                if (currentFileDir.FileText is not null && !currentFileDir.FolderChildren.Any() && !File.Exists(currentFileDir.PathName))
                {
                    // create the file and write its text into the file
                    await File.WriteAllTextAsync(currentFileDir.PathName, currentFileDir.FileText);
                }
            }
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
    }
}