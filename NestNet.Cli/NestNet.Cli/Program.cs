using NestNet.Cli.Generators;
using NestNet.Cli.Installation;
using Spectre.Console;
using System.CommandLine;
using System.Runtime.InteropServices;

class Program
{
    [DllImport("kernel32.dll")]
    static extern bool AllocConsole();

    [DllImport("kernel32.dll")]
    static extern bool FreeConsole();

    static async Task<int> Main(string[] args)
    {
        bool showConsole = !args.Contains("--no-console") ;

        if (showConsole)
        {
            AllocConsole();
        }

        int result;
        if (args.Length == 0)
        {
            result = RunInteractiveMode();
        }
        else
        {
            // Remove console argument before processing other args
            args = args.Where(arg => (arg != "--no-console")).ToArray();
            result = await RunSilentMode(args);
        }

        if (showConsole)
        {
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey(true);
            FreeConsole();
        }

        return result;
    }

    static void ShowUsageSamples()
    {        
        Console.WriteLine("Usage samples - commands to be used by you:");
        Console.WriteLine("");
        Console.WriteLine("  Using interactive console:");
        Console.WriteLine("    Usage: nestnet");
        Console.WriteLine();
        Console.WriteLine("  Generate new application:");
        Console.WriteLine("    Usage: nestnet app --force [--no-console]");
        Console.WriteLine();
        Console.WriteLine("  Generate new module (in application):");
        Console.WriteLine("    A generated module contains:");
        Console.WriteLine("      * Sample Entity");
        Console.WriteLine("      * DAO");
        Console.WriteLine("      * DTOs (create/update/result)");
        Console.WriteLine("      * CRUD Service (optional)");
        Console.WriteLine("      * CRUD Controller (optional)");
        Console.WriteLine("    The '--service' and '--controller' define whether or not to generate those layers, defaults are 'true'");
        Console.WriteLine("    Usage: nestnet module --module-name <moduleName> --pluralized-module-name <pluralizedModuleName> [--service true|false] [--controller true|false] [--no-console]");
        Console.WriteLine();
        Console.WriteLine("  Generate new resource (in application):");
        Console.WriteLine("    A generated resource contains:");
        Console.WriteLine("      * Sample Service");
        Console.WriteLine("      * Sample Controller");
        Console.WriteLine("    Both of them without DB support (i.e. without CRUD operations, and without Entity/DAO/DTOs)");
        Console.WriteLine("    Usage: nestnet resource --resource-name <resourceName> [--no-console]");
        Console.WriteLine();
        Console.WriteLine("Usage samples - commands to be generally used by tools:");
        Console.WriteLine("");
        Console.WriteLine("  Generate/update DTOs (in application, for each exists module, accurding its entities, will be run automaticly as post-build command in the IDE):");
        Console.WriteLine("    Usage: nestnet dtos --tar-dir <relativeTarDir> [--no-console]");
        Console.WriteLine();
        Console.WriteLine("  Set the PATH environment variable (to be used by installer):");
        Console.WriteLine("    Usage: nestnet set-path --all-users <1|0> [--target-dir <pathOfTheCliExe>] [--no-console]");
        Console.WriteLine();
        Console.WriteLine("  Remove the PATH environment variable (to be used by installer):");
        Console.WriteLine("    Usage: nestnet remove-path --all-users <1|0> [--target-dir <pathOfTheCliExe>] [--no-console]");
        Console.WriteLine();
    }

    static void ShowVersion()
    {
        var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        Console.WriteLine($"NestNet.Cli version {version}");
    }

    static RootCommand SetupCommands()
    {
        var rootCommand = new RootCommand(@"NestNet CLI tool for generating and managing ASP.NET Core microservices");

        // Override the default usage help
        rootCommand.TreatUnmatchedTokensAsErrors = true;
        rootCommand.Name = "nestnet";

        // Add console option to root command
        var consoleOption = new Option<bool>(
            new[] { "--no-console" },
            "Do not show console window"
        );
        rootCommand.AddGlobalOption(consoleOption);

        SetupAppCommand(rootCommand);
        SetupModuleCommand(rootCommand);
        SetupResourceCommand(rootCommand);
        SetupDtosCommand(rootCommand);
        SetupSetPathCommand(rootCommand);
        SetupRemovePathCommand(rootCommand);
        return rootCommand;
    }

