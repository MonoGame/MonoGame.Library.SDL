
namespace BuildScripts;

[TaskName("Build Linux")]
[IsDependentOn(typeof(PrepTask))]
[IsDependeeOf(typeof(BuildLibraryTask))]
public sealed class BuildLinuxTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.IsRunningOnLinux();

    public override void Run(BuildContext context)
    {
        // Prepare artifacts
        context.CreateDirectory($"{context.ArtifactsDir}");

        // Build sdl2-compat
        var buildDir = "sdl2-compat/cmake-build";
        context.CreateDirectory(buildDir);
        context.StartProcess("cmake", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "-S ../ -DCMAKE_BUILD_TYPE=Release -DSDL3_INCLUDE_DIRS=../../sdl3/include" });
        context.StartProcess("make", new ProcessSettings { WorkingDirectory = buildDir });
        //libSDL2-2.0.so -> libSDL2-2.0.so.0
        //libSDL2-2.0.so.0 -> libSDL2-2.0.so.0.xxxx.xx
        var copied = false;
        foreach (var filePath in context.GetFiles(buildDir + "/*"))
        {
            if (filePath.GetFilename().ToString().StartsWith("libSDL2-2.0.so.0."))
            {
                context.CopyFile(filePath, $"{context.ArtifactsDir}/libSDL2-2.0.so.0");
                copied = true;
                break;
            }
        }
        if(!copied)
            throw new Exception("Failed to locate the artifact file of libSDL2-2.0.so :/");

        // Build sdl3
        buildDir = "sdl3/build";
        context.CreateDirectory(buildDir);
        context.StartProcess("cmake", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "-S ../ -DCMAKE_BUILD_TYPE=Release" });
        context.StartProcess("make", new ProcessSettings { WorkingDirectory = buildDir });
        // libSDL3.so -> libSDL3.so.0
        // libSDL3.so.0 -> libSDL3.so.0.2.10
        // libSDL3.so.0.2.10
        context.CopyFile($"{buildDir}/libSDL3.so.0.2.10", $"{context.ArtifactsDir}/libSDL3.so.0.2.10");
    }
}