using CliWrap;
using CliWrap.Buffered;
using GoBackendProjectStarter.Utils;

internal class Program
{
    private static async Task Main()
    {
        bool done = false;
        try
        {
            // get folder name 
            String? folderName = null;
            {
                InputValidation.GetUserInput(inputVar: ref folderName, message: "Please input a valid folder name for the project: ");
            }

            // get moduile name from user
            String? moduleName = null;
            {
                InputValidation.GetUserInput(inputVar: ref moduleName, message: "Please input a valid module name: ");
            }

            // get directory from user
            String? directory = null;
            {
                InputValidation.GetUserInput(inputVar: ref directory, message: "Please input the directory you want your project to be created: ");
            }

            if (string.IsNullOrWhiteSpace(directory) || string.IsNullOrWhiteSpace(moduleName) || string.IsNullOrWhiteSpace(folderName)) return;

            Directory.SetCurrentDirectory(directory);


            FileCreator fileCreator = new(workingDir: directory, moduleName: moduleName, projectFolderName: folderName);

            Console.CancelKeyPress += (object? sender, ConsoleCancelEventArgs e) =>
            {
                if (!done)
                {
                    var projectDir = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                    if (Directory.Exists(projectDir))
                    {
                        Directory.Delete(projectDir, true);
                    }
                }
            };

            done = await fileCreator.CreateProject();

            Console.WriteLine("SUCCESSFULLY CREATED PROJECT");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}