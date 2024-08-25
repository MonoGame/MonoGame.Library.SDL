
namespace BuildScripts;

[TaskName("Build Windows")]
[IsDependentOn(typeof(PrepTask))]
[IsDependeeOf(typeof(BuildLibraryTask))]
public sealed class BuildWindowsTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.IsRunningOnWindows();

    public override void Run(BuildContext context)
    {
        // Build
        var buildDir = "sdl/build";
        context.CreateDirectory(buildDir);
        context.StartProcess("cmake", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "-A x64 -D CMAKE_MSVC_RUNTIME_LIBRARY=MultiThreaded ../" });
        context.StartProcess("msbuild", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "SDL2.sln /p:Configuration=Release /p:Platform=x64" });

        // Copy artifact
        context.CreateDirectory(context.ArtifactsDir);
        context.CopyFile("sdl/build/Release/SDL2.dll", $"{context.ArtifactsDir}/SDL2.dll");
        context.CopyFile("sdl/build/Release/SDL2-static.lib", $"{context.ArtifactsDir}/SDL2-static.lib");
    }
}