    static void SetupAppCommand(Command rootCommand)
    {
        var appCommand = new Command("app", "Generate new application");
        var forceOption = new Option<bool>("--force", "Force regeneration of folder content");
        appCommand.AddOption(forceOption);
        appCommand.SetHandler((force) =>
        {
            AppGenerator.Run(new AppGenerator.InputParams()
            {
                Force = force
            });
        }, forceOption);
        rootCommand.AddCommand(appCommand);
    }

    static void SetupModuleCommand(Command rootCommand)
    {
        var moduleCommand = new Command("module", "Generate new module");
        
        var moduleNameOption = new Option<string>("--module-name", "Name of the module") { IsRequired = true };
        var pluralizedModuleNameOption = new Option<string>("--pluralized-module-name", "Pluralized name of the module") { IsRequired = true };
        // var dbSupportOption = new Option<bool>("--db-support", () => true, "Generate database support (entity + dao), default - true");
        var serviceOption = new Option<bool>("--service", () => true, "Generate service, default - true" /*"Generate service (requires --db-support)"*/);
        var controllerOption = new Option<bool>("--controller", () => true, "Generate controller, default - true (requires --service)");

        moduleCommand.AddOption(moduleNameOption);
        moduleCommand.AddOption(pluralizedModuleNameOption);
        // moduleCommand.AddOption(dbSupportOption);
        moduleCommand.AddOption(serviceOption);
        moduleCommand.AddOption(controllerOption);

        moduleCommand.SetHandler((moduleName, pluralizedModuleName, /*dbSupport,*/ service, controller) =>
        {
            // Validate layer dependencies
            /*
            if (!dbSupport && service)
            {
                throw new ArgumentException("Service generation requires database support generation to be enabled");
            }
            */
            if (!service && controller)
            {
                throw new ArgumentException("Controller generation requires service generation to be enabled");
            }

            // If a higher layer is disabled, automatically disable dependent layers
            /*
            if (!dbSupport)
            {
                service = false;
                controller = false;
            }
            */
            if (!service)
            {
                controller = false;
            }

            ModuleGenerator.Run(new ModuleGenerator.InputParams
            {
                ModuleName = moduleName,
                PluralizedModuleName = pluralizedModuleName,
                GenerateDbSupport = true, // dbSupport,
                GenerateService = service,
                GenerateController = controller,
            });
        }, moduleNameOption, pluralizedModuleNameOption, /*dbSupportOption,*/ serviceOption, controllerOption);

        rootCommand.AddCommand(moduleCommand);
    }

    static void SetupResourceCommand(Command rootCommand)
    {
        var resourceCommand = new Command("resource", "Generate new resource");
        var resourceNameOption = new Option<string>("--resource-name", "Name of the resource") { IsRequired = true };
        resourceCommand.AddOption(resourceNameOption);
        resourceCommand.SetHandler(resourceName =>
        {
            ResourceGenerator.Run(new ResourceGenerator.InputParams()
            {
                ResourceName = resourceName
            });
        }, resourceNameOption);
        rootCommand.AddCommand(resourceCommand);
    }

    static void SetupDtosCommand(Command rootCommand)
    {
        var dtosCommand = new Command("dtos", "Generate/update DTOs (will be run automaticly as post-build command in the IDE)");
        var tarDirOption = new Option<string>("--tar-dir", "Relative target directory") { IsRequired = true };
        dtosCommand.AddOption(tarDirOption);
        dtosCommand.SetHandler((tarDir) =>
        {
            DtosGenerator.Run(new DtosGenerator.InputParams
            {
                RelativeTarDir = tarDir
            });
        }, tarDirOption);
        rootCommand.AddCommand(dtosCommand);
    }

