
namespace BuildScripts;

public static class Program
{
    public static int Main(string[] args)
        => new CakeHost()
            .UseWorkingDirectory("../")
            .UseContext<BuildContext>()
            .Run(args);
}

public class BuildContext : FrostingContext
{
    public string ArtifactsDir { get; }

    public BuildContext(ICakeContext context) : base(context)
    {
        ArtifactsDir = context.Arguments("artifactsDir", "artifacts").FirstOrDefault();
    }
}

[TaskName("Default")]
[IsDependentOn(typeof(BuildWindowsTask))]
[IsDependentOn(typeof(BuildMacOSTask))]
[IsDependentOn(typeof(BuildLinuxTask))]
public class DefaultTask : FrostingTask
{
}
