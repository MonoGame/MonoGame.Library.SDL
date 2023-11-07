
namespace BuildScripts;

public static class Program
{
    public static int Main(string[] args)
        => new CakeHost()
            .AddAssembly(typeof(BuildContext).Assembly)
            .UseWorkingDirectory("../")
            .UseContext<BuildContext>()
            .Run(args);
}
