#addin nuget:?package=Cake.FileHelpers&version=5.0.0

var target = Argument("target", "Build");
var artifactsDir = "artifacts";

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
        ReplaceRegexInFiles(filePath, @"10\.11", "10.15", System.Text.RegularExpressions.RegexOptions.Singleline);

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
    foreach (var filePath in GetFiles(buildDir + "/*"))
    {
        if (filePath.GetFilename().ToString().StartsWith("libSDL2-2.0.so.0."))
        {
            CopyFile(filePath, $"{artifactsDir}/libSDL2-2.0.so.0");
            return;
        }
    }

    throw new Exception("Failed to locate the artifact file of libSDL2-2.0.so :/");
});

Task("Package")
    .Does(() =>
{
    var sdlMajor = FindRegexMatchGroupInFile("sdl/include/SDL_version.h", @"#define SDL_MAJOR_VERSION +(?<ver>\d+)", 1, System.Text.RegularExpressions.RegexOptions.Singleline);
    var sdlMinor = FindRegexMatchGroupInFile("sdl/include/SDL_version.h", @"#define SDL_MINOR_VERSION +(?<ver>\d+)", 1, System.Text.RegularExpressions.RegexOptions.Singleline);
    var sdlPatch = FindRegexMatchGroupInFile("sdl/include/SDL_version.h", @"#define SDL_PATCHLEVEL +(?<ver>\d+)", 1, System.Text.RegularExpressions.RegexOptions.Singleline);
    var sdlVersion = $"{sdlMajor}.{sdlMinor}.{sdlPatch}";
    
    var dnMsBuildSettings = new DotNetMSBuildSettings();
    dnMsBuildSettings.WithProperty("Version", sdlVersion + "." + EnvironmentVariable("GITHUB_RUN_NUMBER"));
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