    static void SetupSetPathCommand(Command rootCommand)
    {
        SetupPathCommand(rootCommand, "set-path", "Set the PATH environment variable (to be used by installer)", PathOperation.Set);
    }

    static void SetupRemovePathCommand(Command rootCommand)
    {
        SetupPathCommand(rootCommand, "remove-path", "Remove the PATH environment variable (to be used by installer)", PathOperation.Remove);
    }

    static void SetupPathCommand(Command rootCommand, string commandName, string description, PathOperation operation)
    {
        var pathCommand = new Command(commandName, description);
        var allUsersOption = new Option<string>("--all-users", "Scope for PATH modification ('1': change the path for all users, '0' or empty: change the path for current user only)") { IsRequired = false };
        var targetDirOption = new Option<string>("--target-dir", "Path of the NestNET.Cli.exe installation (if not specified - path of current running directory will be taken)") { IsRequired = false };

        allUsersOption.AddValidator(result =>
        {
            var value = result.GetValueOrDefault<string>();
            if (!string.IsNullOrEmpty(value) && value != "0" && value != "1")
            {
                result.ErrorMessage = "The --all-users option must be either '0', '1', or empty.";
            }
        });

        pathCommand.AddOption(allUsersOption);
        pathCommand.AddOption(targetDirOption);

        pathCommand.SetHandler((string allUsers, string targetDir) =>
        {
            if (string.IsNullOrWhiteSpace(targetDir))
            {
                targetDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "";
                Console.WriteLine($"Warning: The --target-dir parameter is empty. Using default path: {targetDir}");
            }
            else
            {
                targetDir = targetDir.Replace("\"", ""); // For unknowm reason - using of '--target-dir "[TARGETDIR]"' add '"' at end of this parameter, here we remove it (if exists).
            }
            bool isAllUsers = allUsers == "1";
            PathModifier.ModifyPath(targetDir, isAllUsers, operation);
        }, allUsersOption, targetDirOption);

        rootCommand.AddCommand(pathCommand);
    }

    static async Task<int> RunSilentMode(string[] args)
    {     
        if (args.Contains("--version"))
        {
            ShowVersion();
            return 0;
        }

        args = HandleEmptyAllUsers(args);

        var rootCommand = SetupCommands();
        var result = await rootCommand.InvokeAsync(args);
        if (result != 0 || args.Contains("--help") || args.Contains("-h") || args.Contains("-?"))
        {
            ShowUsageSamples();
        }

        return result;
    }

    /// <summary>
    /// Handle --all-users with empty/null/no value.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    private static string[] HandleEmptyAllUsers(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--all-users")
            {
                if (((i + 1) == args.Length) || args[i + 1].StartsWith("--") || (args[i + 1] == ""))
                {
                    var newArgs = new List<string>(args);
                    if (((i + 1) < args.Length) && (args[i + 1] == ""))
                    {
                        newArgs.RemoveAt(i + 1);
                    }
                    newArgs.Insert(i + 1, "0");
                    args = newArgs.ToArray();
                }
                break;
            }
        }

        return args;
    }

    static int RunInteractiveMode()
    {
        while (true)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]NestNet.Cli[/] - Use arrow keys to select an option and press Enter:")
                    .AddChoices(new[] {
                        "Generate App",
                        "Generate Module",
                        "Generate Resource",
                        "Generate DTOs",
                        "Show Command Line Help",
                        "Exit"
                    }));

            switch (choice)
            {
                case "Generate App":
                    AppGenerator.Run();
                    break;
                case "Generate Module":
                    ModuleGenerator.Run();
                    break;
                case "Generate Resource":
                    ResourceGenerator.Run();
                    break;
                case "Generate DTOs":
                    DtosGenerator.Run();
                    break;
                case "Show Command Line Help":
                    Console.WriteLine();
                    RunSilentMode(new[] { "--help" }).Wait();
                    break;
                case "Exit":
                    return 0;
            }

            if (choice != "Exit")
            {
                AnsiConsole.WriteLine();
                AnsiConsole.WriteLine("Press any key to return to the main menu...");
                Console.ReadKey(true);
            }
        }
    }
}