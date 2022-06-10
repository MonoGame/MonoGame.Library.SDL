#addin nuget:?package=Cake.FileHelpers&version=5.0.0

var target = Argument("target", "Build");
var artifactsDir = "artifacts";
var version = "2.0.22";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("BuildWindows")
    .WithCriteria(() => IsRunningOnWindows())
    .Does(() =>
{
    // Build
    var buildDir = "sdl/build";
    CreateDirectory(buildDir);
    StartProcess("cmake", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "-A x64 ../" });
    StartProcess("msbuild", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "SDL2.sln /p:Configuration=Release" });

    // Copy artifact
    CreateDirectory(artifactsDir);
    CopyFile("sdl/build/Release/SDL2.dll", $"{artifactsDir}/SDL2.dll");
});

Task("BuildMacOS")
    .WithCriteria(() => IsRunningOnMacOs())
    .Does(() =>
{
    // Set new minimum target to 10.15
    var filePaths = new[] { "sdl/build-scripts/clang-fat.sh" };

    foreach (var filePath in filePaths)
        ReplaceRegexInFiles(filePath, @"10\.6", "10.15", System.Text.RegularExpressions.RegexOptions.Singleline);

    // Build
    var buildDir = "sdl/build";
    CreateDirectory(buildDir);
    StartProcess("sdl/configure", new ProcessSettings {
        WorkingDirectory = buildDir,
        EnvironmentVariables = new Dictionary<string, string>{
            { "CC", "../build-scripts/clang-fat.sh" }
        }
    });
    StartProcess("make", new ProcessSettings { WorkingDirectory = buildDir });

    // Copy artifact
    CreateDirectory(artifactsDir);
    CopyFile("sdl/build/build/.libs/libSDL2-2.0.0.dylib", $"{artifactsDir}/libSDL2.dylib");
});

Task("BuildLinux")
    .WithCriteria(() => IsRunningOnLinux())
    .Does(() =>
{
    // Build
    var buildDir = "sdl/build";
    CreateDirectory(buildDir);
    StartProcess("cmake", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "../ -DCMAKE_BUILD_TYPE=Release" });
    StartProcess("make", new ProcessSettings { WorkingDirectory = buildDir });

    // Copy artifact
    CreateDirectory(artifactsDir);
    var versionSplit = version.Split('.');
    CopyFile($"sdl/build/libSDL2-2.0.so.{versionSplit[1]}.{versionSplit[2]}.0", $"{artifactsDir}/libSDL2-2.0.so.0");
});

Task("Package")
    .Does(() =>
{
    var dnMsBuildSettings = new DotNetMSBuildSettings();
    dnMsBuildSettings.WithProperty("Version", version + "." + EnvironmentVariable("GITHUB_RUN_NUMBER"));
    dnMsBuildSettings.WithProperty("RepositoryUrl", "https://github.com/" + EnvironmentVariable("GITHUB_REPOSITORY"));

    var dnPackSettings = new DotNetPackSettings();
    dnPackSettings.MSBuildSettings = dnMsBuildSettings;
    dnPackSettings.Verbosity = DotNetVerbosity.Minimal;
    dnPackSettings.Configuration = "Release";   

    DotNetPack("MonoGame.Library.SDL.csproj", dnPackSettings);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Build")
    .IsDependentOn("BuildWindows")
    .IsDependentOn("BuildMacOS")
    .IsDependentOn("BuildLinux");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);