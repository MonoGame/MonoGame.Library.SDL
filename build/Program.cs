
namespace BuildScripts;

public static class Program
{
    public static int Main(string[] args)
        => new CakeHost()
            .UseWorkingDirectory("../")
            .UseContext<BuildContext>()
            .Run(args);

    public static string GetArgument(this ICakeArguments args, string argName, string defaultArgValue)
        => args.HasArgument(argName) ? args.GetArgument(argName) : defaultArgValue;
}

public class BuildContext : FrostingContext
{
    public string ArtifactsDir { get; }

    public BuildContext(ICakeContext context) : base(context)
    {
        ArtifactsDir = context.Arguments.GetArgument("artifactsDir", "artifacts");
    }
}

[TaskName("Default")]
[IsDependentOn(typeof(BuildWindowsTask))]
[IsDependentOn(typeof(BuildMacOSTask))]
[IsDependentOn(typeof(BuildLinuxTask))]
public class DefaultTask : FrostingTask
{
}
