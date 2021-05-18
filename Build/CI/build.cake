#tool nuget:?package=Tools.InnoSetup
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////
const string commandStr = "command";
const string buildStr = "build";
const string cleanStr ="clean";
const string packStr ="pack";
const string customCommandStr = "Command";


const string solutionPath = "../../QuickDeploy.sln";
const string sourcePath ="../../";
const string setupScriptPath = "../Scripts/Windows/setup.iss";

class CommandProcess
{
    public CommandProcess(string commandName, string shortcut1, string shortcut2,string shortcut3, string description)
    {
        this.CommandName = commandName.Trim();
        this.Shortcut1 = shortcut1.Trim();
        this.Shortcut2 = shortcut2.Trim();
        this.Shortcut3 = shortcut3.Trim();        
        this.Description = description.Trim();
    }

    public string CommandName {get; private set;}
    public string Shortcut1 {get; private set;}
    public string Shortcut2 {get; private set;}
    public string Shortcut3 {get; private set;}
    public string Description {get; private set;}

    public string Shortcuts
    {
        get 
        {
            return Shortcut1 + (Shortcut2 == "" ? "" : "," + Shortcut2) +(Shortcut3 == "" ?  "" : "," + Shortcut3);
        }
    }
}

List<CommandProcess> commandList = new List<CommandProcess>()
{
    new CommandProcess(buildStr,"b", "01" , "1","build the application"),
    new CommandProcess(cleanStr,"c", "" , "","clean obj/bin folders"),
    new CommandProcess(packStr,"pkg", "" , "","package the application")
};

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var command = Argument(commandStr, "");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn(customCommandStr);

Task(buildStr)
    .Does(() =>
{
    Warning("Restoring Nuget Packages:");
    NuGetRestore(solutionPath);

    Warning("Building Source:");
    MSBuild(solutionPath, settings =>
    settings.SetConfiguration("Release")
        .SetVerbosity(Verbosity.Minimal)
        .WithTarget("Build")
        .WithProperty("TreatWarningsAsErrors","true"));   
});


Task(cleanStr)
    .Does(() =>
{
    var path = MakeAbsolute(Directory(sourcePath)).FullPath;
    Warning("Cleaning bin/obj folders from Path " + path);
    CleanDirectories(path + "/**/bin/" + "Debug");
    CleanDirectories(path + "/**/bin/" + "Release");
    CleanDirectories(path + "/**/obj/" + "Debug");   
    CleanDirectories(path + "/**/obj/" + "Release");
});

Task(packStr)
    .IsDependentOn(buildStr)
    .Does(() =>
{
    FilePath innoPath = Context.Tools.Resolve("iscc.exe");
    Warning("Using Innosetup path from - " + innoPath);
    InnoSetup(setupScriptPath, new InnoSetupSettings(){
        ToolPath = innoPath,
    });
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////


Task(customCommandStr)
    .Does(() =>
{
    var command = Argument<string>(commandStr);
    
    if(command == null || command.Trim() == "")
    {
        if (command.Trim().Length > 0)
            Error("Invalid command");
        else
            Error("No Parameter specified");
        ShowUsage();        
        return;
    }

    command = command.Trim().ToLower();

    var item = commandList.FirstOrDefault(x=> x.CommandName.Trim().ToLower() == command || x.Shortcut1.Trim().ToLower() == command || x.Shortcut2.Trim().ToLower() == command|| x.Shortcut3.Trim().ToLower() == command);
    if (item == null)
    {
        Error("Invalid command - " + command);
        ShowUsage();
        return;
    }

    RunTarget(item.CommandName);

})
.OnError(exception =>
{
    Error("Error when executing command - " + exception.Message + " - ST - "+ exception.StackTrace);
});

void ShowUsage()
{
    Warning("=====");
    Warning("Usage");
    Warning("=====");
    Warning("ci.bat <Command>");
    int commandPadding = 20;
    int shortcutPadding = 10;

    var header = "Command".PadRight(commandPadding,' ')+" - "+"Shortcut".PadRight(shortcutPadding,' ')+" - "+"Description";
    var line = "=".PadRight(header.Length + 30,'=');

    Warning(line);
    Warning(header);
    Warning(line);
    foreach(var item in commandList)
    {
        Warning(item.CommandName.PadRight(commandPadding,' ')+" - "+item.Shortcuts.PadRight(shortcutPadding,' ')+" - "+item.Description);
    }
    Warning(line);
}

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);