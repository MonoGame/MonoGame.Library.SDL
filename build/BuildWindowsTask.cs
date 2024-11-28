
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
        var buildDir = "sdl/build_x64";
        context.CreateDirectory(buildDir);
        context.CreateDirectory($"{context.ArtifactsDir}/win-x64");
        context.StartProcess("cmake", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "-A x64 -D CMAKE_MSVC_RUNTIME_LIBRARY=MultiThreaded ../" });
        context.StartProcess("msbuild", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "SDL2.sln /p:Configuration=Release" });

        // Copy artifact
        context.CreateDirectory(context.ArtifactsDir);
        context.CopyFile("sdl/build_x64/Release/SDL2.dll", $"{context.ArtifactsDir}/win-x64/SDL2.dll");

        buildDir = "sdl/build_arm64";
        context.CreateDirectory(buildDir);
        context.CreateDirectory($"{context.ArtifactsDir}/win-arm64");
        context.StartProcess("cmake", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "-A ARM64 -D CMAKE_MSVC_RUNTIME_LIBRARY=MultiThreaded ../" });
        context.StartProcess("msbuild", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "SDL2.sln /p:Configuration=Release" });

        // Copy artifact
        context.CreateDirectory(context.ArtifactsDir);
        context.CopyFile("sdl/build_arm64/Release/SDL2.dll", $"{context.ArtifactsDir}/win-arm64/SDL2.dll");
    }
}
