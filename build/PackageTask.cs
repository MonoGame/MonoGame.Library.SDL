
namespace BuildScripts;

[TaskName("Package")]
public sealed class PackageTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        var sdlMajor = context.FindRegexMatchGroupInFile("sdl/include/SDL_version.h", @"#define SDL_MAJOR_VERSION +(?<ver>\d+)", 1, System.Text.RegularExpressions.RegexOptions.Singleline);
        var sdlMinor = context.FindRegexMatchGroupInFile("sdl/include/SDL_version.h", @"#define SDL_MINOR_VERSION +(?<ver>\d+)", 1, System.Text.RegularExpressions.RegexOptions.Singleline);
        var sdlPatch = context.FindRegexMatchGroupInFile("sdl/include/SDL_version.h", @"#define SDL_PATCHLEVEL +(?<ver>\d+)", 1, System.Text.RegularExpressions.RegexOptions.Singleline);
        var sdlVersion = $"{sdlMajor}.{sdlMinor}.{sdlPatch}";
        var dnMsBuildSettings = new DotNetMSBuildSettings();
        dnMsBuildSettings.WithProperty("Version", sdlVersion + "." + context.EnvironmentVariable("GITHUB_RUN_NUMBER"));
        dnMsBuildSettings.WithProperty("RepositoryUrl", "https://github.com/" + context.EnvironmentVariable("GITHUB_REPOSITORY"));

        context.DotNetPack("src/MonoGame.Library.SDL.csproj", new DotNetPackSettings
        {
            MSBuildSettings = dnMsBuildSettings,
            Verbosity = DotNetVerbosity.Minimal,
            Configuration = "Release"
        });
    }
}
