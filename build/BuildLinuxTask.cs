
namespace BuildScripts;

[TaskName("Build Linux")]
[IsDependentOn(typeof(PrepTask))]
[IsDependeeOf(typeof(BuildLibraryTask))]
public sealed class BuildLinuxTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.IsRunningOnLinux();

    public override void Run(BuildContext context)
    {
        // Build
        var buildDir = "sdl/build";
        context.CreateDirectory(buildDir);
        context.StartProcess("cmake", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "../ -DCMAKE_BUILD_TYPE=Release" });
        context.StartProcess("make", new ProcessSettings { WorkingDirectory = buildDir });

        // Copy artifact
        context.CreateDirectory(context.ArtifactsDir);
        foreach (var filePath in context.GetFiles(buildDir + "/*"))
        {
            if (filePath.GetFilename().ToString().StartsWith("libSDL2-2.0.so.0."))
            {
                context.CopyFile(filePath, $"{context.ArtifactsDir}/libSDL2-2.0.so.0");
                return;
            }
        }

        throw new Exception("Failed to locate the artifact file of libSDL2-2.0.so :/");
    }
}
