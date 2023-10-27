
namespace BuildScripts;

[TaskName("Build macOS")]
public sealed class BuildMacOSTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.IsRunningOnMacOs();

    public override void Run(BuildContext context)
    {
        // Build
        var buildDir = "sdl/build";
        context.CreateDirectory(buildDir);
        context.StartProcess("cmake", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "../ -DCMAKE_OSX_DEPLOYMENT_TARGET=10.15 -DCMAKE_OSX_ARCHITECTURES=arm64;x86_64 -DCMAKE_BUILD_TYPE=Release" });
        context.StartProcess("make", new ProcessSettings { WorkingDirectory = buildDir });

        // Copy artifact
        context.CreateDirectory(context.ArtifactsDir);
        context.CopyFile("sdl/build/libSDL2-2.0.0.dylib", $"{context.ArtifactsDir}/libSDL2-2.0.0.dylib");
    }
}
