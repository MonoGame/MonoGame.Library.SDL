
namespace BuildScripts;

[TaskName("Build macOS")]
[IsDependentOn(typeof(PrepTask))]
[IsDependeeOf(typeof(BuildLibraryTask))]
public sealed class BuildMacOSTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.IsRunningOnMacOs();

    public override void Run(BuildContext context)
    {
        // Prepare artifacts
        context.CreateDirectory($"{context.ArtifactsDir}");

        // Build sdl2-compat
        var buildDir = "sdl2-compat/cmake-build";
        context.CreateDirectory(buildDir);
        context.StartProcess("cmake", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "-S ../ -DCMAKE_OSX_DEPLOYMENT_TARGET=10.15 -DCMAKE_OSX_ARCHITECTURES=arm64;x86_64 -DCMAKE_BUILD_TYPE=Release -DSDL3_INCLUDE_DIRS=../../sdl3/include" });
        context.StartProcess("make", new ProcessSettings { WorkingDirectory = buildDir });
        // libSDL2-2.0.dylib -> libSDL2-2.0.0.dylib
        context.CopyFile($"{buildDir}/libSDL2-2.0.0.dylib", $"{context.ArtifactsDir}/libSDL2-2.0.0.dylib");

        // Build sdl3
        buildDir = "sdl3/build";
        context.CreateDirectory(buildDir);
        context.StartProcess("cmake", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "-S ../ -DCMAKE_OSX_DEPLOYMENT_TARGET=10.15 -DCMAKE_OSX_ARCHITECTURES=arm64;x86_64 -DCMAKE_BUILD_TYPE=Release" });
        context.StartProcess("make", new ProcessSettings { WorkingDirectory = buildDir });
        // libSDL3.dylib -> libSDL3.0.dylib
        context.CopyFile($"{buildDir}/libSDL3.0.dylib", $"{context.ArtifactsDir}/libSDL3.0.dylib");
    }
}
