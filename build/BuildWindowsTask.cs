
namespace BuildScripts;

[TaskName("Build Windows")]
public sealed class BuildWindowsTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.IsRunningOnWindows();

    public override void Run(BuildContext context)
    {
        // Build
        var buildDir = "sdl/build";
        context.CreateDirectory(buildDir);
        context.StartProcess("cmake", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "-A x64 ../" });
        context.StartProcess("msbuild", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "SDL2.sln /p:Configuration=Release" });

        // Copy artifact
        context.CreateDirectory(context.ArtifactsDir);
        context.CopyFile("sdl/build/Release/SDL2.dll", $"{context.ArtifactsDir}/SDL2.dll");
    }
}
